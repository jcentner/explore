using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Defines a gravity source attached to a celestial body (planet, moon, asteroid).
    /// Uses linear falloff with hard cutoff at MaxRange.
    /// </summary>
    public class GravityBody : MonoBehaviour, IGravitySource
    {
        // === Inspector Fields ===
        [Header("Gravity Settings")]
        [SerializeField]
        [Tooltip("Gravity strength at the surface in m/s². Earth ≈ 9.8, Moon ≈ 1.6")]
        private float _baseStrength = 9.8f;

        [SerializeField]
        [Tooltip("Maximum range of gravity field in meters. Beyond this, gravity is zero.")]
        private float _maxRange = 200f;

        [SerializeField]
        [Tooltip("Priority for tie-breaking overlapping fields. Higher wins.")]
        private int _priority = 0;

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

        /// <summary>
        /// The radius of the surface (for placing objects on the surface).
        /// </summary>
        public float SurfaceRadius => _useColliderRadius ? GetColliderRadius() : _surfaceRadius;

        // === Private Fields ===
        private SphereCollider _sphereCollider;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _sphereCollider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            GravityManager.Instance.Register(this);
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
        /// Calculate gravity vector using linear falloff.
        /// Formula: strength = baseStrength * (1 - distance/maxRange)
        /// </summary>
        public Vector3 CalculateGravity(Vector3 worldPosition)
        {
            Vector3 toCenter = GravityCenter - worldPosition;
            float distance = toCenter.magnitude;

            // Outside range - no gravity
            if (distance > _maxRange || distance < 0.001f)
            {
                return Vector3.zero;
            }

            // Linear falloff
            float normalizedDistance = distance / _maxRange;
            float strength = _baseStrength * (1f - normalizedDistance);

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
            // Draw surface radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, SurfaceRadius);

            // Draw max gravity range
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
