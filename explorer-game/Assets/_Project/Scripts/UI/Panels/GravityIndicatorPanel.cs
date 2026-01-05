using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Explorer.Core;
using Explorer.Gravity;

namespace Explorer.UI
{
    /// <summary>
    /// UI panel showing the current gravity direction and magnitude.
    /// Displays an arrow pointing in the gravity direction and indicates zero-g state.
    /// </summary>
    public class GravityIndicatorPanel : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Target")]
        [SerializeField]
        [Tooltip("The transform with IGravityAffected component. Auto-finds player if null.")]
        private Transform _target;

        [Header("Arrow Display")]
        [SerializeField]
        [Tooltip("The arrow transform that rotates to show gravity direction.")]
        private RectTransform _arrowTransform;

        [SerializeField]
        [Tooltip("Arrow text component (uses ▼ character). Preferred over Image.")]
        private TextMeshProUGUI _arrowText;

        [SerializeField]
        [Tooltip("Arrow image component for color changes (fallback if no text).")]
        private Image _arrowImage;

        [SerializeField]
        [Tooltip("Minimum arrow scale at zero gravity.")]
        private float _minArrowScale = 0.3f;

        [SerializeField]
        [Tooltip("Maximum arrow scale at full gravity.")]
        private float _maxArrowScale = 1f;

        [SerializeField]
        [Tooltip("Gravity magnitude (m/s²) at which arrow reaches max scale.")]
        private float _maxGravityForScale = 15f;

        [Header("Zero-G Display")]
        [SerializeField]
        [Tooltip("Container shown when in zero-g.")]
        private GameObject _zeroGContainer;

        [SerializeField]
        [Tooltip("Text label for zero-g state.")]
        private TextMeshProUGUI _zeroGText;

        [SerializeField]
        [Tooltip("Text showing gravity magnitude (optional).")]
        private TextMeshProUGUI _magnitudeText;

        [SerializeField]
        [Tooltip("Pulse speed for zero-g indicator.")]
        private float _zeroGPulseSpeed = 2f;

        [SerializeField]
        [Tooltip("Pulse alpha range (min, max).")]
        private Vector2 _zeroGPulseAlpha = new Vector2(0.5f, 1f);

        [Header("Colors")]
        [SerializeField]
        [Tooltip("Arrow color at normal gravity.")]
        private Color _normalGravityColor = Color.white;

        [SerializeField]
        [Tooltip("Arrow color at low gravity.")]
        private Color _lowGravityColor = Color.yellow;

        [SerializeField]
        [Tooltip("Zero-G indicator color.")]
        private Color _zeroGColor = new Color(1f, 0f, 1f, 1f); // Magenta

        [SerializeField]
        [Tooltip("Gravity threshold below which color starts blending to low gravity.")]
        private float _lowGravityThreshold = 3f;

        [Header("Smoothing")]
        [SerializeField]
        [Tooltip("How quickly the arrow rotates to match gravity direction.")]
        private float _rotationSmoothing = 10f;

        [SerializeField]
        [Tooltip("How quickly the arrow scales to match gravity magnitude.")]
        private float _scaleSmoothing = 5f;

        // === Private Fields ===
        private IGravityAffected _gravityAffected;
        private Camera _mainCamera;
        private float _currentRotation;
        private float _currentScale = 1f;
        private bool _wasInZeroG;
        private CanvasGroup _canvasGroup;
        private bool _useTextArrow; // True if using TextMeshProUGUI, false if using Image
        private Transform _playerTransform;
        private Transform _lastPlayerParent;
        private float _recheckInterval = 0.5f;
        private float _recheckTimer;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Auto-find child references if not assigned
            AutoFindChildReferences();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            FindGravityTarget();

            // Initialize zero-g container state
            if (_zeroGContainer != null)
                _zeroGContainer.SetActive(false);
        }

        /// <summary>
        /// Automatically finds child references if not assigned in inspector.
        /// Looks for children named "GravityArrow", "ZeroGContainer", "ZeroGText".
        /// </summary>
        private void AutoFindChildReferences()
        {
            // Find arrow transform and text/image
            if (_arrowTransform == null)
            {
                Transform arrowChild = transform.Find("GravityArrow");
                if (arrowChild != null)
                {
                    _arrowTransform = arrowChild as RectTransform;
                    
                    // Prefer TextMeshProUGUI over Image
                    if (_arrowText == null)
                        _arrowText = arrowChild.GetComponent<TextMeshProUGUI>();
                    if (_arrowImage == null)
                        _arrowImage = arrowChild.GetComponent<Image>();
                }
            }
            
            // Determine which arrow type to use
            _useTextArrow = _arrowText != null;
            
            // If using text, ensure it has an arrow character
            if (_useTextArrow && string.IsNullOrEmpty(_arrowText.text))
            {
                _arrowText.text = "▼";
                _arrowText.fontSize = 72;
                _arrowText.alignment = TextAlignmentOptions.Center;
            }

            // Find zero-g container
            if (_zeroGContainer == null)
            {
                Transform zeroGChild = transform.Find("ZeroGContainer");
                if (zeroGChild != null)
                {
                    _zeroGContainer = zeroGChild.gameObject;

                    // Find zero-g text within container
                    if (_zeroGText == null)
                    {
                        Transform textChild = zeroGChild.Find("ZeroGText");
                        if (textChild != null)
                            _zeroGText = textChild.GetComponent<TextMeshProUGUI>();
                    }
                }
            }
            
            // Find magnitude text
            if (_magnitudeText == null)
            {
                Transform magChild = transform.Find("MagnitudeText");
                if (magChild != null)
                    _magnitudeText = magChild.GetComponent<TextMeshProUGUI>();
            }
        }

        private void Update()
        {
            // Check if player's parent changed (boarded/exited ship)
            if (_playerTransform != null && _playerTransform.parent != _lastPlayerParent)
            {
                _lastPlayerParent = _playerTransform.parent;
                _target = null;
                _gravityAffected = null;
            }
            
            // Periodic re-check if no target
            if (_gravityAffected == null)
            {
                _recheckTimer -= Time.deltaTime;
                if (_recheckTimer <= 0f)
                {
                    _recheckTimer = _recheckInterval;
                    FindGravityTarget();
                }
                if (_gravityAffected == null)
                    return;
            }

            // Refresh camera if needed (e.g., scene reload)
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            UpdateArrowDirection();
            UpdateArrowScale();
            UpdateMagnitudeDisplay();
            UpdateZeroGDisplay();
        }

        // === Public Methods ===

        /// <summary>
        /// Set the target transform with IGravityAffected component.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            FindGravityTarget();
        }

        // === Private Methods ===

        private void FindGravityTarget()
        {
            // Refresh camera reference
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            
            if (_target != null)
            {
                _gravityAffected = _target.GetComponent<IGravityAffected>();
                if (_gravityAffected != null)
                    return;
            }

            // Try to find player by tag first
            var player = GameObject.FindGameObjectWithTag(Tags.PLAYER);
            if (player != null)
            {
                _playerTransform = player.transform;
                _lastPlayerParent = _playerTransform.parent;
                
                // Player might be in a ship - check parent hierarchy for IGravityAffected first
                Transform current = player.transform.parent;
                while (current != null)
                {
                    var parentGravity = current.GetComponent<IGravityAffected>();
                    if (parentGravity != null)
                    {
                        _target = current;
                        _gravityAffected = parentGravity;
                        return;
                    }
                    current = current.parent;
                }
                
                // No parent with gravity - check player itself
                var playerGravity = player.GetComponent<IGravityAffected>();
                if (playerGravity != null)
                {
                    _target = player.transform;
                    _gravityAffected = playerGravity;
                    return;
                }
            }
            
            // Fallback: find any GravitySolver in scene
            var solver = FindFirstObjectByType<Explorer.Gravity.GravitySolver>();
            if (solver != null)
            {
                _target = solver.transform;
                _gravityAffected = solver;
            }
        }

        private void UpdateArrowDirection()
        {
            if (_arrowTransform == null || _mainCamera == null)
                return;

            Vector3 gravity = _gravityAffected.CurrentGravity;

            if (gravity.sqrMagnitude < 0.001f)
            {
                // No gravity - hide or minimize arrow
                return;
            }

            // Project gravity direction onto screen space
            // We want the arrow to point "down" relative to the camera's view
            Vector3 gravityDirection = gravity.normalized;

            // Get the gravity direction in camera space
            Vector3 gravityCameraSpace = _mainCamera.transform.InverseTransformDirection(gravityDirection);

            // Calculate angle on screen (in the XY plane of screen space)
            // Gravity pointing down in world = arrow pointing down on screen
            float targetAngle = Mathf.Atan2(-gravityCameraSpace.x, -gravityCameraSpace.y) * Mathf.Rad2Deg;

            // Smooth rotation
            _currentRotation = Mathf.LerpAngle(_currentRotation, targetAngle, _rotationSmoothing * Time.deltaTime);

            // Apply rotation
            _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, _currentRotation);
        }

        private void UpdateArrowScale()
        {
            if (_arrowTransform == null)
                return;
            
            // Need either text or image for color
            if (!_useTextArrow && _arrowImage == null)
                return;

            Vector3 gravity = _gravityAffected.CurrentGravity;
            float magnitude = gravity.magnitude;

            // Calculate target scale based on gravity magnitude
            float normalizedMagnitude = Mathf.Clamp01(magnitude / _maxGravityForScale);
            float targetScale = Mathf.Lerp(_minArrowScale, _maxArrowScale, normalizedMagnitude);

            // Smooth scale
            _currentScale = Mathf.Lerp(_currentScale, targetScale, _scaleSmoothing * Time.deltaTime);
            _arrowTransform.localScale = Vector3.one * _currentScale;

            // Update color based on gravity strength
            Color targetColor;
            if (magnitude < _lowGravityThreshold)
            {
                float t = magnitude / _lowGravityThreshold;
                targetColor = Color.Lerp(_zeroGColor, _lowGravityColor, t);
            }
            else
            {
                float t = Mathf.Clamp01((magnitude - _lowGravityThreshold) / (_maxGravityForScale - _lowGravityThreshold));
                targetColor = Color.Lerp(_lowGravityColor, _normalGravityColor, t);
            }

            // Apply color to appropriate component
            if (_useTextArrow)
                _arrowText.color = targetColor;
            else
                _arrowImage.color = targetColor;
        }

        private void UpdateMagnitudeDisplay()
        {
            if (_magnitudeText == null)
                return;
                
            float magnitude = _gravityAffected.CurrentGravity.magnitude;
            _magnitudeText.text = $"{magnitude:F1} m/s²";
        }

        private void UpdateZeroGDisplay()
        {
            bool isInZeroG = _gravityAffected.CurrentGravity.magnitude < 0.1f; // Match GravitySolver threshold

            // Handle state transitions
            if (isInZeroG && !_wasInZeroG)
            {
                OnEnterZeroG();
            }
            else if (!isInZeroG && _wasInZeroG)
            {
                OnExitZeroG();
            }

            _wasInZeroG = isInZeroG;

            // Animate zero-g indicator
            if (isInZeroG && _zeroGContainer != null && _zeroGText != null)
            {
                // Pulse animation
                float pulse = Mathf.Lerp(_zeroGPulseAlpha.x, _zeroGPulseAlpha.y,
                    (Mathf.Sin(Time.time * _zeroGPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);

                Color textColor = _zeroGText.color;
                textColor.a = pulse;
                _zeroGText.color = textColor;
            }
        }

        private void OnEnterZeroG()
        {
            if (_zeroGContainer != null)
                _zeroGContainer.SetActive(true);

            if (_arrowTransform != null)
                _arrowTransform.gameObject.SetActive(false);
        }

        private void OnExitZeroG()
        {
            if (_zeroGContainer != null)
                _zeroGContainer.SetActive(false);

            if (_arrowTransform != null)
                _arrowTransform.gameObject.SetActive(true);
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnValidate()
        {
            _minArrowScale = Mathf.Max(0.1f, _minArrowScale);
            _maxArrowScale = Mathf.Max(_minArrowScale, _maxArrowScale);
            _maxGravityForScale = Mathf.Max(0.1f, _maxGravityForScale);
            _lowGravityThreshold = Mathf.Clamp(_lowGravityThreshold, 0.1f, _maxGravityForScale);
        }
#endif
    }
}
