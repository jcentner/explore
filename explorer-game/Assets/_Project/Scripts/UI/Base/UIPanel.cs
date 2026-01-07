using System;
using UnityEngine.UIElements;

namespace Explorer.UI
{
    /// <summary>
    /// Abstract base class for HUD panels (always-visible or contextual UI).
    /// Panels are registered with UIManager but not stacked like screens.
    /// 
    /// Usage:
    /// 1. Create a derived class (e.g., InteractionPromptPanel)
    /// 2. Override OnShown/OnHidden for custom behavior
    /// 3. Register with UIManager.RegisterPanel()
    /// </summary>
    public abstract class UIPanel
    {
        // === Properties ===
        
        /// <summary>The root VisualElement for this panel.</summary>
        public VisualElement Root { get; protected set; }
        
        /// <summary>Whether the panel is currently visible.</summary>
        public bool IsVisible { get; private set; }
        
        // === Events ===
        
        /// <summary>Fired when the panel is shown.</summary>
        public event Action Shown;
        
        /// <summary>Fired when the panel is hidden.</summary>
        public event Action Hidden;
        
        // === Constructor ===
        
        /// <summary>
        /// Create a panel with an existing VisualElement.
        /// </summary>
        /// <param name="root">The root element for this panel.</param>
        protected UIPanel(VisualElement root)
        {
            Root = root;
            
            // Start hidden by default
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.AddToClassList("panel");
            }
        }
        
        /// <summary>
        /// Create a panel that will set its root later.
        /// </summary>
        protected UIPanel()
        {
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Show the panel with optional animation.
        /// </summary>
        public virtual void Show()
        {
            if (Root == null) return;
            
            IsVisible = true;
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList("panel--hidden");
            Root.AddToClassList("panel--visible");
            
            OnShown();
            Shown?.Invoke();
        }
        
        /// <summary>
        /// Hide the panel with optional animation.
        /// </summary>
        public virtual void Hide()
        {
            if (Root == null) return;
            
            IsVisible = false;
            Root.RemoveFromClassList("panel--visible");
            Root.AddToClassList("panel--hidden");
            Root.style.display = DisplayStyle.None;
            
            OnHidden();
            Hidden?.Invoke();
        }
        
        /// <summary>
        /// Toggle visibility.
        /// </summary>
        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }
        
        // === Protected Methods ===
        
        /// <summary>
        /// Called when the panel is shown. Override for custom initialization.
        /// </summary>
        protected virtual void OnShown() { }
        
        /// <summary>
        /// Called when the panel is hidden. Override for custom cleanup.
        /// </summary>
        protected virtual void OnHidden() { }
        
        /// <summary>
        /// Set the root element (for panels that load UXML dynamically).
        /// </summary>
        protected void SetRoot(VisualElement root)
        {
            Root = root;
            if (Root != null)
            {
                Root.style.display = DisplayStyle.None;
                Root.AddToClassList("panel");
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
