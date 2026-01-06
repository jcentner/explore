using System;
using UnityEngine;
using Explorer.Gravity;

namespace Explorer.Player
{
    /// <summary>
    /// Character motor designed for spherical gravity surfaces.
    /// Handles movement, jumping, up-alignment, and zero-G floating.
    /// </summary>
    [RequireComponent(typeof(GravitySolver))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMotorSpherical : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Movement")]
        [SerializeField]
        [Tooltip("Walking speed in m/s.")]
        private float _walkSpeed = 5f;

        [SerializeField]
        [Tooltip("Running speed in m/s.")]
        private float _runSpeed = 8f;

        [SerializeField]
        [Tooltip("Acceleration when starting to move.")]
        private float _acceleration = 10f;

        [SerializeField]
        [Tooltip("Deceleration when stopping.")]
        private float _deceleration = 12f;

        [Header("Jumping")]
        [SerializeField]
        [Tooltip("Jump force in m/s.")]
        private float _jumpForce = 8f;

        [SerializeField]
        [Tooltip("Control multiplier while airborne (0-1).")]
        private float _airControl = 0.3f;

        [SerializeField]
        [Tooltip("Cooldown before player can jump again.")]
        private float _jumpCooldown = 0.2f;

        [Header("Grounding")]
        [SerializeField]
        [Tooltip("Distance to check for ground below player.")]
        private float _groundCheckDistance = 0.2f;

        [SerializeField]
        [Tooltip("Radius of the spherecast for ground detection.")]
        private float _groundCheckRadius = 0.3f;

        [SerializeField]
        [Tooltip("Layers considered as walkable ground.")]
        private LayerMask _groundLayers = ~0; // Everything by default

        [Header("Rotation")]
        [SerializeField]
        [Tooltip("Speed at which player aligns to gravity 'up' (degrees/second). Uses GravitySolver's smoothed LocalUp.")]
        private float _upAlignmentSpeed = 180f;

        [SerializeField]
        [Tooltip("Maximum rotation speed for alignment to prevent disorientation (degrees/second).")]
        private float _maxAlignmentSpeed = 360f;

        [Header("Input")]
        [SerializeField]
        [Tooltip("InputReader ScriptableObject for receiving input.")]
        private InputReader _inputReader;

        // === Public Properties ===
        
        /// <summary>
        /// Whether the player is currently on the ground.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Whether the player is currently in zero-g.
        /// </summary>
        public bool IsInZeroG => _gravitySolver != null && _gravitySolver.IsInZeroG;

        /// <summary>
        /// Whether the player is currently moving (has velocity).
        /// </summary>
        public bool IsMoving => _horizontalVelocity.sqrMagnitude > 0.1f;

        /// <summary>
        /// Whether the player is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Local "up" direction (opposite of gravity).
        /// </summary>
        public Vector3 LocalUp => _gravitySolver.LocalUp;

        /// <summary>
        /// Current horizontal velocity relative to the surface.
        /// </summary>
        public Vector3 Velocity => _rb.linearVelocity;

        // === Events ===
        /// <summary>Fired when entering zero-g state.</summary>
        public event Action OnZeroGEntered;

        /// <summary>Fired when exiting zero-g state.</summary>
        public event Action OnZeroGExited;

        // === Private Fields ===
        private GravitySolver _gravitySolver;
        private Rigidbody _rb;
        private CapsuleCollider _capsule;

        private Vector2 _moveInput;
        private Vector3 _horizontalVelocity;
        private float _lastJumpTime;
        private bool _jumpRequested;
        private Transform _cameraTransform;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _gravitySolver = GetComponent<GravitySolver>();
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            // Configure Rigidbody for character control
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void Start()
        {
            _cameraTransform = Camera.main?.transform;

            // Subscribe to input events (if InputReader was assigned in Inspector)
            if (_inputReader != null)
            {
                _inputReader.OnJump += HandleJumpInput;
            }

            // Subscribe to gravity solver events
            if (_gravitySolver != null)
            {
                _gravitySolver.OnZeroGEntered += HandleZeroGEntered;
                _gravitySolver.OnZeroGExited += HandleZeroGExited;
            }
        }

        private void OnDestroy()
        {
            if (_inputReader != null)
            {
                _inputReader.OnJump -= HandleJumpInput;
            }

            if (_gravitySolver != null)
            {
                _gravitySolver.OnZeroGEntered -= HandleZeroGEntered;
                _gravitySolver.OnZeroGExited -= HandleZeroGExited;
            }
        }

        private void Update()
        {
            // Read continuous input
            if (_inputReader != null)
            {
                _moveInput = _inputReader.MoveInput;
                IsRunning = _inputReader.SprintHeld;
            }
        }

        private void FixedUpdate()
        {
            // In zero-g, player is helpless (no movement control)
            // They can only drift and wait for gravity or use their ship
            if (!IsInZeroG)
            {
                CheckGrounded();
                ApplyGravity();
                HandleMovement();
                HandleJump();
                AlignToGravity();
            }
        }

        // === Public Methods ===

        /// <summary>
        /// Set the InputReader at runtime.
        /// </summary>
        public void SetInputReader(InputReader inputReader)
        {
            // Unsubscribe from old reader
            if (_inputReader != null)
            {
                _inputReader.OnJump -= HandleJumpInput;
            }

            _inputReader = inputReader;

            // Subscribe to new reader
            if (_inputReader != null)
            {
                _inputReader.OnJump += HandleJumpInput;
            }
        }

        /// <summary>
        /// Set movement input directly (for external control).
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        /// <summary>
        /// Request a jump (for external control).
        /// </summary>
        public void Jump()
        {
            HandleJumpInput();
        }

        // === Private Methods ===

        private void HandleJumpInput()
        {
            if (IsGrounded && Time.time - _lastJumpTime > _jumpCooldown)
            {
                _jumpRequested = true;
            }
        }

        private void CheckGrounded()
        {
            // Spherecast downward along local down
            Vector3 origin = transform.position + LocalUp * _capsule.radius;
            float checkDistance = _capsule.radius + _groundCheckDistance;

            IsGrounded = Physics.SphereCast(
                origin,
                _groundCheckRadius,
                -LocalUp,
                out _,
                checkDistance,
                _groundLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        private void ApplyGravity()
        {
            // Apply gravity as force
            _gravitySolver.ApplyGravityForce();
        }

        private void HandleMovement()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                // Decelerate
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    Vector3.zero,
                    _deceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                // Calculate movement direction relative to camera
                Vector3 moveDirection = GetMoveDirection();
                
                // Target speed
                float targetSpeed = IsRunning ? _runSpeed : _walkSpeed;
                
                // Control factor (reduced in air)
                float controlFactor = IsGrounded ? 1f : _airControl;

                // Accelerate toward target velocity
                Vector3 targetVelocity = moveDirection * targetSpeed;
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    targetVelocity,
                    _acceleration * controlFactor * Time.fixedDeltaTime
                );
            }

            // Apply horizontal velocity (preserve vertical/gravity component)
            Vector3 gravityVelocity = Vector3.Project(_rb.linearVelocity, -LocalUp);
            _rb.linearVelocity = gravityVelocity + _horizontalVelocity;
        }

        private Vector3 GetMoveDirection()
        {
            if (_cameraTransform == null)
            {
                _cameraTransform = Camera.main?.transform;
                if (_cameraTransform == null)
                    return transform.forward * _moveInput.y + transform.right * _moveInput.x;
            }

            // Get camera forward/right projected onto local horizontal plane
            Vector3 cameraForward = Vector3.ProjectOnPlane(_cameraTransform.forward, LocalUp).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(_cameraTransform.right, LocalUp).normalized;

            // Combine into move direction
            Vector3 moveDirection = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;
            return moveDirection;
        }

        private void HandleJump()
        {
            if (_jumpRequested && IsGrounded)
            {
                // Apply jump force along local up
                _rb.linearVelocity += LocalUp * _jumpForce;
                _lastJumpTime = Time.time;
                _jumpRequested = false;
            }
        }

        private void AlignToGravity()
        {
            Vector3 targetUp = LocalUp;
            if (targetUp == Vector3.zero || targetUp.sqrMagnitude < 0.001f)
                return;

            // Calculate angle between current up and target up
            float angleDiff = Vector3.Angle(transform.up, targetUp);
            if (angleDiff < 0.01f)
                return;

            // Calculate max rotation this frame (degrees)
            float maxAngleThisFrame = _upAlignmentSpeed * Time.fixedDeltaTime;
            if (_maxAlignmentSpeed > 0f)
            {
                maxAngleThisFrame = Mathf.Min(maxAngleThisFrame, _maxAlignmentSpeed * Time.fixedDeltaTime);
            }

            // Calculate blend factor based on angle remaining
            float blendT = Mathf.Clamp01(maxAngleThisFrame / angleDiff);

            // Handle near-180° flip: choose consistent rotation direction
            Quaternion targetRotation;
            if (angleDiff > 170f)
            {
                // Find rotation axis perpendicular to both up vectors
                Vector3 rotationAxis = Vector3.Cross(transform.up, targetUp);
                if (rotationAxis.sqrMagnitude < 0.001f)
                {
                    // Parallel vectors - use forward as rotation axis
                    rotationAxis = transform.forward;
                }
                rotationAxis.Normalize();

                // Create incremental rotation around the axis
                Quaternion incrementalRotation = Quaternion.AngleAxis(maxAngleThisFrame, rotationAxis);
                targetRotation = incrementalRotation * transform.rotation;
            }
            else
            {
                // Normal case: rotate from current up to target up
                targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
                targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, blendT);
            }

            transform.rotation = targetRotation;
        }

        // === Zero-G Methods ===

        private void HandleZeroGEntered()
        {
            // Clear horizontal velocity tracking when entering zero-g
            _horizontalVelocity = Vector3.zero;
            IsGrounded = false;

            // Player is helpless in zero-g - add slight drag so they don't drift forever
            _rb.linearDamping = 0.1f;

            OnZeroGEntered?.Invoke();
        }

        private void HandleZeroGExited()
        {
            // Reset drag when exiting zero-g
            _rb.linearDamping = 0f;

            OnZeroGExited?.Invoke();
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_capsule == null)
                _capsule = GetComponent<CapsuleCollider>();
            if (_capsule == null)
                return;

            // Draw ground check (not relevant in zero-g)
            Vector3 localUp = Application.isPlaying ? LocalUp : transform.up;
            Vector3 origin = transform.position + localUp * _capsule.radius;
            float checkDistance = _capsule.radius + _groundCheckDistance;

            // Color based on state
            if (Application.isPlaying && IsInZeroG)
            {
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f); // Magenta for zero-g
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
            else
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(origin - localUp * checkDistance, _groundCheckRadius);
            }
        }
#endif
    }
}
