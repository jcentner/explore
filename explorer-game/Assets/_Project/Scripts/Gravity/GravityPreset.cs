using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// ScriptableObject preset for gravity configuration.
    /// Allows designers to create and reuse gravity profiles for different body types.
    /// </summary>
    [CreateAssetMenu(fileName = "GravityPreset", menuName = "Explorer/Gravity Preset")]
    public class GravityPreset : ScriptableObject
    {
        [Header("Gravity Settings")]
        [SerializeField]
        [Tooltip("Gravity strength at the surface in m/s². Earth ≈ 9.8, Moon ≈ 1.6")]
        private float _baseSurfaceGravity = 9.8f;

        [SerializeField]
        [Tooltip("Maximum range as multiplier of surface radius. E.g., 10 means gravity extends 10× the body's radius.")]
        private float _rangeMultiplier = 10f;

        [SerializeField]
        [Tooltip("Priority for dominant source selection. Higher wins.")]
        private int _priority = 0;

        [SerializeField]
        [Tooltip("How this source participates in multi-body gravity accumulation.")]
        private GravityMode _mode = GravityMode.Both;

        [Header("Orientation Blending (Phase 2)")]
        [SerializeField]
        [Tooltip("Speed at which orientation blends when this becomes dominant (degrees/second).")]
        private float _orientationBlendSpeed = 90f;

        // === Public Properties ===
        /// <summary>Gravity at surface in m/s².</summary>
        public float BaseSurfaceGravity => _baseSurfaceGravity;

        /// <summary>MaxRange = SurfaceRadius × RangeMultiplier.</summary>
        public float RangeMultiplier => _rangeMultiplier;

        /// <summary>Priority for dominant source selection.</summary>
        public int Priority => _priority;

        /// <summary>How this source participates in accumulation.</summary>
        public GravityMode Mode => _mode;

        /// <summary>Orientation blend speed in degrees/second.</summary>
        public float OrientationBlendSpeed => _orientationBlendSpeed;

        /// <summary>
        /// Apply this preset's values to a GravityBody.
        /// </summary>
        /// <param name="body">The GravityBody to configure.</param>
        public void ApplyTo(GravityBody body)
        {
            if (body == null) return;

            // Use reflection or serialized property access in editor
            // For runtime, GravityBody would need public setters or an Apply method
            Debug.Log($"[GravityPreset] Would apply preset '{name}' to {body.name}. " +
                      $"Gravity: {_baseSurfaceGravity} m/s², Range: {_rangeMultiplier}× radius");
        }

        // === Static Presets ===
        /// <summary>
        /// Create a default Earth-like preset at runtime.
        /// </summary>
        public static GravityPreset CreateEarthLike()
        {
            var preset = CreateInstance<GravityPreset>();
            preset._baseSurfaceGravity = 9.8f;
            preset._rangeMultiplier = 10f;
            preset._priority = 0;
            preset._mode = GravityMode.Both;
            preset._orientationBlendSpeed = 90f;
            preset.name = "Earth-Like";
            return preset;
        }

        /// <summary>
        /// Create a Moon-like low gravity preset at runtime.
        /// </summary>
        public static GravityPreset CreateMoonLike()
        {
            var preset = CreateInstance<GravityPreset>();
            preset._baseSurfaceGravity = 1.6f;
            preset._rangeMultiplier = 8f;
            preset._priority = 0;
            preset._mode = GravityMode.Both;
            preset._orientationBlendSpeed = 60f;
            preset.name = "Moon-Like";
            return preset;
        }

        /// <summary>
        /// Create a micro-gravity asteroid preset at runtime.
        /// </summary>
        public static GravityPreset CreateAsteroid()
        {
            var preset = CreateInstance<GravityPreset>();
            preset._baseSurfaceGravity = 0.3f;
            preset._rangeMultiplier = 5f;
            preset._priority = 0;
            preset._mode = GravityMode.Both;
            preset._orientationBlendSpeed = 30f;
            preset.name = "Asteroid";
            return preset;
        }
    }
}
