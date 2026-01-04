using UnityEngine;
using Explorer.Core;
using Explorer.Player;

namespace Explorer.Ship
{
    /// <summary>
    /// Boarding zone trigger for ships.
    /// Detects when player is in range, shows UI prompt, and handles board/disembark.
    /// Works with PlayerStateController for proper state management.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class ShipBoardingTrigger : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("References")]
        [SerializeField, Tooltip("Ship input controller")]
        private ShipInput _shipInput;
        
        [SerializeField, Tooltip("Ship camera to activate when piloting")]
        private Camera _shipCamera;
        
        [SerializeField, Tooltip("Exit point transform (player spawns here on disembark)")]
        private Transform _exitPoint;
        
        [SerializeField, Tooltip("Input reader for interaction events")]
        private InputReader _inputReader;
        
        [Header("Settings")]
        [SerializeField, Tooltip("Radius of boarding trigger zone")]
        private float _boardingRadius = 5f;
        
        [SerializeField, Tooltip("Ground check distance for safe exit")]
        private float _groundCheckDistance = 10f;
        
        [SerializeField, Tooltip("Layers considered as ground for exit")]
        private LayerMask _groundLayers = ~0;
        
        [Header("UI")]
        [SerializeField, Tooltip("UI prompt to show when in range")]
        private GameObject _boardingPromptUI;
        
        // === Events ===
        /// <summary>Fired when player enters boarding range.</summary>
        public event System.Action OnPlayerInRange;
        
        /// <summary>Fired when player leaves boarding range.</summary>
        public event System.Action OnPlayerOutOfRange;
        
        // === Public Properties ===
        public bool IsPlayerInRange => _isPlayerInRange;
        public bool IsPiloting => _isPiloting;
        
        // === Private Fields ===
        private SphereCollider _trigger;
        private PlayerStateController _playerStateController;
        private Transform _playerTransform;
        private bool _isPlayerInRange;
        private bool _isPiloting;
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            // Setup trigger collider
            _trigger = GetComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = _boardingRadius;
            
            // Auto-find ShipInput if not assigned
            if (_shipInput == null)
                _shipInput = GetComponentInParent<ShipInput>();
            
            // Auto-find InputReader if not assigned
            if (_inputReader == null)
                _inputReader = Resources.Load<InputReader>("InputReader");
            
            // Validate references
            if (_shipCamera == null)
                Debug.LogWarning($"ShipBoardingTrigger on {name}: ShipCamera not assigned!");
        }
        
        private void Start()
        {
            // Ensure ship input starts disabled
            _shipInput?.DisableInput();
            
            // Hide boarding prompt initially
            if (_boardingPromptUI != null)
                _boardingPromptUI.SetActive(false);
            
            // Subscribe to input events
            if (_inputReader != null)
            {
                _inputReader.OnInteract += HandleInteract;
                _inputReader.OnShipExit += HandleShipExit;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from input events
            if (_inputReader != null)
            {
                _inputReader.OnInteract -= HandleInteract;
                _inputReader.OnShipExit -= HandleShipExit;
            }
        }
        
        private void HandleInteract()
        {
            // Player pressed interact while on foot - try to board
            if (_isPlayerInRange && !_isPiloting)
            {
                TryBoard();
            }
        }
        
        private void HandleShipExit()
        {
            // Player pressed exit while in ship - try to disembark
            if (_isPiloting)
            {
                TryDisembark();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.PLAYER)) return;
            if (_isPiloting) return;
            
            _isPlayerInRange = true;
            _playerTransform = other.transform;
            
            // Find PlayerStateController
            _playerStateController = other.GetComponent<PlayerStateController>();
            if (_playerStateController == null)
                _playerStateController = other.GetComponentInParent<PlayerStateController>();
            
            // Show UI prompt (use serialized reference or service locator)
            if (_boardingPromptUI != null)
                _boardingPromptUI.SetActive(true);
            else
                InteractionPromptService.Show("Press [F] to board ship");
            
            OnPlayerInRange?.Invoke();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Tags.PLAYER)) return;
            
            _isPlayerInRange = false;
            
            // Hide UI prompt
            if (_boardingPromptUI != null)
                _boardingPromptUI.SetActive(false);
            else
                InteractionPromptService.Hide();
            
            OnPlayerOutOfRange?.Invoke();
        }
        
        // === Boarding Methods ===
        
        private void TryBoard()
        {
            if (_playerStateController == null)
            {
                Debug.LogError("ShipBoardingTrigger: PlayerStateController not found on player!");
                FallbackBoard();
                return;
            }
            
            if (_playerStateController.IsTransitioning)
            {
                Debug.Log("Cannot board: player is transitioning");
                return;
            }
            
            _isPiloting = true;
            
            // Hide prompt
            if (_boardingPromptUI != null)
                _boardingPromptUI.SetActive(false);
            else
                InteractionPromptService.Hide();
            
            // Use state controller for proper transition
            _playerStateController.BoardShip(transform, _shipCamera, _shipInput);
            
            // Subscribe to disembark for cleanup
            _playerStateController.OnDisembarked += HandleDisembarked;
        }
        
        private void TryDisembark()
        {
            if (_playerStateController == null)
            {
                FallbackDisembark();
                return;
            }
            
            if (_playerStateController.IsTransitioning)
            {
                Debug.Log("Cannot disembark: player is transitioning");
                return;
            }
            
            Vector3 exitPos = FindSafeExitPosition();
            _playerStateController.DisembarkShip(exitPos);
        }
        
        private void HandleDisembarked()
        {
            _isPiloting = false;
            
            if (_playerStateController != null)
                _playerStateController.OnDisembarked -= HandleDisembarked;
        }
        
        /// <summary>
        /// Find a safe position for the player to exit.
        /// Raycasts down from exit point to find ground.
        /// </summary>
        private Vector3 FindSafeExitPosition()
        {
            // Start with explicit exit point or default to ship's right side
            Vector3 baseExitPos = _exitPoint != null
                ? _exitPoint.position
                : transform.position + transform.right * 4f + transform.up * 2f;
            
            // Raycast down to find ground
            if (Physics.Raycast(baseExitPos, -transform.up, out RaycastHit hit, _groundCheckDistance, _groundLayers))
            {
                // Place player slightly above ground
                return hit.point + hit.normal * 1f;
            }
            
            // Also try world down if ship is tilted
            if (Physics.Raycast(baseExitPos, Vector3.down, out hit, _groundCheckDistance, _groundLayers))
            {
                return hit.point + hit.normal * 1f;
            }
            
            // No ground found - use base position (player will fall)
            Debug.LogWarning("ShipBoardingTrigger: No ground found for exit, using default position");
            return baseExitPos;
        }
        
        // === Fallback Methods (if no PlayerStateController) ===
        
        private void FallbackBoard()
        {
            Debug.LogWarning("Using fallback boarding (no PlayerStateController)");
            
            _isPiloting = true;
            
            // Disable player
            if (_playerTransform != null)
                _playerTransform.gameObject.SetActive(false);
            
            // Enable ship
            _shipInput?.EnableInput();
            
            // Switch cameras
            var mainCam = Camera.main;
            if (mainCam != null)
                mainCam.gameObject.SetActive(false);
            
            if (_shipCamera != null)
                _shipCamera.gameObject.SetActive(true);
        }
        
        private void FallbackDisembark()
        {
            Debug.LogWarning("Using fallback disembarking (no PlayerStateController)");
            
            _isPiloting = false;
            
            // Disable ship
            _shipInput?.DisableInput();
            
            // Position and enable player
            Vector3 exitPos = FindSafeExitPosition();
            if (_playerTransform != null)
            {
                _playerTransform.position = exitPos;
                _playerTransform.gameObject.SetActive(true);
            }
            
            // Switch cameras
            if (_shipCamera != null)
                _shipCamera.gameObject.SetActive(false);
            
            var mainCam = Camera.main;
            if (mainCam != null)
                mainCam.gameObject.SetActive(true);
        }
        
        // === Gizmos ===
        private void OnDrawGizmosSelected()
        {
            // Boarding radius
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, _boardingRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _boardingRadius);
            
            // Exit point
            Vector3 exitPos = _exitPoint != null
                ? _exitPoint.position
                : transform.position + transform.right * 4f + transform.up * 2f;
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(exitPos, 0.3f);
            Gizmos.DrawLine(transform.position, exitPos);
            
            // Ground check ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(exitPos, -transform.up * _groundCheckDistance);
        }
    }
}
