using System;
using System.Collections.Generic;
using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Attached to entities that respond to gravity (player, ship, physics objects).
    /// Queries GravityManager each FixedUpdate for accumulated gravity from all sources.
    /// Does NOT apply forces directly - that's the consumer's responsibility.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravitySolver : MonoBehaviour, IGravityAffected
    {
        // === Inspector Fields ===
        [Header("Gravity Settings")]
        [SerializeField]
        [Tooltip("Whether gravity is enabled for this entity.")]
        private bool _gravityEnabled = true;

        [SerializeField]
        [Tooltip("Multiplier for gravity strength. Use for gameplay tuning.")]
        private float _gravityScale = 1f;

        [SerializeField]
        [Tooltip("Gravity magnitude below this threshold is clamped to zero (emergent Lagrange points).")]
        private float _zeroGThreshold = 0.25f;

        [Header("Orientation Blending")]
        [SerializeField]
        [Tooltip("Speed at which orientation blends when gravity direction changes (degrees/second).")]
        private float _orientationBlendSpeed = 90f;

        [SerializeField]
        [Tooltip("Maximum rotation speed to prevent nausea (degrees/second). 0 = unlimited.")]
        private float _maxRotationSpeed = 180f;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Draw gravity direction gizmo in editor.")]
        private bool _drawGizmos = true;

        // === Public Properties (IGravityAffected) ===
        public Vector3 CurrentGravity => _currentGravity;
        public IGravitySource DominantSource => _dominantSource;

        public bool GravityEnabled
        {
            get => _gravityEnabled;
            set => _gravityEnabled = value;
        }

        public Vector3 LocalUp
        {
            get
            {
                if (_smoothedUp.sqrMagnitude < 0.001f)
                    return Vector3.up;
                return _smoothedUp;
            }
        }

        /// <summary>
        /// Raw (unsmoothed) local up direction based on current gravity.
        /// Use LocalUp for smoothed orientation in most cases.
        /// </summary>
        public Vector3 RawLocalUp
        {
            get
            {
                if (_currentGravity.sqrMagnitude < 0.001f)
                    return Vector3.up;
                return -_currentGravity.normalized;
            }
        }

        /// <summary>
        /// Gravity scale multiplier for gameplay tuning.
        /// </summary>
        public float GravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        /// <summary>
        /// True when accumulated gravity magnitude is below zero-g threshold.
        /// Useful for triggering float behavior or zero-g UI indicators.
        /// </summary>
        public bool IsInZeroG => _currentGravity.magnitude < _zeroGThreshold;

        /// <summary>
        /// Threshold below which gravity is considered zero (m/s²).
        /// </summary>
        public float ZeroGThreshold
        {
            get => _zeroGThreshold;
            set => _zeroGThreshold = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Orientation blend speed in degrees/second.
        /// </summary>
        public float OrientationBlendSpeed
        {
            get => _orientationBlendSpeed;
            set => _orientationBlendSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Maximum rotation speed in degrees/second (0 = unlimited).
        /// Prevents nausea-inducing rapid rotations.
        /// </summary>
        public float MaxRotationSpeed
        {
            get => _maxRotationSpeed;
            set => _maxRotationSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// List of all gravity contributions at current position.
        /// Updated each FixedUpdate. Useful for debugging and UI.
        /// </summary>
        public IReadOnlyList<GravityContribution> GravityContributions => _contributions;

        // === Events ===
        /// <summary>
        /// Fired when entering a zero-g zone (gravity drops below threshold).
        /// </summary>
        public event Action OnZeroGEntered;

        /// <summary>
        /// Fired when exiting a zero-g zone (gravity rises above threshold).
        /// </summary>
        public event Action OnZeroGExited;

        /// <summary>
        /// Fired when the dominant gravity source changes.
        /// Parameters: (previous source, new source) - either can be null.
        /// </summary>
        public event Action<IGravitySource, IGravitySource> OnDominantSourceChanged;

        // === Private Fields ===
        private Vector3 _currentGravity;
        private Vector3 _smoothedUp = Vector3.up;
        private Vector3 _targetUp = Vector3.up;
        private IGravitySource _dominantSource;
        private IGravitySource _previousDominantSource;
        private bool _wasInZeroG;
        private Rigidbody _rb;
        private List<GravityContribution> _contributions = new List<GravityContribution>();

        // === Unity Lifecycle ===
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            // Disable Unity's built-in gravity - we handle it
            _rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            UpdateGravity();
            UpdateSmoothedOrientation();
            CheckZeroGTransition();
            CheckDominantSourceChange();
        }

        // === Public Methods ===

        /// <summary>
        /// Apply the current gravity as a force to the Rigidbody.
        /// Call this if you want automatic gravity application.
        /// </summary>
        public void ApplyGravityForce()
        {
            if (_gravityEnabled && _currentGravity.sqrMagnitude > 0.001f)
            {
                _rb.AddForce(_currentGravity, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Force an immediate gravity recalculation.
        /// Normally called automatically in FixedUpdate.
        /// </summary>
        public void UpdateGravity()
        {
            if (!_gravityEnabled)
            {
                _currentGravity = Vector3.zero;
                _dominantSource = null;
                _contributions.Clear();
                return;
            }

            var manager = GravityManager.Instance;
            if (manager == null)
            {
                _currentGravity = Vector3.zero;
                _dominantSource = null;
                _contributions.Clear();
                return;
            }

            // Get accumulated gravity from all sources
            _currentGravity = manager.GetAccumulatedGravity(transform.position) * _gravityScale;
            
            // Clamp to zero if below threshold - creates emergent Lagrange points
            // where gravity sources naturally cancel out
            if (_currentGravity.magnitude < _zeroGThreshold)
            {
                _currentGravity = Vector3.zero;
            }

            // Track previous dominant source before updating
            _previousDominantSource = _dominantSource;

            // Get dominant source for orientation (strongest contributor)
            _dominantSource = manager.GetDominantSource(transform.position);

            // Update contributions list for debugging/UI
            var newContributions = manager.GetAllContributors(transform.position);
            _contributions.Clear();
            _contributions.AddRange(newContributions);
        }

        /// <summary>
        /// Smoothly interpolate orientation toward target up direction.
        /// Uses configurable blend speed with max rotation rate limiting.
        /// </summary>
        private void UpdateSmoothedOrientation()
        {
            // Calculate target up from current gravity
            _targetUp = RawLocalUp;

            // Skip blending if already aligned
            float angleDiff = Vector3.Angle(_smoothedUp, _targetUp);
            if (angleDiff < 0.01f)
            {
                _smoothedUp = _targetUp;
                return;
            }

            // Calculate blend factor
            // Use degrees/second converted to a 0-1 blend factor for Slerp
            float maxAngleThisFrame = _orientationBlendSpeed * Time.fixedDeltaTime;

            // Apply max rotation speed limit if set
            if (_maxRotationSpeed > 0f)
            {
                maxAngleThisFrame = Mathf.Min(maxAngleThisFrame, _maxRotationSpeed * Time.fixedDeltaTime);
            }

            // Calculate blend t based on angle remaining vs max angle this frame
            float blendT = Mathf.Clamp01(maxAngleThisFrame / angleDiff);

            // Handle near-180° flip: choose a consistent rotation axis
            if (angleDiff > 170f)
            {
                // Find a perpendicular axis for rotation (avoid gimbal issues)
                Vector3 rotationAxis = Vector3.Cross(_smoothedUp, _targetUp);
                if (rotationAxis.sqrMagnitude < 0.001f)
                {
                    // Vectors are parallel or anti-parallel, pick arbitrary perpendicular
                    rotationAxis = Vector3.Cross(_smoothedUp, Vector3.right);
                    if (rotationAxis.sqrMagnitude < 0.001f)
                    {
                        rotationAxis = Vector3.Cross(_smoothedUp, Vector3.forward);
                    }
                }
                rotationAxis.Normalize();

                // Rotate around the axis by max angle
                Quaternion rotation = Quaternion.AngleAxis(maxAngleThisFrame, rotationAxis);
                _smoothedUp = (rotation * _smoothedUp).normalized;
            }
            else
            {
                // Normal Slerp for smaller angles
                _smoothedUp = Vector3.Slerp(_smoothedUp, _targetUp, blendT).normalized;
            }
        }

        /// <summary>
        /// Check for zero-G state transitions and fire events.
        /// </summary>
        private void CheckZeroGTransition()
        {
            bool isNowInZeroG = IsInZeroG;

            if (isNowInZeroG && !_wasInZeroG)
            {
                // Entered zero-G
                OnZeroGEntered?.Invoke();
            }
            else if (!isNowInZeroG && _wasInZeroG)
            {
                // Exited zero-G
                OnZeroGExited?.Invoke();
            }

            _wasInZeroG = isNowInZeroG;
        }

        /// <summary>
        /// Check for dominant source changes and fire events.
        /// </summary>
        private void CheckDominantSourceChange()
        {
            if (_dominantSource != _previousDominantSource)
            {
                OnDominantSourceChanged?.Invoke(_previousDominantSource, _dominantSource);
            }
        }

        /// <summary>
        /// Get distance to the dominant gravity source's center.
        /// Returns -1 if no dominant source.
        /// </summary>
        public float GetDistanceToGravityCenter()
        {
            if (_dominantSource == null)
                return -1f;
            return Vector3.Distance(transform.position, _dominantSource.GravityCenter);
        }

        /// <summary>
        /// Get distance to the surface of the dominant gravity source.
        /// Assumes GravityBody component for surface radius.
        /// Returns -1 if no dominant source or source isn't a GravityBody.
        /// </summary>
        public float GetDistanceToSurface()
        {
            if (_dominantSource is GravityBody body)
            {
                float distanceToCenter = Vector3.Distance(transform.position, body.GravityCenter);
                return distanceToCenter - body.SurfaceRadius;
            }
            return -1f;
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawGizmos || !Application.isPlaying)
                return;

            if (_currentGravity.sqrMagnitude > 0.001f)
            {
                // Draw combined gravity direction (yellow)
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, _currentGravity.normalized * 2f);

                // Draw raw local up (orange, thin)
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                Gizmos.DrawRay(transform.position, RawLocalUp * 1.5f);

                // Draw smoothed local up (green, prominent)
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, LocalUp * 2f);

                // Draw individual contributions if multiple sources
                if (_contributions.Count > 1)
                {
                    foreach (var contrib in _contributions)
                    {
                        // Color based on influence (more influence = more opaque cyan)
                        Gizmos.color = new Color(0f, 1f, 1f, contrib.InfluencePercent);
                        Gizmos.DrawRay(transform.position, contrib.Direction * contrib.Magnitude * 0.2f);
                    }
                }
            }
            else if (IsInZeroG)
            {
                // Draw zero-g indicator (pulsing magenta sphere)
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
#endif
    }
}
