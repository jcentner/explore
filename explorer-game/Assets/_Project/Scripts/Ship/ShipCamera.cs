using UnityEngine;

namespace Explorer.Ship
{
    /// <summary>
    /// Simple third-person camera that follows the ship.
    /// Temporary implementation for Phase 1 testing.
    /// Will be replaced with proper camera system in Phase 2.
    /// </summary>
    public class ShipCamera : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Target")]
        [SerializeField, Tooltip("Ship transform to follow")]
        private Transform _target;
        
        [SerializeField, Tooltip("Auto-find Ship_Prototype if target is null")]
        private bool _autoFindShip = true;
        
        [Header("Position")]
        [SerializeField, Tooltip("Offset from ship in local space")]
        private Vector3 _offset = new Vector3(0f, 5f, -15f);
        
        [SerializeField, Tooltip("Position smoothing speed")]
        private float _positionSmoothSpeed = 8f;
        
        [Header("Rotation")]
        [SerializeField, Tooltip("Rotation smoothing speed")]
        private float _rotationSmoothSpeed = 5f;
        
        [SerializeField, Tooltip("Look ahead distance")]
        private float _lookAheadDistance = 20f;
        
        // === Private Fields ===
        private Vector3 _currentVelocity;
        
        // === Unity Lifecycle ===
        private void Start()
        {
            if (_target == null && _autoFindShip)
            {
                var ship = GameObject.Find("Ship_Prototype");
                if (ship != null)
                {
                    _target = ship.transform;
                    Debug.Log("ShipCamera: Auto-found Ship_Prototype");
                    SnapToTarget();
                }
            }
        }
        
        private void LateUpdate()
        {
            if (_target == null)
                return;
            
            FollowTarget();
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Set the target to follow.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }
        
        /// <summary>
        /// Snap immediately to target position (no smoothing).
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null)
                return;
            
            transform.position = GetTargetPosition();
            transform.rotation = GetTargetRotation();
        }
        
        // === Private Methods ===
        
        private void FollowTarget()
        {
            // Smooth position
            Vector3 targetPos = GetTargetPosition();
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref _currentVelocity,
                1f / _positionSmoothSpeed
            );
            
            // Smooth rotation - look at point ahead of ship
            Quaternion targetRot = GetTargetRotation();
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * _rotationSmoothSpeed
            );
        }
        
        private Vector3 GetTargetPosition()
        {
            // Position behind and above ship in ship's local space
            return _target.TransformPoint(_offset);
        }
        
        private Quaternion GetTargetRotation()
        {
            // Look at point ahead of ship
            Vector3 lookAtPoint = _target.position + _target.forward * _lookAheadDistance;
            return Quaternion.LookRotation(lookAtPoint - transform.position, _target.up);
        }
    }
}
