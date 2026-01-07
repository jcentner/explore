namespace Explorer.Core
{
    /// <summary>
    /// Interface for handling pause input.
    /// Allows gameplay code to trigger pause UI without direct UI assembly references.
    /// 
    /// Usage:
    /// - UI registers: UIService&lt;IPauseHandler&gt;.Register(this);
    /// - Gameplay invokes: UIService&lt;IPauseHandler&gt;.Instance?.HandlePause();
    /// </summary>
    public interface IPauseHandler
    {
        /// <summary>Handle pause input (toggle pause menu or handle back navigation).</summary>
        void HandlePause();
        
        /// <summary>Whether any UI screen is currently blocking gameplay.</summary>
        bool IsUIBlocking { get; }
    }
}
