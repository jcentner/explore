using System;
using UnityEngine;

namespace Explorer.Player
{
    /// <summary>
    /// Interface for any vehicle that can be piloted.
    /// Implemented by ship input controllers to avoid circular dependencies.
    /// </summary>
    public interface IPilotable
    {
        /// <summary>Enable input for this vehicle.</summary>
        void EnableInput();
        
        /// <summary>Disable input for this vehicle.</summary>
        void DisableInput();
    }
}
