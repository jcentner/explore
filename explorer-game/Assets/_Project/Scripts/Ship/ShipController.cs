using UnityEngine;
using Explorer.Gravity;

namespace Explorer.Ship
{
    /// <summary>
    /// 6DOF ship flight controller with physics-based movement.
    /// Handles thrust, rotation, braking, and gravity integration.
    /// Input is provided externally via Set* methods.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Thrust")]
        [SerializeField, Tooltip("Base thrust acceleration (m/s²)")]
        private float _thrustForce = 20f;
        
        [SerializeField, Tooltip("Multiplier when boosting")]
        private float _boostMultiplier = 2f;
        
        [SerializeField, Tooltip("Deceleration when braking (m/s²)")]
        private float _brakeForce = 15f;
        
        [Header("Rotation")]
        [SerializeField, Tooltip("Pitch speed (degrees/sec)")]
        private float _pitchSpeed = 90f;
        
        [SerializeField, Tooltip("Yaw speed (degrees/sec)")]
        private float _yawSpeed = 90f;
        
        [SerializeField, Tooltip("Roll speed (degrees/sec)")]
        private float _rollSpeed = 120f;
        
        [SerializeField, Tooltip("How quickly rotation responds (higher = snappier)")]
        private float _rotationResponse = 5f;
        
        [Header("Gravity")]
        [SerializeField, Tooltip("Should ship respond to gravity fields?")]
        private bool _respondToGravity = true;
        
        [SerializeField, Range(0f, 2f), Tooltip("Gravity effect multiplier (0 = immune, 1 = full, 2 = double)")]
        private float _gravityMultiplier = 0.5f;
        
        [Header("Landing")]
        [SerializeField, Range(0.1f, 5f), Tooltip("Velocity threshold to be considered 'landed' (not just touching)")]
        private float _landedVelocityThreshold = 0.5f;
        
        // === Events ===
        /// <summary>Fired when ship lands (grounded and nearly stationary).</summary>
        public event System.Action OnLanded;
        
        /// <summary>Fired when ship takes off (was landed, now moving).</summary>
        public event System.Action OnTakeoff;
        
        // === Public Properties ===
        /// <summary>Current velocity in world space (m/s)</summary>
        public Vector3 Velocity => _rb != null ? _rb.linearVelocity : Vector3.zero;
        
        /// <summary>Current speed magnitude (m/s)</summary>
        public float Speed => Velocity.magnitude;
        
        /// <summary>True if ship is touching any collider</summary>
        public bool IsGrounded => _collisionCount > 0;
        
        /// <summary>True if ship is grounded and nearly stationary (safe to disembark)</summary>
        public bool IsLanded => IsGrounded && Speed < _landedVelocityThreshold;
        
        /// <summary>True if brake is currently active</summary>
        public bool IsBraking => _isBraking;
        
        /// <summary>True if boost is currently active</summary>
        public bool IsBoosting => _isBoosting;
        
        // === Private Fields ===
        private Rigidbody _rb;
        private GravitySolver _gravitySolver;
        
        private Vector3 _thrustInput;      // Local-space: x=strafe, y=vertical, z=forward
        private Vector3 _rotationInput;    // x=pitch, y=yaw, z=roll (-1 to 1)
        private bool _isBoosting;
        private bool _isBraking;
        
        private Quaternion _targetRotation;
        private int _collisionCount;
        private bool _hasRotationInput;    // Track if rotation was requested this frame
        private Vector3 _smoothedRotationInput; // Smoothed rotation for responsive feel
        private bool _wasLanded;           // Track landed state for events
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _gravitySolver = GetComponent<GravitySolver>();
            
            ConfigureRigidbody();
            _targetRotation = transform.rotation;
        }
        
        private void FixedUpdate()
        {
            ApplyThrust();
            ApplyRotation();
            ApplyBrake();
            ApplyGravity();
            CheckLandingState();
        }
        
        private void CheckLandingState()
        {
            bool isLandedNow = IsLanded;
            
            if (isLandedNow && !_wasLanded)
            {
                OnLanded?.Invoke();
            }
            else if (!isLandedNow && _wasLanded)
            {
                OnTakeoff?.Invoke();
            }
            
            _wasLanded = isLandedNow;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            _collisionCount++;
        }
        
        private void OnCollisionExit(Collision collision)
        {
            _collisionCount = Mathf.Max(0, _collisionCount - 1);
        }
        
        // === Public Methods ===
        /// <summary>
        /// Set thrust input in local space.
        /// </summary>
        /// <param name="input">x=strafe, y=vertical, z=forward. Each axis -1 to 1.</param>
        public void SetThrustInput(Vector3 input)
        {
            _thrustInput = Vector3.ClampMagnitude(input, 1f);
        }
        
        /// <summary>
        /// Set rotation input.
        /// </summary>
        /// <param name="input">x=pitch, y=yaw, z=roll. Each axis -1 to 1.</param>
        public void SetRotationInput(Vector3 input)
        {
            _rotationInput = new Vector3(
                Mathf.Clamp(input.x, -1f, 1f),
                Mathf.Clamp(input.y, -1f, 1f),
                Mathf.Clamp(input.z, -1f, 1f)
            );
            // Mark that we have input if any axis is non-trivial
            _hasRotationInput = _rotationInput.sqrMagnitude > 0.0001f;
        }
        
        /// <summary>
        /// Enable or disable boost mode.
        /// </summary>
        public void SetBoost(bool active)
        {
            _isBoosting = active;
        }
        
        /// <summary>
        /// Enable or disable braking.
        /// When braking, ship decelerates toward zero velocity.
        /// </summary>
        public void SetBrake(bool active)
        {
            _isBraking = active;
        }
        
        /// <summary>
        /// Immediately stop all motion. Use for testing or special events.
        /// </summary>
        public void FullStop()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        // === Private Methods ===
        private void ConfigureRigidbody()
        {
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            
            // Keep Inspector values for damping (useful for ground friction feel)
            // _rb.linearDamping and _rb.angularDamping set in Inspector
            
            // Use automatic center of mass from collider geometry
            _rb.automaticCenterOfMass = true;
        }
        
        private void ApplyThrust()
        {
            if (_thrustInput.sqrMagnitude < 0.001f) return;
            
            // Convert local input to world direction
            Vector3 worldThrust = transform.TransformDirection(_thrustInput);
            
            // Calculate force magnitude
            float force = _thrustForce;
            if (_isBoosting)
            {
                force *= _boostMultiplier;
            }
            
            // Apply as acceleration (ignores mass for consistent feel)
            _rb.AddForce(worldThrust * force, ForceMode.Acceleration);
        }
        
        private void ApplyRotation()
        {
            // Smooth the input for responsive but not jerky movement
            _smoothedRotationInput = Vector3.Lerp(
                _smoothedRotationInput,
                _hasRotationInput ? _rotationInput : Vector3.zero,
                Time.fixedDeltaTime * _rotationResponse
            );
            
            // If effectively zero input, let physics handle it (allows settling when landed)
            if (_smoothedRotationInput.sqrMagnitude < 0.0001f)
            {
                return;
            }
            
            // Calculate desired angular velocity in local space (degrees/sec -> rad/sec)
            Vector3 desiredAngularVelocity = new Vector3(
                -_smoothedRotationInput.x * _pitchSpeed,  // Pitch around local X
                _smoothedRotationInput.y * _yawSpeed,      // Yaw around local Y  
                -_smoothedRotationInput.z * _rollSpeed     // Roll around local Z
            ) * Mathf.Deg2Rad;
            
            // Convert to world space
            Vector3 worldAngularVelocity = transform.TransformDirection(desiredAngularVelocity);
            
            // Directly set angular velocity for immediate, smooth response
            _rb.angularVelocity = worldAngularVelocity;
        }
        
        private void ApplyBrake()
        {
            if (!_isBraking) return;
            
            Vector3 velocity = _rb.linearVelocity;
            if (velocity.sqrMagnitude < 0.01f)
            {
                // Close enough to stopped, zero out
                _rb.linearVelocity = Vector3.zero;
                return;
            }
            
            // Apply counter-force opposite to current velocity
            Vector3 brakeDirection = -velocity.normalized;
            float brakeAmount = Mathf.Min(_brakeForce * Time.fixedDeltaTime, velocity.magnitude);
            
            _rb.linearVelocity = velocity + brakeDirection * brakeAmount;
        }
        
        private void ApplyGravity()
        {
            if (!_respondToGravity) return;
            if (_gravitySolver == null) return;
            if (!_gravitySolver.GravityEnabled) return;
            
            // Get gravity from GravitySolver and apply with multiplier
            Vector3 gravity = _gravitySolver.CurrentGravity * _gravityMultiplier;
            _rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }
}
