using System;
using UnityEngine.UIElements;

namespace Explorer.UI
{
    /// <summary>
    /// Abstract base class for full-screen UI elements (menus, dialogs, etc.).
    /// Screens are managed in a stack by UIManager.
    /// 
    /// Usage:
    /// 1. Create a derived class (e.g., PauseScreen)
    /// 2. Override OnShown/OnHidden for custom behavior
    /// 3. Register with UIManager.RegisterScreen()
    /// </summary>
    public abstract class UIScreen
    {
        // === Properties ===
        
        /// <summary>The root VisualElement for this screen.</summary>
        public VisualElement Root { get; protected set; }
        
        /// <summary>Whether the screen is currently visible.</summary>
        public bool IsVisible { get; private set; }
        
        // === Events ===
        
        /// <summary>Fired when the screen is shown.</summary>
        public event Action Shown;
        
        /// <summary>Fired when the screen is hidden.</summary>
        public event Action Hidden;
        
        // === Constructor ===
        
        /// <summary>
        /// Create a screen with an existing VisualElement.
        /// </summary>
        /// <param name="root">The root element for this screen.</param>
        protected UIScreen(VisualElement root)
        {
            Root = root;
            
            // Start hidden
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.AddToClassList("screen");
            }
        }
        
        /// <summary>
        /// Create a screen that will load its root from UXML later.
        /// </summary>
        protected UIScreen()
        {
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Show the screen with optional animation.
        /// </summary>
        public virtual void Show()
        {
            if (Root == null) return;
            
            IsVisible = true;
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList("screen--hidden");
            Root.AddToClassList("screen--visible");
            
            OnShown();
            Shown?.Invoke();
        }
        
        /// <summary>
        /// Hide the screen with optional animation.
        /// </summary>
        public virtual void Hide()
        {
            if (Root == null) return;
            
            IsVisible = false;
            Root.RemoveFromClassList("screen--visible");
            Root.AddToClassList("screen--hidden");
            Root.style.display = DisplayStyle.None;
            
            OnHidden();
            Hidden?.Invoke();
        }
        
        /// <summary>
        /// Handle back/escape input. Default behavior pops this screen.
        /// Override for custom behavior (e.g., confirmation dialogs).
        /// </summary>
        public virtual void HandleBack()
        {
            UIManager.Instance?.PopScreen();
        }
        
        // === Protected Methods ===
        
        /// <summary>
        /// Called when the screen is shown. Override for custom initialization.
        /// </summary>
        protected virtual void OnShown() { }
        
        /// <summary>
        /// Called when the screen is hidden. Override for custom cleanup.
        /// </summary>
        protected virtual void OnHidden() { }
        
        /// <summary>
        /// Set the root element (for screens that load UXML dynamically).
        /// </summary>
        protected void SetRoot(VisualElement root)
        {
            Root = root;
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.AddToClassList("screen");
            }
        }
        
        /// <summary>
        /// Query a child element by name.
        /// </summary>
        protected T Query<T>(string name = null) where T : VisualElement
        {
            return Root?.Q<T>(name);
        }
        
        /// <summary>
        /// Query a child element by name (non-generic).
        /// </summary>
        protected VisualElement Query(string name)
        {
            return Root?.Q<VisualElement>(name);
        }
    }
}
