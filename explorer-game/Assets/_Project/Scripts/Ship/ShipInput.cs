using UnityEngine;
using Explorer.Player;

namespace Explorer.Ship
{
    /// <summary>
    /// Bridges InputReader ship actions to ShipController.
    /// Reads input each frame and passes it to the controller.
    /// </summary>
    [RequireComponent(typeof(ShipController))]
    public class ShipInput : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Input")]
        [SerializeField, Tooltip("Input reader asset")]
        private InputReader _inputReader;
        
        [Header("Mouse Sensitivity")]
        [SerializeField, Tooltip("Mouse look sensitivity multiplier")]
        private float _mouseSensitivity = 0.1f;
        
        [Header("Debug")]
        [SerializeField, Tooltip("Enable ship input on start (for testing without boarding)")]
        private bool _enableOnStart = false;
        
        // === Public Properties ===
        /// <summary>True if this ship is currently receiving input</summary>
        public bool IsReceivingInput => _isReceivingInput;
        
        // === Private Fields ===
        private ShipController _controller;
        private bool _isReceivingInput;
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            _controller = GetComponent<ShipController>();
            
            // Try to find InputReader if not assigned
            if (_inputReader == null)
            {
                _inputReader = Resources.Load<InputReader>("InputReader");
            }
        }
        
        private void Start()
        {
            if (_enableOnStart && _inputReader != null)
            {
                EnableInput();
            }
        }
        
        private void Update()
        {
            if (!_isReceivingInput || _inputReader == null)
                return;
            
            ReadAndApplyInput();
        }
        
        private void OnDestroy()
        {
            if (_isReceivingInput)
            {
                DisableInput();
            }
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Start receiving input for this ship.
        /// Enables Ship action map and subscribes to events.
        /// </summary>
        public void EnableInput()
        {
            if (_isReceivingInput)
                return;
            
            if (_inputReader == null)
            {
                Debug.LogError("ShipInput: No InputReader assigned!");
                return;
            }
            
            _isReceivingInput = true;
            _inputReader.EnableShipInput();
            _inputReader.OnShipExit += HandleShipExit;
            
            // Lock cursor for flight
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        /// <summary>
        /// Stop receiving input for this ship.
        /// Disables Ship action map and unsubscribes from events.
        /// </summary>
        public void DisableInput()
        {
            if (!_isReceivingInput)
                return;
            
            _isReceivingInput = false;
            
            if (_inputReader != null)
            {
                _inputReader.DisableShipInput();
                _inputReader.OnShipExit -= HandleShipExit;
            }
            
            // Clear controller input
            _controller.SetThrustInput(Vector3.zero);
            _controller.SetRotationInput(Vector3.zero);
            _controller.SetBoost(false);
            _controller.SetBrake(false);
        }
        
        // === Private Methods ===
        
        private void ReadAndApplyInput()
        {
            // Build thrust vector (local space)
            // InputReader: X=strafe, Y=forward from WASD
            // ShipController expects: X=strafe, Y=vertical, Z=forward
            Vector3 thrust = new Vector3(
                _inputReader.ShipThrustInput.x,  // A/D strafe
                _inputReader.ShipVerticalInput,   // Ctrl/Shift vertical
                _inputReader.ShipThrustInput.y    // W/S forward
            );
            _controller.SetThrustInput(thrust);
            
            // Build rotation input
            // InputReader: X=yaw (mouse horizontal), Y=pitch (mouse vertical)
            // ShipController expects: X=pitch, Y=yaw, Z=roll
            Vector2 look = _inputReader.ShipLookInput * _mouseSensitivity;
            Vector3 rotation = new Vector3(
                look.y,                      // Pitch (mouse Y)
                look.x,                      // Yaw (mouse X)
                _inputReader.ShipRollInput   // Roll (Q/E)
            );
            _controller.SetRotationInput(rotation);
            
            // Boost and brake
            _controller.SetBoost(_inputReader.ShipBoostHeld);
            _controller.SetBrake(_inputReader.ShipBrakeHeld);
        }
        
        private void HandleShipExit()
        {
            // For now, just log. Phase 2 will implement actual exit logic.
            Debug.Log("ShipInput: Exit pressed - implement in Phase 2 (PlayerStateController)");
        }
    }
}
