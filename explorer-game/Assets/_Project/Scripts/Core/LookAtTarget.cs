using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Makes this object's forward direction always point toward a target.
    /// Useful for directional lights that should illuminate from a fixed source toward the player.
    /// </summary>
    [ExecuteInEditMode]
    public class LookAtTarget : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Target")]
        [SerializeField]
        [Tooltip("The transform to look at. If null, will search for Player tag on Start.")]
        private Transform _target;

        [Header("Options")]
        [SerializeField]
        [Tooltip("If true, will find the Player-tagged object on Start if no target is set.")]
        private bool _findPlayerOnStart = true;

        [SerializeField]
        [Tooltip("If true, updates in LateUpdate for smoother tracking after player movement.")]
        private bool _useLateUpdate = true;

        // === Unity Lifecycle ===
        private void Start()
        {
            if (_target == null && _findPlayerOnStart)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _target = player.transform;
                    Debug.Log($"[LookAtTarget] Found player: {_target.name} at {_target.position}");
                }
                else
                {
                    Debug.LogWarning($"[LookAtTarget] No Player found for {name}");
                }
            }
            
            // Force initial rotation
            UpdateRotation();
            Debug.Log($"[LookAtTarget] Initial rotation set. Forward: {transform.forward}");
        }

        private void Update()
        {
            if (!_useLateUpdate)
            {
                UpdateRotation();
            }
        }

        private void LateUpdate()
        {
            if (_useLateUpdate)
            {
                UpdateRotation();
            }
        }

        // === Private Methods ===
        private void UpdateRotation()
        {
            if (_target == null) return;

            // For directional lights: we want light to travel FROM this position TOWARD the target
            // Directional lights emit along their +Z axis (forward direction)
            Vector3 directionToTarget = _target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToTarget.normalized);
        }

        // === Public Methods ===
        /// <summary>
        /// Sets the target transform at runtime.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
