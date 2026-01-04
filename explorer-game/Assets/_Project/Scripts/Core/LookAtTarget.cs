using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Makes this object's forward direction always point toward a target.
    /// Useful for directional lights that should illuminate from a fixed source toward the player.
    /// </summary>
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
                }
                else
                {
                    Debug.LogWarning($"[LookAtTarget] No Player found for {name}");
                }
            }
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

            transform.LookAt(_target);
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
