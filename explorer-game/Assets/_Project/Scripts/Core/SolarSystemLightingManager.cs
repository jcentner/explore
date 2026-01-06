using System.Collections.Generic;
using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Sets global shader properties for solar system lighting each frame.
    /// Manages a registry of shadow casters for eclipse/shadow calculations.
    /// </summary>
    public class SolarSystemLightingManager : MonoBehaviour
    {
        // === Constants ===
        public const int MAX_SHADOW_CASTERS = 8;

        // === Shader Property IDs (cached for performance) ===
        private static readonly int SunPositionId = Shader.PropertyToID("_SunPosition");
        private static readonly int SunColorId = Shader.PropertyToID("_SunColor");
        private static readonly int SunIntensityId = Shader.PropertyToID("_SunIntensity");
        private static readonly int SpaceAmbientId = Shader.PropertyToID("_SpaceAmbient");
        private static readonly int ShadowCasterCountId = Shader.PropertyToID("_ShadowCasterCount");
        private static readonly int ShadowCasterPositionsId = Shader.PropertyToID("_ShadowCasterPositions");
        private static readonly int ShadowCasterRadiiId = Shader.PropertyToID("_ShadowCasterRadii");

        // === Inspector Fields ===
        [Header("Sun Configuration")]
        [SerializeField]
        [Tooltip("Transform representing the sun's position in the scene")]
        private Transform sunTransform;

        [SerializeField]
        [Tooltip("Color of the sunlight (warm white default)")]
        private Color sunColor = new Color(1f, 0.95f, 0.9f, 1f);

        [SerializeField]
        [Tooltip("Intensity multiplier for sunlight")]
        [Range(0f, 10f)]
        private float sunIntensity = 1f;

        [Header("Ambient Configuration")]
        [SerializeField]
        [Tooltip("Ambient color for unlit areas in space (very dark)")]
        private Color spaceAmbient = new Color(0.02f, 0.02f, 0.04f, 1f);

        // === Public Properties ===
        /// <summary>World position of the sun.</summary>
        public Vector3 SunPosition => sunTransform != null ? sunTransform.position : Vector3.zero;

        /// <summary>Current sun color.</summary>
        public Color SunColor => sunColor;

        /// <summary>Current sun intensity.</summary>
        public float SunIntensity => sunIntensity;

        /// <summary>Singleton instance for easy access.</summary>
        public static SolarSystemLightingManager Instance { get; private set; }

        // === Private Fields ===
        private readonly List<DistantShadowCaster> _shadowCasters = new List<DistantShadowCaster>();
        private Vector4[] _shadowCasterPositions;
        private float[] _shadowCasterRadii;

        // === Unity Lifecycle ===
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[SolarSystemLightingManager] Duplicate instance found on {gameObject.name}, destroying.");
                Destroy(this);
                return;
            }
            Instance = this;

            // Pre-allocate arrays for shader data
            _shadowCasterPositions = new Vector4[MAX_SHADOW_CASTERS];
            _shadowCasterRadii = new float[MAX_SHADOW_CASTERS];
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            UpdateGlobalShaderProperties();
        }

        // === Public Methods ===

        /// <summary>
        /// Registers a shadow caster to be included in shadow calculations.
        /// </summary>
        /// <param name="caster">The shadow caster to register.</param>
        public void RegisterShadowCaster(DistantShadowCaster caster)
        {
            if (caster == null) return;

            if (_shadowCasters.Count >= MAX_SHADOW_CASTERS)
            {
                Debug.LogWarning($"[SolarSystemLightingManager] Max shadow casters ({MAX_SHADOW_CASTERS}) reached. Ignoring {caster.name}.");
                return;
            }

            if (!_shadowCasters.Contains(caster))
            {
                _shadowCasters.Add(caster);
                Debug.Log($"[SolarSystemLightingManager] Registered shadow caster: {caster.name}");
            }
        }

        /// <summary>
        /// Unregisters a shadow caster from shadow calculations.
        /// </summary>
        /// <param name="caster">The shadow caster to unregister.</param>
        public void UnregisterShadowCaster(DistantShadowCaster caster)
        {
            if (caster == null) return;

            if (_shadowCasters.Remove(caster))
            {
                Debug.Log($"[SolarSystemLightingManager] Unregistered shadow caster: {caster.name}");
            }
        }

        /// <summary>
        /// Sets the sun transform at runtime.
        /// </summary>
        public void SetSunTransform(Transform sun)
        {
            sunTransform = sun;
        }

        /// <summary>
        /// Sets sun color and intensity at runtime.
        /// </summary>
        public void SetSunProperties(Color color, float intensity)
        {
            sunColor = color;
            sunIntensity = intensity;
        }

        // === Private Methods ===

        private void UpdateGlobalShaderProperties()
        {
            // Sun properties
            Vector3 sunPos = sunTransform != null ? sunTransform.position : Vector3.zero;
            Shader.SetGlobalVector(SunPositionId, sunPos);
            Shader.SetGlobalColor(SunColorId, sunColor);
            Shader.SetGlobalFloat(SunIntensityId, sunIntensity);
            Shader.SetGlobalColor(SpaceAmbientId, spaceAmbient);

            // Shadow caster properties - check if arrays are initialized
            if (_shadowCasterPositions == null || _shadowCasterRadii == null)
            {
                Shader.SetGlobalInt(ShadowCasterCountId, 0);
                return;
            }

            int casterCount = Mathf.Min(_shadowCasters.Count, MAX_SHADOW_CASTERS);
            Shader.SetGlobalInt(ShadowCasterCountId, casterCount);

            // Clear arrays
            for (int i = 0; i < MAX_SHADOW_CASTERS; i++)
            {
                _shadowCasterPositions[i] = Vector4.zero;
                _shadowCasterRadii[i] = 0f;
            }

            // Fill with active casters
            for (int i = 0; i < casterCount; i++)
            {
                var caster = _shadowCasters[i];
                if (caster != null)
                {
                    Vector3 pos = caster.transform.position;
                    _shadowCasterPositions[i] = new Vector4(pos.x, pos.y, pos.z, 0f);
                    _shadowCasterRadii[i] = caster.Radius;
                }
            }

            Shader.SetGlobalVectorArray(ShadowCasterPositionsId, _shadowCasterPositions);
            Shader.SetGlobalFloatArray(ShadowCasterRadiiId, _shadowCasterRadii);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Immediate update in editor when values change
            if (Application.isPlaying)
            {
                UpdateGlobalShaderProperties();
            }
        }
#endif
    }
}
