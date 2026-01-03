using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Explorer.Player
{
    /// <summary>
    /// ScriptableObject that decouples input from consumers.
    /// Uses Unity's Input System via InputActionAsset reference.
    /// Consumers subscribe to events or poll input values.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Explorer/Input Reader")]
    public class InputReader : ScriptableObject
    {
        // === Input Action Asset ===
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private string _playerMapName = "Player";

        // === Cached Actions ===
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _interactAction;

        // === Public Properties ===
        
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

        // === Events ===
        
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

        // === Private Fields ===
        private bool _isEnabled;

        // === Public Methods ===

        /// <summary>
        /// Enable player input actions.
        /// Call this when player should receive input.
        /// </summary>
        public void EnablePlayerInput()
        {
            if (_isEnabled)
                return;

            // Try to find InputActionAsset if not assigned
            if (_inputActions == null)
            {
                _inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (_inputActions == null)
            {
                Debug.LogError("InputReader: No InputActionAsset assigned and couldn't find 'InputSystem_Actions' in Resources!");
                return;
            }

            _isEnabled = true;
            
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
            if (!_isEnabled)
                return;

            _isEnabled = false;

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

        // === Cleanup ===
        private void OnDisable()
        {
            DisablePlayerInput();
        }
    }
}
