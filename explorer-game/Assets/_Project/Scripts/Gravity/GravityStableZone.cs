using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Marks an explicit stable zone (Lagrange-like point) where gravity is near-zero.
    /// Can be used to:
    /// 1. Mark designer-placed stable points between bodies
    /// 2. Provide visual feedback in editor for gravity balance points
    /// 3. Optionally override gravity to exactly zero within radius
    /// </summary>
    public class GravityStableZone : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Zone Settings")]
        [SerializeField]
        [Tooltip("Radius of the stable zone in meters.")]
        private float _radius = 10f;

        [SerializeField]
        [Tooltip("If true, forces gravity to zero within this zone (overrides accumulation).")]
        private bool _forceZeroGravity = false;

        [SerializeField]
        [Tooltip("If true, smoothly blends gravity to zero near center (vs hard cutoff).")]
        private bool _smoothBlend = true;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Color for the gizmo visualization.")]
        private Color _gizmoColor = new Color(1f, 0f, 1f, 0.3f); // Magenta

        [SerializeField]
        [Tooltip("Show gravity field lines in editor.")]
        private bool _showFieldLines = true;

        // === Public Properties ===
        /// <summary>Center of the stable zone in world space.</summary>
        public Vector3 Center => transform.position;

        /// <summary>Radius of the stable zone.</summary>
        public float Radius => _radius;

        /// <summary>Whether this zone forces gravity to zero.</summary>
        public bool ForceZeroGravity => _forceZeroGravity;

        // === Public Methods ===

        /// <summary>
        /// Check if a position is inside this stable zone.
        /// </summary>
        public bool Contains(Vector3 worldPosition)
        {
            return Vector3.Distance(worldPosition, Center) <= _radius;
        }

        /// <summary>
        /// Get the gravity multiplier for a position (1 = full gravity, 0 = zero gravity).
        /// Only applies if ForceZeroGravity is true.
        /// </summary>
        public float GetGravityMultiplier(Vector3 worldPosition)
        {
            if (!_forceZeroGravity)
                return 1f;

            float distance = Vector3.Distance(worldPosition, Center);
            if (distance > _radius)
                return 1f;

            if (!_smoothBlend)
                return 0f;

            // Smooth blend: 0 at center, 1 at edge
            return distance / _radius;
        }

        /// <summary>
        /// Calculate the actual gravity at a position, accounting for this zone's influence.
        /// </summary>
        public Vector3 ModifyGravity(Vector3 worldPosition, Vector3 originalGravity)
        {
            float multiplier = GetGravityMultiplier(worldPosition);
            return originalGravity * multiplier;
        }

        // === Editor ===
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw zone sphere
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _radius);

            // Draw solid inner sphere (more opaque)
            Color solidColor = _gizmoColor;
            solidColor.a *= 0.3f;
            Gizmos.color = solidColor;
            Gizmos.DrawSphere(transform.position, _radius * 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw more detailed visualization when selected
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _radius);

            // Draw concentric rings showing blend zones
            if (_smoothBlend && _forceZeroGravity)
            {
                for (float t = 0.25f; t < 1f; t += 0.25f)
                {
                    Color ringColor = _gizmoColor;
                    ringColor.a = _gizmoColor.a * t;
                    Gizmos.color = ringColor;
                    Gizmos.DrawWireSphere(transform.position, _radius * t);
                }
            }

            // Draw field lines showing gravity flow
            if (_showFieldLines)
            {
                DrawFieldLines();
            }
        }

        private void DrawFieldLines()
        {
            // Sample gravity at points around the zone
            int samples = 8;
            float sampleRadius = _radius * 1.5f;

            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan

            for (int i = 0; i < samples; i++)
            {
                float angle = (i / (float)samples) * Mathf.PI * 2f;
                
                // Sample in XZ plane
                Vector3 samplePos = transform.position + new Vector3(
                    Mathf.Cos(angle) * sampleRadius,
                    0f,
                    Mathf.Sin(angle) * sampleRadius
                );

                // Get gravity at sample point
                var manager = GravityManager.Instance;
                if (manager != null)
                {
                    Vector3 gravity = manager.GetAccumulatedGravity(samplePos);
                    if (gravity.sqrMagnitude > 0.001f)
                    {
                        // Draw arrow showing gravity direction
                        Gizmos.DrawRay(samplePos, gravity.normalized * 5f);
                    }
                    else
                    {
                        // Draw dot for zero-g point
                        Gizmos.DrawSphere(samplePos, 0.5f);
                    }
                }
            }
        }

        private void OnValidate()
        {
            _radius = Mathf.Max(0.1f, _radius);
        }
#endif
    }
}
