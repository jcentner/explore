using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Marks a celestial body as capable of casting shadows on distant objects.
    /// Auto-registers with SolarSystemLightingManager on enable.
    /// </summary>
    public class DistantShadowCaster : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Shadow Configuration")]
        [SerializeField]
        [Tooltip("Radius used for shadow casting calculations. Should match the visible radius of the body.")]
        [Min(0f)]
        private float radius = 100f;

        [SerializeField]
        [Tooltip("If true, auto-calculate radius from transform scale on Awake.")]
        private bool autoCalculateRadius = true;

        // === Public Properties ===
        /// <summary>Shadow casting radius in world units.</summary>
        public float Radius => radius;

        // === Unity Lifecycle ===
        private void Awake()
        {
            if (autoCalculateRadius)
            {
                // Assume uniform scale sphere - use average of scale components divided by 2
                Vector3 scale = transform.lossyScale;
                radius = (scale.x + scale.y + scale.z) / 6f; // Average radius = average scale / 2
            }
        }

        private void OnEnable()
        {
            // Auto-register with manager
            if (SolarSystemLightingManager.Instance != null)
            {
                SolarSystemLightingManager.Instance.RegisterShadowCaster(this);
            }
            else
            {
                // Manager might not exist yet, try again next frame
                StartCoroutine(RegisterWhenReady());
            }
        }

        private void OnDisable()
        {
            // Auto-unregister from manager
            if (SolarSystemLightingManager.Instance != null)
            {
                SolarSystemLightingManager.Instance.UnregisterShadowCaster(this);
            }
        }

        // === Private Methods ===
        private System.Collections.IEnumerator RegisterWhenReady()
        {
            // Wait a frame for manager to initialize
            yield return null;

            if (SolarSystemLightingManager.Instance != null)
            {
                SolarSystemLightingManager.Instance.RegisterShadowCaster(this);
            }
            else
            {
                Debug.LogWarning($"[DistantShadowCaster] No SolarSystemLightingManager found. {name} will not cast shadows.");
            }
        }

        /// <summary>
        /// Manually sets the shadow radius.
        /// </summary>
        public void SetRadius(float newRadius)
        {
            radius = Mathf.Max(0f, newRadius);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualize shadow radius in editor
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
