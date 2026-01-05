using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Defines a gravity source attached to a celestial body (planet, moon, asteroid).
    /// Uses inverse-square falloff: g = baseStrength × (surfaceRadius² / distance²)
    /// Hard cutoff at MaxRange for performance.
    /// </summary>
    public class GravityBody : MonoBehaviour, IGravitySource
    {
        // === Inspector Fields ===
        [Header("Gravity Settings")]
        [SerializeField]
        [Tooltip("Gravity strength at the surface in m/s². Earth ≈ 9.8, Moon ≈ 1.6")]
        private float _baseStrength = 9.8f;

        [SerializeField]
        [Tooltip("Maximum range of gravity field in meters. Beyond this, gravity is zero (performance cutoff).")]
        private float _maxRange = 200f;

        [SerializeField]
        [Tooltip("Priority for tie-breaking overlapping fields. Higher wins. Only used for dominant source selection.")]
        private int _priority = 0;

        [SerializeField]
        [Tooltip("How this source participates in multi-body gravity accumulation.")]
        private GravityMode _mode = GravityMode.Both;

        [Header("Surface Detection")]
        [SerializeField]
        [Tooltip("If true, surface radius is auto-detected from SphereCollider.")]
        private bool _useColliderRadius = true;

        [SerializeField]
        [Tooltip("Manual surface radius if not using collider. Used for gravity calculations.")]
        private float _surfaceRadius = 50f;

        // === Public Properties (IGravitySource) ===
        public Vector3 GravityCenter => transform.position;
        public float BaseStrength => _baseStrength;
        public float MaxRange => _maxRange;
        public int Priority => _priority;
        public GravityMode Mode => _mode;

        /// <summary>
        /// The radius of the surface (for placing objects on the surface).
        /// </summary>
        public float SurfaceRadius => _useColliderRadius ? GetColliderRadius() : _surfaceRadius;

        /// <summary>
        /// Gravitational mass parameter, auto-derived from BaseStrength × SurfaceRadius².
        /// Used in inverse-square formula: g = Mass / r²
        /// </summary>
        public float Mass => _baseStrength * SurfaceRadius * SurfaceRadius;

        // === Private Fields ===
        private SphereCollider _sphereCollider;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _sphereCollider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            GravityManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            if (GravityManager.Instance != null)
            {
                GravityManager.Instance.Unregister(this);
            }
        }

        // === Public Methods (IGravitySource) ===

        /// <summary>
        /// Calculate gravity vector using inverse-square falloff.
        /// Formula: g = baseStrength × (surfaceRadius² / distance²)
        /// Clamped to baseStrength when inside surface to prevent infinite values.
        /// </summary>
        public Vector3 CalculateGravity(Vector3 worldPosition)
        {
            Vector3 toCenter = GravityCenter - worldPosition;
            float distance = toCenter.magnitude;

            // Outside range - no gravity (performance cutoff)
            if (distance > _maxRange)
            {
                return Vector3.zero;
            }

            // Prevent division by zero and clamp inside surface
            float effectiveDistance = Mathf.Max(distance, SurfaceRadius);

            // Inverse-square falloff: g = g₀ × (r₀² / r²)
            float surfaceRadiusSq = SurfaceRadius * SurfaceRadius;
            float distanceSq = effectiveDistance * effectiveDistance;
            float strength = _baseStrength * (surfaceRadiusSq / distanceSq);

            // Direction toward center, scaled by strength
            Vector3 direction = toCenter.normalized;
            return direction * strength;
        }

        /// <summary>
        /// Get a position on the surface of this body at the given direction.
        /// Useful for spawning objects on the surface.
        /// </summary>
        /// <param name="direction">Direction from center (will be normalized).</param>
        /// <returns>World position on the surface.</returns>
        public Vector3 GetSurfacePosition(Vector3 direction)
        {
            return GravityCenter + direction.normalized * SurfaceRadius;
        }

        // === Private Methods ===
        private float GetColliderRadius()
        {
            if (_sphereCollider != null)
            {
                // Account for scale
                float maxScale = Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.y,
                    transform.lossyScale.z
                );
                return _sphereCollider.radius * maxScale;
            }
            return _surfaceRadius;
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            float radius = SurfaceRadius;

            // Draw surface radius (solid green)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);

            // Draw gravity strength contours using inverse-square falloff
            // Show where gravity drops to 50%, 25%, and 10% of surface gravity
            float[] strengthFractions = { 0.5f, 0.25f, 0.1f };
            Color[] contourColors = {
                new Color(1f, 1f, 0f, 0.5f),    // Yellow - 50%
                new Color(1f, 0.5f, 0f, 0.4f),  // Orange - 25%
                new Color(1f, 0f, 0f, 0.3f)     // Red - 10%
            };

            for (int i = 0; i < strengthFractions.Length; i++)
            {
                // g = g₀ × (r₀² / r²) → r = r₀ / √fraction
                float contourRadius = radius / Mathf.Sqrt(strengthFractions[i]);
                if (contourRadius <= _maxRange)
                {
                    Gizmos.color = contourColors[i];
                    Gizmos.DrawWireSphere(transform.position, contourRadius);
                }
            }

            // Draw max gravity range (hard cutoff)
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _maxRange);
        }

        private void OnValidate()
        {
            // Ensure sensible values
            _baseStrength = Mathf.Max(0f, _baseStrength);
            _maxRange = Mathf.Max(1f, _maxRange);
            _surfaceRadius = Mathf.Max(0.1f, _surfaceRadius);
        }
#endif
    }
}
