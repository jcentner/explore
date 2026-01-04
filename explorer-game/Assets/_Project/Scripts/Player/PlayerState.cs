namespace Explorer.Player
{
    /// <summary>
    /// High-level player states for the game loop.
    /// </summary>
    public enum PlayerState
    {
        /// <summary>Walking on foot, normal character controls.</summary>
        OnFoot,
        
        /// <summary>Transitioning into a ship (boarding animation/fade).</summary>
        BoardingShip,
        
        /// <summary>Piloting a ship, ship controls active.</summary>
        InShip,
        
        /// <summary>Transitioning out of a ship (disembarking).</summary>
        DisembarkingShip
    }
}
