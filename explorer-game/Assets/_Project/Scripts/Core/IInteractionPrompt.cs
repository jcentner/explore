using System;

namespace Explorer.Core
{
    /// <summary>
    /// Interface for interaction prompt display.
    /// Allows systems to show/hide prompts without direct UI dependencies.
    /// </summary>
    public interface IInteractionPrompt
    {
        /// <summary>Show the interaction prompt with specified text.</summary>
        /// <param name="text">The prompt text to display.</param>
        void Show(string text);
        
        /// <summary>Hide the interaction prompt.</summary>
        void Hide();
        
        /// <summary>Whether the prompt is currently visible.</summary>
        bool IsVisible { get; }
    }
    
    /// <summary>
    /// Service locator for interaction prompts.
    /// Allows decoupled access to UI systems.
    /// </summary>
    public static class InteractionPromptService
    {
        private static IInteractionPrompt _current;
        
        /// <summary>
        /// Register an interaction prompt implementation.
        /// Call this from UI system initialization.
        /// </summary>
        public static void Register(IInteractionPrompt prompt)
        {
            _current = prompt;
        }
        
        /// <summary>
        /// Unregister the current prompt (call on destruction).
        /// </summary>
        public static void Unregister(IInteractionPrompt prompt)
        {
            if (_current == prompt)
                _current = null;
        }
        
        /// <summary>
        /// Show an interaction prompt if one is registered.
        /// Safe to call even if no prompt system exists.
        /// </summary>
        public static void Show(string text)
        {
            _current?.Show(text);
        }
        
        /// <summary>
        /// Hide the interaction prompt if one is registered.
        /// Safe to call even if no prompt system exists.
        /// </summary>
        public static void Hide()
        {
            _current?.Hide();
        }
        
        /// <summary>
        /// Whether a prompt is registered and visible.
        /// </summary>
        public static bool IsVisible => _current?.IsVisible ?? false;
    }
}
