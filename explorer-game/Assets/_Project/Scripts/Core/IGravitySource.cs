using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Implemented by objects that generate gravity fields (planets, moons, asteroids).
    /// </summary>
    public interface IGravitySource
    {
        /// <summary>
        /// Center point of the gravity field in world space.
        /// </summary>
        Vector3 GravityCenter { get; }

        /// <summary>
        /// Gravity strength at the surface in m/s².
        /// Earth ≈ 9.8, Moon ≈ 1.6
        /// </summary>
        float BaseStrength { get; }

        /// <summary>
        /// Maximum range of the gravity field in meters.
        /// Beyond this distance, gravity is zero.
        /// </summary>
        float MaxRange { get; }

        /// <summary>
        /// Priority for tie-breaking when entity is in overlapping fields.
        /// Higher priority wins.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Calculate the gravity vector for a given world position.
        /// Returns Vector3.zero if position is outside the gravity field.
        /// </summary>
        /// <param name="worldPosition">The position to calculate gravity for.</param>
        /// <returns>Gravity vector (direction × magnitude) in m/s².</returns>
        Vector3 CalculateGravity(Vector3 worldPosition);
    }
}
