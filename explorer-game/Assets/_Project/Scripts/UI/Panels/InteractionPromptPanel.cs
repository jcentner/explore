using UnityEngine;
using UnityEngine.UIElements;
using Explorer.Core;

namespace Explorer.UI
{
    /// <summary>
    /// UI Toolkit panel for displaying interaction prompts (e.g., "Press F to Board Ship").
    /// Implements IInteractionPrompt for decoupled access via UIService.
    /// 
    /// Required UXML structure:
    /// - KeyLabel: Label for the action key
    /// - ActionLabel: Label for the action text
    /// </summary>
    public class InteractionPromptPanel : UIPanel, IInteractionPrompt
    {
        // === Constants ===
        private const string HIDDEN_CLASS = "interaction-prompt--hidden";
        private const string VISIBLE_CLASS = "interaction-prompt--visible";
        
        // === UI Elements ===
        private Label _keyLabel;
        private Label _actionLabel;
        
        // === Configuration ===
        private readonly string _defaultKey = "F";
        private readonly string _defaultAction = "Interact";
        
        // === Constructor ===
        
        /// <summary>
        /// Create the interaction prompt panel from a template.
        /// </summary>
        /// <param name="templateContainer">The instantiated UXML template.</param>
        public InteractionPromptPanel(VisualElement templateContainer) : base()
        {
            // Find the root prompt element (first child of template)
            Root = templateContainer.Q<VisualElement>(className: "interaction-prompt");
            if (Root == null)
            {
                // Fallback: use the container itself
                Root = templateContainer;
            }
            
            // Query UI elements
            _keyLabel = Root.Q<Label>("KeyLabel");
            _actionLabel = Root.Q<Label>("ActionLabel");
            
            // Validate
            if (_keyLabel == null)
            {
                Debug.LogWarning("[InteractionPromptPanel] KeyLabel not found in template");
            }
            if (_actionLabel == null)
            {
                Debug.LogWarning("[InteractionPromptPanel] ActionLabel not found in template");
            }
            
            // Start hidden
            Root.AddToClassList(HIDDEN_CLASS);
            Root.style.display = DisplayStyle.None;
            
            // Register with service locator
            UIService<IInteractionPrompt>.Register(this);
        }
        
        // === IInteractionPrompt Implementation ===
        
        /// <summary>Show prompt with simple text message.</summary>
        public void Show(string text)
        {
            // Parse simple format: "Press [F] to board" or just show as-is
            if (_actionLabel != null)
            {
                _actionLabel.text = text;
            }
            if (_keyLabel != null)
            {
                _keyLabel.style.display = DisplayStyle.None;
            }
            
            ShowInternal();
        }
        
        /// <summary>Show prompt with key and context.</summary>
        public void Show(string actionKey, string context)
        {
            if (_keyLabel != null)
            {
                _keyLabel.text = actionKey;
                _keyLabel.style.display = DisplayStyle.Flex;
            }
            if (_actionLabel != null)
            {
                _actionLabel.text = context;
            }
            
            ShowInternal();
        }
        
        /// <summary>Show prompt with structured data.</summary>
        public void Show(InteractionPromptData data)
        {
            string key = string.IsNullOrEmpty(data.ActionKey) ? _defaultKey : data.ActionKey;
            
            // Build action text: "Board Ship" or just "Board"
            string action = data.ActionVerb ?? _defaultAction;
            if (!string.IsNullOrEmpty(data.TargetName))
            {
                action = $"{action} {data.TargetName}";
            }
            
            Show(key, action);
        }
        
        /// <summary>Hide the interaction prompt.</summary>
        public override void Hide()
        {
            if (Root == null) return;
            
            Root.RemoveFromClassList(VISIBLE_CLASS);
            Root.AddToClassList(HIDDEN_CLASS);
            
            // Delay hiding to allow transition
            Root.schedule.Execute(() =>
            {
                if (!IsVisible)
                {
                    Root.style.display = DisplayStyle.None;
                }
            }).StartingIn(250); // Match transition duration
            
            base.Hide();
        }
        
        // === Private Methods ===
        
        private void ShowInternal()
        {
            if (Root == null) return;
            
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList(HIDDEN_CLASS);
            Root.AddToClassList(VISIBLE_CLASS);
            
            // Use base class to set IsVisible and fire events
            // We need to call the base but avoid double visibility changes
            if (!IsVisible)
            {
                base.Show();
            }
        }
        
        // === Cleanup ===
        
        /// <summary>
        /// Call when the panel is being destroyed.
        /// </summary>
        public void Dispose()
        {
            UIService<IInteractionPrompt>.Unregister(this);
        }
    }
}
