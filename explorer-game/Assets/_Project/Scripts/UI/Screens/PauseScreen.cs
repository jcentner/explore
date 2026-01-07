using UnityEngine;
using UnityEngine.UIElements;

namespace Explorer.UI
{
    /// <summary>
    /// Pause menu screen that halts gameplay and provides Resume/Settings/Quit options.
    /// Handles time scale, cursor visibility, and UI-only input mode.
    /// </summary>
    public class PauseScreen : UIScreen
    {
        // === Constants ===
        private const string OVERLAY_CLASS = "pause-screen__overlay";
        private const string RESUME_BUTTON = "ResumeButton";
        private const string SETTINGS_BUTTON = "SettingsButton";
        private const string QUIT_BUTTON = "QuitButton";
        
        // === UI Elements ===
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _quitButton;
        
        // === State ===
        private float _previousTimeScale = 1f;
        
        // === Constructor ===
        
        /// <summary>
        /// Create pause screen from template container.
        /// </summary>
        public PauseScreen(VisualElement templateContainer) : base()
        {
            // Find the root pause-screen element
            Root = templateContainer.Q<VisualElement>(className: "pause-screen");
            if (Root == null)
            {
                Root = templateContainer;
            }
            
            // Query buttons
            _resumeButton = Root.Q<Button>(RESUME_BUTTON);
            _settingsButton = Root.Q<Button>(SETTINGS_BUTTON);
            _quitButton = Root.Q<Button>(QUIT_BUTTON);
            
            // Wire up button callbacks
            if (_resumeButton != null)
            {
                _resumeButton.clicked += OnResumeClicked;
            }
            else
            {
                Debug.LogWarning("[PauseScreen] ResumeButton not found in template");
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.clicked += OnSettingsClicked;
            }
            
            if (_quitButton != null)
            {
                _quitButton.clicked += OnQuitClicked;
            }
            
            // Start hidden
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.AddToClassList("screen");
                Root.AddToClassList("screen--hidden");
            }
        }
        
        // === Public Methods ===
        
        public override void Show()
        {
            // Store current time scale and pause
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            
            // Show cursor for menu interaction
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            
            base.Show();
            
            // Focus the resume button for keyboard/gamepad navigation
            _resumeButton?.Focus();
        }
        
        public override void Hide()
        {
            // Restore time scale
            Time.timeScale = _previousTimeScale;
            
            // Re-lock cursor for gameplay
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            
            base.Hide();
        }
        
        public override void HandleBack()
        {
            // Escape while paused = resume
            OnResumeClicked();
        }
        
        // === Button Callbacks ===
        
        private void OnResumeClicked()
        {
            UIManager.Instance?.PopScreen();
        }
        
        private void OnSettingsClicked()
        {
            // TODO: Push settings screen when implemented
            Debug.Log("[PauseScreen] Settings clicked - not yet implemented");
            // UIManager.Instance?.PushScreen<SettingsScreen>();
        }
        
        private void OnQuitClicked()
        {
            // Restore time before quitting
            Time.timeScale = 1f;
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        // === Cleanup ===
        
        /// <summary>
        /// Unsubscribe from button events.
        /// </summary>
        public void Dispose()
        {
            if (_resumeButton != null)
            {
                _resumeButton.clicked -= OnResumeClicked;
            }
            if (_settingsButton != null)
            {
                _settingsButton.clicked -= OnSettingsClicked;
            }
            if (_quitButton != null)
            {
                _quitButton.clicked -= OnQuitClicked;
            }
        }
    }
}
