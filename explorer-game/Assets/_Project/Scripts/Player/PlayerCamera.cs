using UnityEngine;
using UnityEngine.Rendering;

namespace Explorer.Player
{
    /// <summary>
    /// Camera perspective modes.
    /// </summary>
    public enum CameraPerspective
    {
        ThirdPerson,
        FirstPerson
    }

    /// <summary>
    /// Camera with perspective toggle (first-person / third-person) that handles spherical gravity orientation.
    /// Smoothly aligns "up" to the player's LocalUp while allowing orbit controls.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Target")]
        [SerializeField]
        [Tooltip("The transform to follow (usually the player).")]
        private Transform _target;

        [SerializeField]
        [Tooltip("Offset from target position to look at.")]
        private Vector3 _targetOffset = new Vector3(0f, 1f, 0f);

        [Header("Third-Person")]
        [SerializeField]
        [Tooltip("Distance from target in third-person.")]
        private float _distance = 5f;

        [SerializeField]
        [Tooltip("Minimum distance (when zooming in).")]
        private float _minDistance = 2f;

        [SerializeField]
        [Tooltip("Maximum distance (when zooming out).")]
        private float _maxDistance = 10f;

        [Header("First-Person")]
        [SerializeField]
        [Tooltip("Offset from target position for first-person view (head height).")]
        private Vector3 _firstPersonOffset = new Vector3(0f, 1.6f, 0f);

        [SerializeField]
        [Tooltip("Time to transition between perspectives.")]
        private float _perspectiveTransitionTime = 0.3f;

        [Header("Model Visibility")]
        [SerializeField]
        [Tooltip("Renderers to hide in first-person mode.")]
        private Renderer[] _playerRenderers;

        [Header("Rotation")]
        [SerializeField]
        [Tooltip("Horizontal rotation speed (degrees per unit input).")]
        private float _horizontalSensitivity = 2f;

        [SerializeField]
        [Tooltip("Vertical rotation speed (degrees per unit input).")]
        private float _verticalSensitivity = 2f;

        [SerializeField]
        [Tooltip("Minimum pitch angle (looking up).")]
        private float _minPitch = -40f;

        [SerializeField]
        [Tooltip("Maximum pitch angle (looking down).")]
        private float _maxPitch = 70f;

        [Header("Smoothing")]
        [SerializeField]
        [Tooltip("How quickly camera follows target position.")]
        private float _followSmoothing = 10f;

        [SerializeField]
        [Tooltip("How quickly camera aligns up with player's LocalUp (degrees/second).")]
        private float _upAlignmentSpeed = 90f;

        [SerializeField]
        [Tooltip("Maximum rotation speed for up alignment to prevent nausea (degrees/second). 0 = unlimited.")]
        private float _maxUpRotationSpeed = 180f;

        [Header("Input")]
        [SerializeField]
        [Tooltip("InputReader ScriptableObject for receiving look input.")]
        private InputReader _inputReader;

        [Header("Collision")]
        [SerializeField]
        [Tooltip("Layers to check for camera collision.")]
        private LayerMask _collisionLayers = ~0;

        [SerializeField]
        [Tooltip("How close to collision point before pushing camera.")]
        private float _collisionPadding = 0.2f;

        // === Public Properties ===

        /// <summary>
        /// The current target being followed.
        /// </summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>
        /// The current camera perspective.
        /// </summary>
        public CameraPerspective CurrentPerspective => _currentPerspective;

        /// <summary>
        /// Whether the camera is currently transitioning between perspectives.
        /// </summary>
        public bool IsTransitioning => _transitionProgress < 1f;

        // === Private Fields ===
        private float _yaw;
        private float _pitch;
        private Vector3 _currentUp = Vector3.up;
        private CharacterMotorSpherical _motor;
        private bool _subscribedToInput;

        // Perspective transition
        private CameraPerspective _currentPerspective = CameraPerspective.ThirdPerson;
        private float _transitionProgress = 1f;
        private float _currentDistance;
        private Vector3 _currentOffset;

        // === Unity Lifecycle ===
        private void Start()
        {
            // Initialize rotation from current camera orientation
            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;

            // Clamp initial pitch
            if (_pitch > 180f) _pitch -= 360f;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            // Get motor reference for LocalUp
            if (_target != null)
            {
                _motor = _target.GetComponent<CharacterMotorSpherical>();
            }

            // Initialize perspective state
            _currentPerspective = CameraPerspective.ThirdPerson;
            _transitionProgress = 1f;
            _currentDistance = _distance;
            _currentOffset = _targetOffset;
            SetPlayerModelVisibility(true);

            // Subscribe to input events (if assigned via Inspector, not SetInputReader)
            // Note: SetInputReader handles its own subscription
            if (_inputReader != null && !_subscribedToInput)
            {
                _inputReader.OnToggleCameraView += HandleToggleCameraView;
                _subscribedToInput = true;
            }

            // Lock cursor for mouse look
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events
            if (_inputReader != null && _subscribedToInput)
            {
                _inputReader.OnToggleCameraView -= HandleToggleCameraView;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            HandleInput();
            UpdatePerspectiveTransition();
            UpdateUpDirection();
            UpdateCameraPosition();
        }

        // === Public Methods ===

        /// <summary>
        /// Set the target to follow.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            _motor = target?.GetComponent<CharacterMotorSpherical>();
        }

        /// <summary>
        /// Set the InputReader at runtime.
        /// </summary>
        public void SetInputReader(InputReader inputReader)
        {
            // Unsubscribe from old input reader
            if (_inputReader != null && _subscribedToInput)
            {
                _inputReader.OnToggleCameraView -= HandleToggleCameraView;
            }

            _inputReader = inputReader;

            // Subscribe to new input reader
            if (_inputReader != null)
            {
                _inputReader.OnToggleCameraView += HandleToggleCameraView;
                _subscribedToInput = true;
            }
            else
            {
                _subscribedToInput = false;
            }
        }

        /// <summary>
        /// Snap camera to a specific rotation instantly.
        /// </summary>
        public void SnapToRotation(float yaw, float pitch)
        {
            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);
        }

        /// <summary>
        /// Toggle between first-person and third-person perspectives.
        /// </summary>
        public void TogglePerspective()
        {
            if (IsTransitioning)
                return;

            CameraPerspective newPerspective = _currentPerspective == CameraPerspective.ThirdPerson
                ? CameraPerspective.FirstPerson
                : CameraPerspective.ThirdPerson;

            SetPerspective(newPerspective);
        }

        /// <summary>
        /// Set a specific camera perspective.
        /// </summary>
        public void SetPerspective(CameraPerspective perspective)
        {
            if (perspective == _currentPerspective && _transitionProgress >= 1f)
                return;

            _currentPerspective = perspective;
            _transitionProgress = 0f;

            // Update model visibility
            SetPlayerModelVisibility(perspective == CameraPerspective.ThirdPerson);
        }

        /// <summary>
        /// Force reset to third-person (e.g., when boarding ship).
        /// </summary>
        public void ResetToThirdPerson()
        {
            _currentPerspective = CameraPerspective.ThirdPerson;
            _transitionProgress = 1f;
            _currentDistance = _distance;
            _currentOffset = _targetOffset;
            SetPlayerModelVisibility(true);
        }

        /// <summary>
        /// Set player renderers for visibility toggling.
        /// </summary>
        public void SetPlayerRenderers(Renderer[] renderers)
        {
            _playerRenderers = renderers;
        }

        // === Private Methods ===

        private void HandleToggleCameraView()
        {
            Debug.Log("PlayerCamera: HandleToggleCameraView called");
            TogglePerspective();
        }

        private void HandleInput()
        {
            Vector2 lookInput = Vector2.zero;

            if (_inputReader != null)
            {
                lookInput = _inputReader.LookInput;
            }

            // Apply rotation
            _yaw += lookInput.x * _horizontalSensitivity;
            _pitch -= lookInput.y * _verticalSensitivity; // Inverted for natural feel
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        private void UpdatePerspectiveTransition()
        {
            if (_transitionProgress >= 1f)
                return;

            // Progress the transition
            _transitionProgress += Time.deltaTime / _perspectiveTransitionTime;
            _transitionProgress = Mathf.Clamp01(_transitionProgress);

            // Smooth easing
            float t = Mathf.SmoothStep(0f, 1f, _transitionProgress);

            // Interpolate distance and offset based on target perspective
            if (_currentPerspective == CameraPerspective.FirstPerson)
            {
                _currentDistance = Mathf.Lerp(_distance, 0f, t);
                _currentOffset = Vector3.Lerp(_targetOffset, _firstPersonOffset, t);
            }
            else
            {
                _currentDistance = Mathf.Lerp(0f, _distance, t);
                _currentOffset = Vector3.Lerp(_firstPersonOffset, _targetOffset, t);
            }
        }

        private void SetPlayerModelVisibility(bool visible)
        {
            if (_playerRenderers == null || _playerRenderers.Length == 0)
                return;

            foreach (var renderer in _playerRenderers)
            {
                if (renderer == null)
                    continue;

                if (visible)
                {
                    // Show model fully
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.enabled = true;
                }
                else
                {
                    // Shadow-only mode: cast shadows but don't render
                    renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
            }
        }

        private void UpdateUpDirection()
        {
            // Get target up direction from player motor (already smoothed by GravitySolver)
            Vector3 targetUp = Vector3.up;
            if (_motor != null)
            {
                targetUp = _motor.LocalUp;
            }
            else if (_target != null)
            {
                targetUp = _target.up;
            }

            // Calculate angle difference
            float angleDiff = Vector3.Angle(_currentUp, targetUp);
            if (angleDiff < 0.01f)
            {
                _currentUp = targetUp;
                return;
            }

            // Calculate max rotation this frame (degrees)
            float maxAngleThisFrame = _upAlignmentSpeed * Time.deltaTime;
            if (_maxUpRotationSpeed > 0f)
            {
                maxAngleThisFrame = Mathf.Min(maxAngleThisFrame, _maxUpRotationSpeed * Time.deltaTime);
            }

            // Calculate blend factor
            float blendT = Mathf.Clamp01(maxAngleThisFrame / angleDiff);

            // Handle near-180° flip with consistent rotation direction
            if (angleDiff > 170f)
            {
                Vector3 rotationAxis = Vector3.Cross(_currentUp, targetUp);
                if (rotationAxis.sqrMagnitude < 0.001f)
                {
                    // Use camera right as fallback rotation axis
                    rotationAxis = transform.right;
                }
                rotationAxis.Normalize();

                Quaternion rotation = Quaternion.AngleAxis(maxAngleThisFrame, rotationAxis);
                _currentUp = (rotation * _currentUp).normalized;
            }
            else
            {
                // Normal Slerp for smaller angles
                _currentUp = Vector3.Slerp(_currentUp, targetUp, blendT).normalized;
            }
        }

        private void UpdateCameraPosition()
        {
            // Calculate target position (with offset in local space)
            Vector3 targetPosition = _target.position + _target.TransformDirection(_currentOffset);

            // Build rotation relative to current up
            // Create a rotation basis where Y is our current up
            Quaternion upRotation = Quaternion.FromToRotation(Vector3.up, _currentUp);
            
            // Apply yaw and pitch in that space
            Quaternion yawRotation = Quaternion.AngleAxis(_yaw, Vector3.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(_pitch, Vector3.right);
            
            Quaternion finalRotation = upRotation * yawRotation * pitchRotation;

            // Calculate camera position based on current distance
            Vector3 desiredPosition;
            if (_currentDistance > 0.01f)
            {
                // Third-person: behind the target
                Vector3 offset = finalRotation * new Vector3(0f, 0f, -_currentDistance);
                desiredPosition = targetPosition + offset;

                // Check for collision
                float adjustedDistance = CheckCollision(targetPosition, desiredPosition);
                if (adjustedDistance < _currentDistance)
                {
                    offset = finalRotation * new Vector3(0f, 0f, -adjustedDistance);
                    desiredPosition = targetPosition + offset;
                }
            }
            else
            {
                // First-person: at the target position
                desiredPosition = targetPosition;
            }

            // Apply position with smoothing
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _followSmoothing * Time.deltaTime);

            // Look direction
            if (_currentDistance > 0.01f)
            {
                // Third-person: look at target
                transform.LookAt(targetPosition, _currentUp);
            }
            else
            {
                // First-person: look in player's forward direction based on yaw/pitch
                transform.rotation = finalRotation;
            }
        }

        private float CheckCollision(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.SphereCast(from, _collisionPadding, direction.normalized, out RaycastHit hit, 
                distance, _collisionLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.distance - _collisionPadding;
            }

            return _distance;
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_target == null)
                return;

            // Draw line to target
            Gizmos.color = Color.cyan;
            Vector3 targetPos = _target.position + _target.TransformDirection(_targetOffset);
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
        }
#endif
    }
}
