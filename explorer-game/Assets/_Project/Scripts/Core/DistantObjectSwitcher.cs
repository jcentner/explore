using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Switches between real-lit and distant-shader versions of an object
    /// based on distance from the player/camera.
    /// </summary>
    public class DistantObjectSwitcher : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("LOD Objects")]
        [SerializeField]
        [Tooltip("The version rendered with real URP lighting (near camera)")]
        private GameObject nearVersion;

        [SerializeField]
        [Tooltip("The version rendered with distant shader (far from camera)")]
        private GameObject distantVersion;

        [Header("Distance Settings")]
        [SerializeField]
        [Tooltip("Distance at which to switch from near to distant version")]
        [Min(0f)]
        private float switchDistance = 500f;

        [SerializeField]
        [Tooltip("Buffer to prevent flip-flopping at the boundary")]
        [Min(0f)]
        private float hysteresis = 50f;

        [Header("Target")]
        [SerializeField]
        [Tooltip("Transform to measure distance from (defaults to main camera if null)")]
        private Transform distanceTarget;

        // === Private Fields ===
        private bool _isShowingDistant;
        private Camera _mainCamera;

        // === Public Properties ===
        /// <summary>Whether the distant version is currently active.</summary>
        public bool IsShowingDistant => _isShowingDistant;

        /// <summary>Current distance to target.</summary>
        public float CurrentDistance { get; private set; }

        // === Unity Lifecycle ===
        private void Awake()
        {
            _mainCamera = Camera.main;

            // Validate setup
            if (nearVersion == null || distantVersion == null)
            {
                Debug.LogError($"[DistantObjectSwitcher] {name}: Missing near or distant version reference.");
                enabled = false;
                return;
            }

            // Initialize to near version
            SetNearVersion();
        }

        private void Update()
        {
            UpdateLOD();
        }

        // === Private Methods ===
        private void UpdateLOD()
        {
            // Get distance target
            Transform target = distanceTarget;
            if (target == null)
            {
                // Try to get camera - might not be available in Awake
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                if (_mainCamera != null)
                {
                    target = _mainCamera.transform;
                }
            }

            if (target == null)
            {
                // No valid target, keep current state
                return;
            }

            // Calculate distance
            CurrentDistance = Vector3.Distance(transform.position, target.position);

            // Apply hysteresis to prevent flip-flopping
            if (_isShowingDistant)
            {
                // Currently distant, switch to near if closer than (switchDistance - hysteresis)
                if (CurrentDistance < switchDistance - hysteresis)
                {
                    SetNearVersion();
                }
            }
            else
            {
                // Currently near, switch to distant if farther than (switchDistance + hysteresis)
                if (CurrentDistance > switchDistance + hysteresis)
                {
                    SetDistantVersion();
                }
            }
        }

        private void SetNearVersion()
        {
            _isShowingDistant = false;
            if (nearVersion != null) nearVersion.SetActive(true);
            if (distantVersion != null) distantVersion.SetActive(false);
        }

        private void SetDistantVersion()
        {
            _isShowingDistant = true;
            if (nearVersion != null) nearVersion.SetActive(false);
            if (distantVersion != null) distantVersion.SetActive(true);
        }

        /// <summary>
        /// Sets the switch distance at runtime.
        /// </summary>
        public void SetSwitchDistance(float distance)
        {
            switchDistance = Mathf.Max(0f, distance);
        }

        /// <summary>
        /// Sets the distance target at runtime.
        /// </summary>
        public void SetDistanceTarget(Transform target)
        {
            distanceTarget = target;
        }

        /// <summary>
        /// Forces an immediate LOD update.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateLOD();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualize switch distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, switchDistance);

            // Visualize hysteresis band
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, switchDistance - hysteresis);
            Gizmos.DrawWireSphere(transform.position, switchDistance + hysteresis);
        }
#endif
    }
}
