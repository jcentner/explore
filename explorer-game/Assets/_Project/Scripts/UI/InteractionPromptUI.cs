using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Explorer.UI
{
    /// <summary>
    /// Simple interaction prompt UI that shows "Press [F] to board" style prompts.
    /// Can be controlled by ShipBoardingTrigger or other interaction systems.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("UI Elements")]
        [SerializeField, Tooltip("The text component showing the prompt")]
        private TextMeshProUGUI _promptText;
        
        [SerializeField, Tooltip("Optional background panel")]
        private Image _backgroundPanel;
        
        [Header("Animation")]
        [SerializeField, Tooltip("Fade duration when showing/hiding")]
        private float _fadeDuration = 0.2f;
        
        [SerializeField, Tooltip("Slight bob animation")]
        private bool _enableBob = true;
        
        [SerializeField, Tooltip("Bob amount in pixels")]
        private float _bobAmount = 5f;
        
        [SerializeField, Tooltip("Bob speed")]
        private float _bobSpeed = 2f;
        
        // === Private Fields ===
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private float _targetAlpha;
        private bool _isVisible;
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
            
            // Start hidden
            _canvasGroup.alpha = 0f;
            _targetAlpha = 0f;
            _isVisible = false;
        }
        
        private void Update()
        {
            // Smooth fade
            if (!Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
            {
                _canvasGroup.alpha = Mathf.MoveTowards(
                    _canvasGroup.alpha, 
                    _targetAlpha, 
                    Time.deltaTime / _fadeDuration
                );
            }
            
            // Bob animation when visible
            if (_enableBob && _isVisible)
            {
                float bob = Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
                _rectTransform.anchoredPosition = _originalPosition + Vector2.up * bob;
            }
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Show the prompt with specified text.
        /// </summary>
        public void Show(string text = "Press [F] to interact")
        {
            if (_promptText != null)
                _promptText.text = text;
            
            _targetAlpha = 1f;
            _isVisible = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Hide the prompt.
        /// </summary>
        public void Hide()
        {
            _targetAlpha = 0f;
            _isVisible = false;
        }
        
        /// <summary>
        /// Immediately show/hide without animation.
        /// </summary>
        public void SetVisible(bool visible, string text = null)
        {
            if (text != null && _promptText != null)
                _promptText.text = text;
            
            _isVisible = visible;
            _targetAlpha = visible ? 1f : 0f;
            _canvasGroup.alpha = _targetAlpha;
            gameObject.SetActive(visible);
        }
    }
}
