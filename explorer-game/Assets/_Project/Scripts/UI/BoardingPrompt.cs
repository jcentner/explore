using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Explorer.Core;

namespace Explorer.UI
{
    /// <summary>
    /// Creates and manages a simple "Press [F] to board" prompt.
    /// Auto-creates the Canvas and UI elements on Awake.
    /// Implements IInteractionPrompt for decoupled access.
    /// </summary>
    public class BoardingPrompt : MonoBehaviour, IInteractionPrompt
    {
        // === Inspector Fields ===
        [Header("Settings")]
        [SerializeField, Tooltip("Text to display")]
        private string _promptText = "Press [F] to board";
        
        [SerializeField, Tooltip("Fade duration")]
        private float _fadeDuration = 0.2f;
        
        [SerializeField, Tooltip("Vertical offset from bottom of screen")]
        private float _bottomOffset = 100f;
        
        // === Private Fields ===
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _text;
        private float _targetAlpha;
        
        // === Public Properties ===
        
        /// <summary>Whether the prompt is currently visible.</summary>
        public bool IsVisible => _targetAlpha > 0f;
        
        // === Singleton ===
        public static BoardingPrompt Instance { get; private set; }
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            CreateUI();
            
            // Register with service locator for decoupled access
            UIService<IInteractionPrompt>.Register(this);
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
        }
        
        private void OnDestroy()
        {
            // Unregister from service locator
            UIService<IInteractionPrompt>.Unregister(this);
            
            if (Instance == this)
                Instance = null;
        }
        
        // === Public Methods (IInteractionPrompt) ===
        
        /// <summary>Show the boarding prompt with simple text.</summary>
        public void Show(string text)
        {
            if (text != null)
                _text.text = text;
            else
                _text.text = _promptText;
            
            _targetAlpha = 1f;
        }
        
        /// <summary>Show the boarding prompt with key and context.</summary>
        public void Show(string actionKey, string context)
        {
            _text.text = $"[{actionKey}] {context}";
            _targetAlpha = 1f;
        }
        
        /// <summary>Show the boarding prompt with structured data.</summary>
        public void Show(InteractionPromptData data)
        {
            string action = data.ActionVerb ?? "Interact";
            if (!string.IsNullOrEmpty(data.TargetName))
                action = $"{action} {data.TargetName}";
            
            Show(data.ActionKey ?? "F", action);
        }
        
        /// <summary>Hide the boarding prompt.</summary>
        public void Hide()
        {
            _targetAlpha = 0f;
        }
        
        // === Private Methods ===
        
        private void CreateUI()
        {
            // Create Canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            
            // Create panel for prompt
            var panelGO = new GameObject("PromptPanel");
            panelGO.transform.SetParent(transform);
            
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, _bottomOffset);
            panelRect.sizeDelta = new Vector2(300f, 50f);
            
            _canvasGroup = panelGO.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            
            // Background
            var bgImage = panelGO.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Text
            var textGO = new GameObject("PromptText");
            textGO.transform.SetParent(panelGO.transform);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);
            
            _text = textGO.AddComponent<TextMeshProUGUI>();
            _text.text = _promptText;
            _text.fontSize = 24f;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = Color.white;
            
            _targetAlpha = 0f;
        }
        
        // === Static Creation ===
        
        /// <summary>
        /// Create the boarding prompt if it doesn't exist.
        /// Call this from any script that needs the prompt.
        /// </summary>
        public static BoardingPrompt GetOrCreate()
        {
            if (Instance != null)
                return Instance;
            
            var go = new GameObject("BoardingPrompt");
            return go.AddComponent<BoardingPrompt>();
        }
    }
}
