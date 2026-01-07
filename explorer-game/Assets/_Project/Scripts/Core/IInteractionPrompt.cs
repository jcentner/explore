namespace Explorer.Core
{
    /// <summary>
    /// Interface for interaction prompt display.
    /// Allows systems to show/hide prompts without direct UI dependencies.
    /// 
    /// Usage:
    /// - Register: UIService&lt;IInteractionPrompt&gt;.Register(this);
    /// - Consume: UIService&lt;IInteractionPrompt&gt;.Instance?.Show("Press F to board");
    /// </summary>
    public interface IInteractionPrompt
    {
        /// <summary>Show the interaction prompt with specified text.</summary>
        /// <param name="text">The prompt text to display.</param>
        void Show(string text);
        
        /// <summary>Show the interaction prompt with action key and context.</summary>
        /// <param name="actionKey">The key to display (e.g., "F").</param>
        /// <param name="context">The action context (e.g., "Board Ship").</param>
        void Show(string actionKey, string context);
        
        /// <summary>Show the interaction prompt with detailed data.</summary>
        /// <param name="data">Structured prompt data.</param>
        void Show(InteractionPromptData data);
        
        /// <summary>Hide the interaction prompt.</summary>
        void Hide();
        
        /// <summary>Whether the prompt is currently visible.</summary>
        bool IsVisible { get; }
    }
    
    /// <summary>
    /// Structured data for interaction prompts.
    /// Supports key rebinding and detailed action descriptions.
    /// </summary>
    public struct InteractionPromptData
    {
        /// <summary>The key or icon name to display (e.g., "F", "E").</summary>
        public string ActionKey;
        
        /// <summary>The action verb (e.g., "Board", "Pick up", "Open").</summary>
        public string ActionVerb;
        
        /// <summary>The target name (e.g., "Ship", "Artifact", "Gate").</summary>
        public string TargetName;
        
        /// <summary>If true, resolve ActionKey from Input System binding.</summary>
        public bool UseInputBinding;
        
        /// <summary>Create prompt data with just action and target.</summary>
        public InteractionPromptData(string actionKey, string actionVerb, string targetName = null)
        {
            ActionKey = actionKey;
            ActionVerb = actionVerb;
            TargetName = targetName;
            UseInputBinding = false;
        }
    }
}
