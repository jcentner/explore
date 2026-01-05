using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Determines how a gravity source participates in multi-body accumulation.
    /// </summary>
    public enum GravityMode
    {
        /// <summary>Only considered for dominant source selection (legacy behavior).</summary>
        Dominant,
        /// <summary>Only contributes to accumulated gravity sum.</summary>
        Accumulate,
        /// <summary>Both contributes to sum AND can be selected as dominant.</summary>
        Both
    }

    /// <summary>
    /// Implemented by objects that generate gravity fields (planets, moons, asteroids).
    /// Supports multi-body accumulation with inverse-square falloff.
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
        /// Beyond this distance, gravity is zero (performance cutoff).
        /// </summary>
        float MaxRange { get; }

        /// <summary>
        /// Priority for tie-breaking when entity is in overlapping fields.
        /// Higher priority wins. Only used for dominant source selection.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Radius of the body's surface in meters.
        /// Used for inverse-square gravity calculations.
        /// </summary>
        float SurfaceRadius { get; }

        /// <summary>
        /// Gravitational mass parameter (derived from BaseStrength × SurfaceRadius²).
        /// Used for inverse-square formula: g = Mass / r²
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// Determines how this source participates in multi-body gravity.
        /// </summary>
        GravityMode Mode { get; }

        /// <summary>
        /// Calculate the gravity vector for a given world position.
        /// Uses inverse-square falloff: g = baseStrength × (surfaceRadius² / distance²)
        /// Returns Vector3.zero if position is outside MaxRange.
        /// </summary>
        /// <param name="worldPosition">The position to calculate gravity for.</param>
        /// <returns>Gravity vector (direction × magnitude) in m/s².</returns>
        Vector3 CalculateGravity(Vector3 worldPosition);
    }
}
