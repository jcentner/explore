using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Explorer.Core;

namespace Explorer.UI
{
    /// <summary>
    /// Central manager for all UI in the game.
    /// Manages screen stack, panel visibility, and pause state.
    /// Implements IPauseHandler for decoupled pause input handling.
    /// Requires a UIDocument component on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour, IPauseHandler
    {
        // === Singleton ===
        private static UIManager _instance;
        public static UIManager Instance => _instance;
        
        // === Private Fields ===
        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _screensContainer;
        private VisualElement _panelsContainer;
        
        private readonly Stack<UIScreen> _screenStack = new();
        private readonly Dictionary<Type, UIPanel> _panels = new();
        private readonly Dictionary<Type, UIScreen> _screens = new();
        
        private bool _isPaused;
        private float _previousTimeScale = 1f;
        
        // === Public Properties ===
        
        /// <summary>Root VisualElement of the UI document.</summary>
        public VisualElement Root => _root;
        
        /// <summary>Container for screen elements.</summary>
        public VisualElement ScreensContainer => _screensContainer;
        
        /// <summary>Container for panel elements.</summary>
        public VisualElement PanelsContainer => _panelsContainer;
        
        /// <summary>Whether the game is currently paused via UI.</summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>Whether any screen is currently open.</summary>
        public bool IsAnyScreenOpen => _screenStack.Count > 0;
        
        // === Events ===
        
        /// <summary>Fired when pause state changes.</summary>
        public event Action<bool> OnPauseStateChanged;
        
        // === Unity Lifecycle ===
        
        private void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[UIManager] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            // Register with service locator for decoupled pause handling
            UIService<IPauseHandler>.Register(this);
            
            // Get UIDocument
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError("[UIManager] UIDocument component required!");
                return;
            }
        }
        
        private void OnEnable()
        {
            // Wait for document to be ready
            if (_document.rootVisualElement != null)
            {
                SetupUI();
            }
            else
            {
                // UIDocument might not be ready yet, wait a frame
                StartCoroutine(WaitForDocument());
            }
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                UIService<IPauseHandler>.Unregister(this);
            }
        }
        
        private System.Collections.IEnumerator WaitForDocument()
        {
            yield return null; // Wait one frame
            if (_document.rootVisualElement != null)
            {
                SetupUI();
            }
            else
            {
                Debug.LogError("[UIManager] UIDocument rootVisualElement is null after waiting.");
            }
        }
        
        // === Setup ===
        
        private void SetupUI()
        {
            _root = _document.rootVisualElement;
            
            // Find or create containers
            _panelsContainer = _root.Q<VisualElement>("Panels");
            if (_panelsContainer == null)
            {
                _panelsContainer = new VisualElement { name = "Panels" };
                _panelsContainer.AddToClassList("panels-container");
                _root.Add(_panelsContainer);
            }
            
            _screensContainer = _root.Q<VisualElement>("Screens");
            if (_screensContainer == null)
            {
                _screensContainer = new VisualElement { name = "Screens" };
                _screensContainer.AddToClassList("screens-container");
                _root.Add(_screensContainer);
            }
            
            // Initialize built-in panels and screens
            InitializePanels();
            InitializeScreens();
        }
        
        private void InitializePanels()
        {
            // Initialize Interaction Prompt Panel from template instance
            var promptInstance = _panelsContainer.Q<TemplateContainer>("InteractionPromptInstance");
            if (promptInstance != null)
            {
                var interactionPrompt = new InteractionPromptPanel(promptInstance);
                RegisterPanel(interactionPrompt);
                Debug.Log("[UIManager] InteractionPromptPanel initialized.");
            }
            else
            {
                Debug.LogWarning("[UIManager] InteractionPromptInstance not found in Panels container. " +
                    "Make sure MainUI.uxml includes the InteractionPrompt template.");
            }
        }
        
        private void InitializeScreens()
        {
            // Initialize Pause Screen from template instance
            var pauseInstance = _screensContainer.Q<TemplateContainer>("PauseScreenInstance");
            if (pauseInstance != null)
            {
                var pauseScreen = new PauseScreen(pauseInstance);
                RegisterScreen(pauseScreen);
                Debug.Log("[UIManager] PauseScreen initialized.");
            }
            else
            {
                Debug.LogWarning("[UIManager] PauseScreenInstance not found in Screens container. " +
                    "Make sure MainUI.uxml includes the PauseScreen template.");
            }
        }
        
        // === Screen Management ===
        
        /// <summary>
        /// Register a screen instance for later retrieval.
        /// </summary>
        public void RegisterScreen<T>(T screen) where T : UIScreen
        {
            _screens[typeof(T)] = screen;
        }
        
        /// <summary>
        /// Push a screen onto the stack and show it.
        /// </summary>
        public void PushScreen<T>() where T : UIScreen
        {
            if (!_screens.TryGetValue(typeof(T), out var screen))
            {
                Debug.LogWarning($"[UIManager] Screen {typeof(T).Name} not registered.");
                return;
            }
            
            // Hide current top screen if any
            if (_screenStack.Count > 0)
            {
                _screenStack.Peek().Hide();
            }
            
            _screenStack.Push(screen);
            screen.Show();
        }
        
        /// <summary>
        /// Pop the current screen from the stack.
        /// </summary>
        public void PopScreen()
        {
            if (_screenStack.Count == 0) return;
            
            var screen = _screenStack.Pop();
            screen.Hide();
            
            // Show previous screen if any
            if (_screenStack.Count > 0)
            {
                _screenStack.Peek().Show();
            }
            
            // If no more screens, unpause
            if (_screenStack.Count == 0 && _isPaused)
            {
                SetPaused(false);
            }
        }
        
        /// <summary>
        /// Pop all screens and return to gameplay.
        /// </summary>
        public void PopAllScreens()
        {
            while (_screenStack.Count > 0)
            {
                var screen = _screenStack.Pop();
                screen.Hide();
            }
            
            if (_isPaused)
            {
                SetPaused(false);
            }
        }
        
        /// <summary>
        /// Check if a specific screen type is currently open.
        /// </summary>
        public bool IsScreenOpen<T>() where T : UIScreen
        {
            foreach (var screen in _screenStack)
            {
                if (screen is T) return true;
            }
            return false;
        }
        
        // === Panel Management ===
        
        /// <summary>
        /// Register a panel instance for management.
        /// </summary>
        public void RegisterPanel<T>(T panel) where T : UIPanel
        {
            _panels[typeof(T)] = panel;
        }
        
        /// <summary>
        /// Get a registered panel by type.
        /// </summary>
        public T GetPanel<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                return panel as T;
            }
            return null;
        }
        
        /// <summary>
        /// Show a panel by type.
        /// </summary>
        public void ShowPanel<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                panel.Show();
            }
        }
        
        /// <summary>
        /// Hide a panel by type.
        /// </summary>
        public void HidePanel<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                panel.Hide();
            }
        }
        
        // === IPauseHandler Implementation ===
        
        /// <summary>Whether any UI screen is currently blocking gameplay.</summary>
        public bool IsUIBlocking => _screenStack.Count > 0;
        
        /// <summary>
        /// Handle pause input. Implements IPauseHandler for decoupled access.
        /// Call this from InputReader.OnPause event via UIService.
        /// </summary>
        public void HandlePause()
        {
            if (_screenStack.Count > 0)
            {
                // If screens are open, handle back navigation (e.g., resume from pause)
                var topScreen = _screenStack.Peek();
                topScreen.HandleBack();
            }
            else
            {
                // No screens open, show pause menu
                PushScreen<PauseScreen>();
            }
        }
        
        /// <summary>
        /// Legacy method - use HandlePause() instead.
        /// </summary>
        [Obsolete("Use HandlePause() instead")]
        public void HandlePauseInput() => HandlePause();
        
        /// <summary>
        /// Toggle pause state (internal, prefer HandlePause for user-facing).
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!_isPaused);
        }
        
        /// <summary>
        /// Set the paused state of the game.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (_isPaused == paused) return;
            
            _isPaused = paused;
            
            if (paused)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
            else
            {
                Time.timeScale = _previousTimeScale;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }
            
            OnPauseStateChanged?.Invoke(_isPaused);
        }
    }
}
