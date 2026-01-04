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
        private float _pitchSpeed = 60f;
        
        [SerializeField, Tooltip("Yaw speed (degrees/sec)")]
        private float _yawSpeed = 45f;
        
        [SerializeField, Tooltip("Roll speed (degrees/sec)")]
        private float _rollSpeed = 90f;
        
        [SerializeField, Tooltip("Rotation smoothing factor")]
        private float _rotationSmoothSpeed = 10f;
        
        [Header("Gravity")]
        [SerializeField, Tooltip("Should ship respond to gravity fields?")]
        private bool _respondToGravity = true;
        
        [SerializeField, Tooltip("Gravity effect multiplier (0 = immune, 1 = full)")]
        private float _gravityMultiplier = 0.5f;
        
        // === Public Properties ===
        /// <summary>Current velocity in world space (m/s)</summary>
        public Vector3 Velocity => _rb != null ? _rb.linearVelocity : Vector3.zero;
        
        /// <summary>Current speed magnitude (m/s)</summary>
        public float Speed => Velocity.magnitude;
        
        /// <summary>True if ship is touching any collider</summary>
        public bool IsGrounded => _collisionCount > 0;
        
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
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _gravitySolver = GetComponent<GravitySolver>();
            
            ConfigureRigidbody();
            _targetRotation = transform.rotation;
        }
        
        private void Update()
        {
            // Calculate target rotation from input (smoother in Update)
            UpdateTargetRotation();
        }
        
        private void FixedUpdate()
        {
            ApplyThrust();
            ApplyRotation();
            ApplyBrake();
            ApplyGravity();
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
            
            // Zero damping for space physics (no air resistance)
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
            
            // Center of mass at origin for stable rotation
            _rb.centerOfMass = Vector3.zero;
        }
        
        private void UpdateTargetRotation()
        {
            if (_rotationInput.sqrMagnitude < 0.001f) return;
            
            float dt = Time.deltaTime;
            
            // Build rotation deltas using Quaternion composition
            // Pitch around local right axis
            var pitchDelta = Quaternion.AngleAxis(
                -_rotationInput.x * _pitchSpeed * dt,
                transform.right
            );
            
            // Yaw around local up axis
            var yawDelta = Quaternion.AngleAxis(
                _rotationInput.y * _yawSpeed * dt,
                transform.up
            );
            
            // Roll around local forward axis
            var rollDelta = Quaternion.AngleAxis(
                -_rotationInput.z * _rollSpeed * dt,
                transform.forward
            );
            
            // Compose rotations: yaw * pitch * roll * current
            _targetRotation = yawDelta * pitchDelta * rollDelta * _targetRotation;
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
            // Smoothly interpolate toward target rotation
            Quaternion smoothedRotation = Quaternion.Slerp(
                _rb.rotation,
                _targetRotation,
                Time.fixedDeltaTime * _rotationSmoothSpeed
            );
            
            _rb.MoveRotation(smoothedRotation);
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
