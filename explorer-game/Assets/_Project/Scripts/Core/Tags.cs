namespace Explorer.Core
{
    /// <summary>
    /// Centralized tag constants to eliminate magic strings.
    /// Tags must be defined in Unity's Tag Manager to work.
    /// </summary>
    public static class Tags
    {
        /// <summary>The player character tag.</summary>
        public const string PLAYER = "Player";
        
        /// <summary>Main camera tag (Unity built-in).</summary>
        public const string MAIN_CAMERA = "MainCamera";
        
        /// <summary>Ground/terrain surfaces.</summary>
        public const string GROUND = "Ground";
        
        /// <summary>Interactable objects.</summary>
        public const string INTERACTABLE = "Interactable";
    }
    
    /// <summary>
    /// Centralized layer constants for physics queries.
    /// Layers must be defined in Unity's Layer settings to work.
    /// </summary>
    public static class Layers
    {
        /// <summary>Default layer (0).</summary>
        public const int DEFAULT = 0;
        
        /// <summary>Player layer for collision filtering.</summary>
        public const int PLAYER = 6;
        
        /// <summary>Ground/terrain layer.</summary>
        public const int GROUND = 7;
        
        /// <summary>Ship layer.</summary>
        public const int SHIP = 8;
        
        // === Layer Masks ===
        
        /// <summary>Mask for ground layer only.</summary>
        public static readonly int GROUND_MASK = 1 << GROUND;
        
        /// <summary>Mask for all walkable surfaces (Default + Ground).</summary>
        public static readonly int WALKABLE_MASK = (1 << DEFAULT) | (1 << GROUND);
    }
}
