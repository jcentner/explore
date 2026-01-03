using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Attached to entities that respond to gravity (player, ship, physics objects).
    /// Queries GravityManager each FixedUpdate and exposes gravity for consumers.
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

        // === Private Fields ===
        private Vector3 _currentGravity;
        private IGravitySource _dominantSource;
        private Rigidbody _rb;

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
                return;
            }

            var manager = GravityManager.Instance;
            if (manager == null)
            {
                _currentGravity = Vector3.zero;
                _dominantSource = null;
                return;
            }

            _dominantSource = manager.GetDominantSource(transform.position);

            if (_dominantSource != null)
            {
                _currentGravity = _dominantSource.CalculateGravity(transform.position) * _gravityScale;
            }
            else
            {
                _currentGravity = Vector3.zero;
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
                // Draw gravity direction
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, _currentGravity.normalized * 2f);

                // Draw local up
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, LocalUp * 2f);
            }
        }
#endif
    }
}
