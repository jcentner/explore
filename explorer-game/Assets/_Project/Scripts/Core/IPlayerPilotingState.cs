namespace Explorer.Core
{
    /// <summary>
    /// Interface for querying player piloting state.
    /// Allows gravity/UI systems to check if player is in a ship without circular dependencies.
    /// </summary>
    public interface IPlayerPilotingState
    {
        /// <summary>True if player is currently piloting a ship.</summary>
        bool IsPiloting { get; }
        
        /// <summary>The ship transform being piloted (null if not piloting).</summary>
        UnityEngine.Transform CurrentShip { get; }
    }
    
    /// <summary>
    /// Service locator for player piloting state.
    /// Allows decoupled access to player state without assembly dependencies.
    /// </summary>
    public static class PlayerPilotingService
    {
        private static IPlayerPilotingState _current;
        
        /// <summary>
        /// Register a player piloting state provider.
        /// Call this from PlayerStateController initialization.
        /// </summary>
        public static void Register(IPlayerPilotingState provider)
        {
            _current = provider;
        }
        
        /// <summary>
        /// Unregister the current provider (call on destruction).
        /// </summary>
        public static void Unregister(IPlayerPilotingState provider)
        {
            if (_current == provider)
                _current = null;
        }
        
        /// <summary>
        /// Check if player is currently piloting a ship.
        /// Returns false if no provider is registered.
        /// </summary>
        public static bool IsPiloting => _current?.IsPiloting ?? false;
        
        /// <summary>
        /// Get the current ship being piloted.
        /// Returns null if not piloting or no provider registered.
        /// </summary>
        public static UnityEngine.Transform CurrentShip => _current?.CurrentShip;
    }
}
