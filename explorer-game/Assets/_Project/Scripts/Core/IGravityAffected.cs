using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Implemented by entities that respond to gravity (player, ship, physics objects).
    /// </summary>
    public interface IGravityAffected
    {
        /// <summary>
        /// Current gravity vector being applied to this entity.
        /// </summary>
        Vector3 CurrentGravity { get; }

        /// <summary>
        /// The dominant gravity source currently affecting this entity.
        /// Null if no gravity sources are in range.
        /// </summary>
        IGravitySource DominantSource { get; }

        /// <summary>
        /// Whether gravity is currently enabled for this entity.
        /// When false, entity ignores all gravity fields.
        /// </summary>
        bool GravityEnabled { get; set; }

        /// <summary>
        /// Local "up" direction for this entity (opposite of gravity).
        /// Returns Vector3.up if no gravity is active.
        /// </summary>
        Vector3 LocalUp { get; }
    }
}
