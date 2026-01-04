using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Explorer.Player
{
    /// <summary>
    /// ScriptableObject that decouples input from consumers.
    /// Uses Unity's Input System via InputActionAsset reference.
    /// Consumers subscribe to events or poll input values.
    /// Supports both Player (on-foot) and Ship action maps.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Explorer/Input Reader")]
    public class InputReader : ScriptableObject
    {
        // === Input Action Asset ===
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private string _playerMapName = "Player";
        [SerializeField] private string _shipMapName = "Ship";

        // === Cached Player Actions ===
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _interactAction;

        // === Cached Ship Actions ===
        private InputAction _shipThrustAction;
        private InputAction _shipVerticalAction;
        private InputAction _shipLookAction;
        private InputAction _shipRollAction;
        private InputAction _shipBrakeAction;
        private InputAction _shipBoostAction;
        private InputAction _shipExitAction;

        // === Player Properties ===
        
        /// <summary>
        /// Current movement input (WASD/Left Stick). Normalized.
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>
        /// Current look input (Mouse Delta/Right Stick).
        /// </summary>
        public Vector2 LookInput { get; private set; }

        /// <summary>
        /// Whether sprint/run is held.
        /// </summary>
        public bool SprintHeld { get; private set; }

        // === Ship Properties ===
        
        /// <summary>
        /// Ship thrust input (WASD/Left Stick). X=strafe, Y=forward.
        /// </summary>
        public Vector2 ShipThrustInput { get; private set; }
        
        /// <summary>
        /// Ship vertical thrust (Ctrl/Shift). -1 down, +1 up.
        /// </summary>
        public float ShipVerticalInput { get; private set; }
        
        /// <summary>
        /// Ship look input (Mouse Delta/Right Stick). X=yaw, Y=pitch.
        /// </summary>
        public Vector2 ShipLookInput { get; private set; }
        
        /// <summary>
        /// Ship roll input (Q/E). -1 left roll, +1 right roll.
        /// </summary>
        public float ShipRollInput { get; private set; }
        
        /// <summary>
        /// Whether ship brake is held.
        /// </summary>
        public bool ShipBrakeHeld { get; private set; }
        
        /// <summary>
        /// Whether ship boost is held.
        /// </summary>
        public bool ShipBoostHeld { get; private set; }

        // === Player Events ===
        
        /// <summary>
        /// Fired when jump is pressed.
        /// </summary>
        public event Action OnJump;

        /// <summary>
        /// Fired when interact is pressed.
        /// </summary>
        public event Action OnInteract;

        /// <summary>
        /// Fired when pause is pressed.
        /// </summary>
        public event Action OnPause;

        // === Ship Events ===
        
        /// <summary>
        /// Fired when ship exit is pressed (F key).
        /// </summary>
        public event Action OnShipExit;

        // === Private Fields ===
        private bool _isPlayerEnabled;
        private bool _isShipEnabled;

        // === Unity Lifecycle ===
        
        /// <summary>
        /// Reset state when ScriptableObject is enabled (entering play mode).
        /// </summary>
        private void OnEnable()
        {
            // Reset flags - ScriptableObjects persist state between play sessions
            _isPlayerEnabled = false;
            _isShipEnabled = false;
            
            // Clear cached actions
            _moveAction = null;
            _lookAction = null;
            _jumpAction = null;
            _sprintAction = null;
            _interactAction = null;
            _shipThrustAction = null;
            _shipVerticalAction = null;
            _shipLookAction = null;
            _shipRollAction = null;
            _shipBrakeAction = null;
            _shipBoostAction = null;
            _shipExitAction = null;
            
            // Reset input values
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            SprintHeld = false;
            ShipThrustInput = Vector2.zero;
            ShipVerticalInput = 0f;
            ShipLookInput = Vector2.zero;
            ShipRollInput = 0f;
            ShipBrakeHeld = false;
            ShipBoostHeld = false;
        }

        // === Public Methods ===

        /// <summary>
        /// Enable player input actions.
        /// Call this when player should receive input.
        /// Automatically disables ship input.
        /// </summary>
        public void EnablePlayerInput()
        {
            // Disable ship input first to ensure exclusivity
            if (_isShipEnabled)
            {
                DisableShipInput();
            }
            
            if (_isPlayerEnabled)
                return;

            EnsureInputActionsLoaded();

            if (_inputActions == null)
            {
                Debug.LogError("InputReader: No InputActionAsset assigned and couldn't find 'InputSystem_Actions' in Resources!");
                return;
            }

            _isPlayerEnabled = true;
            
            // Find action map
            var playerMap = _inputActions.FindActionMap(_playerMapName);
            if (playerMap == null)
            {
                Debug.LogError($"InputReader: Action map '{_playerMapName}' not found!");
                return;
            }

            // Cache and subscribe to actions
            _moveAction = playerMap.FindAction("Move");
            _lookAction = playerMap.FindAction("Look");
            _jumpAction = playerMap.FindAction("Jump");
            _sprintAction = playerMap.FindAction("Sprint");
            _interactAction = playerMap.FindAction("Interact");

            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed += OnLookPerformed;
                _lookAction.canceled += OnLookCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJumpPerformed;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprintPerformed;
                _sprintAction.canceled += OnSprintCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
            }

            playerMap.Enable();
        }

        /// <summary>
        /// Disable player input actions.
        /// Call this during cutscenes, menus, etc.
        /// </summary>
        public void DisablePlayerInput()
        {
            if (!_isPlayerEnabled)
                return;

            _isPlayerEnabled = false;

            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= OnLookPerformed;
                _lookAction.canceled -= OnLookCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed -= OnSprintPerformed;
                _sprintAction.canceled -= OnSprintCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
            }

            var playerMap = _inputActions?.FindActionMap(_playerMapName);
            playerMap?.Disable();

            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            SprintHeld = false;
        }

        /// <summary>
        /// Enable ship input actions.
        /// Call this when player is piloting a ship.
        /// Automatically disables player input.
        /// </summary>
        public void EnableShipInput()
        {
            // Disable player input first to ensure exclusivity
            if (_isPlayerEnabled)
            {
                DisablePlayerInput();
            }
            
            if (_isShipEnabled)
                return;

            EnsureInputActionsLoaded();

            if (_inputActions == null)
            {
                Debug.LogError("InputReader: No InputActionAsset assigned and couldn't find 'InputSystem_Actions' in Resources!");
                return;
            }

            _isShipEnabled = true;
            
            // Find action map
            var shipMap = _inputActions.FindActionMap(_shipMapName);
            if (shipMap == null)
            {
                Debug.LogError($"InputReader: Action map '{_shipMapName}' not found!");
                return;
            }

            // Cache and subscribe to ship actions
            _shipThrustAction = shipMap.FindAction("Thrust");
            _shipVerticalAction = shipMap.FindAction("Vertical");
            _shipLookAction = shipMap.FindAction("Look");
            _shipRollAction = shipMap.FindAction("Roll");
            _shipBrakeAction = shipMap.FindAction("Brake");
            _shipBoostAction = shipMap.FindAction("Boost");
            _shipExitAction = shipMap.FindAction("Exit");

            if (_shipThrustAction != null)
            {
                _shipThrustAction.performed += OnShipThrustPerformed;
                _shipThrustAction.canceled += OnShipThrustCanceled;
            }

            if (_shipVerticalAction != null)
            {
                _shipVerticalAction.performed += OnShipVerticalPerformed;
                _shipVerticalAction.canceled += OnShipVerticalCanceled;
            }

            if (_shipLookAction != null)
            {
                _shipLookAction.performed += OnShipLookPerformed;
                _shipLookAction.canceled += OnShipLookCanceled;
            }

            if (_shipRollAction != null)
            {
                _shipRollAction.performed += OnShipRollPerformed;
                _shipRollAction.canceled += OnShipRollCanceled;
            }

            if (_shipBrakeAction != null)
            {
                _shipBrakeAction.performed += OnShipBrakePerformed;
                _shipBrakeAction.canceled += OnShipBrakeCanceled;
            }

            if (_shipBoostAction != null)
            {
                _shipBoostAction.performed += OnShipBoostPerformed;
                _shipBoostAction.canceled += OnShipBoostCanceled;
            }

            if (_shipExitAction != null)
            {
                _shipExitAction.performed += OnShipExitPerformed;
            }

            shipMap.Enable();
        }

        /// <summary>
        /// Disable ship input actions.
        /// Call this when player exits ship.
        /// </summary>
        public void DisableShipInput()
        {
            if (!_isShipEnabled)
                return;

            _isShipEnabled = false;

            if (_shipThrustAction != null)
            {
                _shipThrustAction.performed -= OnShipThrustPerformed;
                _shipThrustAction.canceled -= OnShipThrustCanceled;
            }

            if (_shipVerticalAction != null)
            {
                _shipVerticalAction.performed -= OnShipVerticalPerformed;
                _shipVerticalAction.canceled -= OnShipVerticalCanceled;
            }

            if (_shipLookAction != null)
            {
                _shipLookAction.performed -= OnShipLookPerformed;
                _shipLookAction.canceled -= OnShipLookCanceled;
            }

            if (_shipRollAction != null)
            {
                _shipRollAction.performed -= OnShipRollPerformed;
                _shipRollAction.canceled -= OnShipRollCanceled;
            }

            if (_shipBrakeAction != null)
            {
                _shipBrakeAction.performed -= OnShipBrakePerformed;
                _shipBrakeAction.canceled -= OnShipBrakeCanceled;
            }

            if (_shipBoostAction != null)
            {
                _shipBoostAction.performed -= OnShipBoostPerformed;
                _shipBoostAction.canceled -= OnShipBoostCanceled;
            }

            if (_shipExitAction != null)
            {
                _shipExitAction.performed -= OnShipExitPerformed;
            }

            var shipMap = _inputActions?.FindActionMap(_shipMapName);
            shipMap?.Disable();

            ShipThrustInput = Vector2.zero;
            ShipVerticalInput = 0f;
            ShipLookInput = Vector2.zero;
            ShipRollInput = 0f;
            ShipBrakeHeld = false;
            ShipBoostHeld = false;
        }

        // === Input Callbacks ===

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MoveInput = Vector2.zero;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            LookInput = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            OnJump?.Invoke();
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            SprintHeld = true;
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            SprintHeld = false;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            OnInteract?.Invoke();
        }

        // === Ship Input Callbacks ===

        private void OnShipThrustPerformed(InputAction.CallbackContext context)
        {
            ShipThrustInput = context.ReadValue<Vector2>();
        }

        private void OnShipThrustCanceled(InputAction.CallbackContext context)
        {
            ShipThrustInput = Vector2.zero;
        }

        private void OnShipVerticalPerformed(InputAction.CallbackContext context)
        {
            ShipVerticalInput = context.ReadValue<float>();
        }

        private void OnShipVerticalCanceled(InputAction.CallbackContext context)
        {
            ShipVerticalInput = 0f;
        }

        private void OnShipLookPerformed(InputAction.CallbackContext context)
        {
            ShipLookInput = context.ReadValue<Vector2>();
        }

        private void OnShipLookCanceled(InputAction.CallbackContext context)
        {
            ShipLookInput = Vector2.zero;
        }

        private void OnShipRollPerformed(InputAction.CallbackContext context)
        {
            ShipRollInput = context.ReadValue<float>();
        }

        private void OnShipRollCanceled(InputAction.CallbackContext context)
        {
            ShipRollInput = 0f;
        }

        private void OnShipBrakePerformed(InputAction.CallbackContext context)
        {
            ShipBrakeHeld = true;
        }

        private void OnShipBrakeCanceled(InputAction.CallbackContext context)
        {
            ShipBrakeHeld = false;
        }

        private void OnShipBoostPerformed(InputAction.CallbackContext context)
        {
            ShipBoostHeld = true;
        }

        private void OnShipBoostCanceled(InputAction.CallbackContext context)
        {
            ShipBoostHeld = false;
        }

        private void OnShipExitPerformed(InputAction.CallbackContext context)
        {
            OnShipExit?.Invoke();
        }

        // === Helper Methods ===
        
        private void EnsureInputActionsLoaded()
        {
            if (_inputActions != null)
                return;
            
            // Try loading directly as InputActionAsset
            _inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            
            if (_inputActions == null)
            {
                // .inputactions files aren't loadable as InputActionAsset via Resources
                // Load the .json version and parse it
                var textAsset = Resources.Load<TextAsset>("InputSystem_Actions");
                if (textAsset != null)
                {
                    _inputActions = InputActionAsset.FromJson(textAsset.text);
                    if (_inputActions != null)
                    {
                        Debug.Log($"InputReader: Loaded InputActionAsset from JSON TextAsset: {_inputActions.name}");
                    }
                }
            }
            
            if (_inputActions == null)
            {
                // Final fallback: search all InputActionAssets
                var assets = Resources.LoadAll<InputActionAsset>("");
                if (assets != null && assets.Length > 0)
                {
                    _inputActions = assets[0];
                    Debug.Log($"InputReader: Found InputActionAsset via LoadAll: {_inputActions.name}");
                }
            }
        }

        // === Cleanup ===
        private void OnDisable()
        {
            DisablePlayerInput();
            DisableShipInput();
        }
    }
}
