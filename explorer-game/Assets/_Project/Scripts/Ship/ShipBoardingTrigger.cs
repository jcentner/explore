using UnityEngine;
using UnityEngine.InputSystem;
using Explorer.Player;

namespace Explorer.Ship
{
    /// <summary>
    /// Simple boarding trigger for ship prototype testing.
    /// Place on ship - when player enters trigger and presses interact, they board.
    /// Press Exit (F) while piloting to disembark.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class ShipBoardingTrigger : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("References")]
        [SerializeField] private ShipInput _shipInput;
        [SerializeField] private Transform _exitPoint;
        
        [Header("Settings")]
        [SerializeField] private float _boardingRadius = 5f;
        
        // === Private Fields ===
        private SphereCollider _trigger;
        private GameObject _player;
        private Camera _playerCamera;
        private Camera _shipCamera;
        private InputReader _inputReader;
        private bool _isPlayerInRange;
        private bool _isPiloting;
        
        // === Properties ===
        public bool IsPiloting => _isPiloting;
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            // Setup trigger collider
            _trigger = GetComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = _boardingRadius;
            
            // Auto-find ShipInput if not assigned
            if (_shipInput == null)
            {
                _shipInput = GetComponentInParent<ShipInput>();
            }
            
            // Find ship camera
            var shipCamObj = GameObject.Find("ShipCamera");
            if (shipCamObj != null)
            {
                _shipCamera = shipCamObj.GetComponent<Camera>();
            }
        }
        
        private void Start()
        {
            // Ensure ship input starts disabled
            if (_shipInput != null)
            {
                _shipInput.DisableInput();
            }
        }
        
        private void Update()
        {
            // Check for F key using new Input System
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (_isPiloting)
                {
                    Disembark();
                }
                else if (_isPlayerInRange)
                {
                    Board();
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInRange = true;
                _player = other.gameObject;
                
                // Cache player camera
                if (_playerCamera == null)
                {
                    _playerCamera = Camera.main;
                }
                
                // Cache input reader
                if (_inputReader == null)
                {
                    _inputReader = Resources.Load<InputReader>("InputReader");
                }
                
                Debug.Log("Press F to board ship");
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInRange = false;
                Debug.Log("Left ship boarding range");
            }
        }
        
        // === Boarding Methods ===
        private void Board()
        {
            if (_player == null || _shipInput == null)
                return;
            
            _isPiloting = true;
            
            // Disable player
            _player.SetActive(false);
            
            // Disable player input, enable ship input
            if (_inputReader != null)
            {
                _inputReader.DisablePlayerInput();
            }
            _shipInput.EnableInput();
            
            // Switch cameras
            if (_playerCamera != null)
            {
                _playerCamera.gameObject.SetActive(false);
            }
            if (_shipCamera != null)
            {
                _shipCamera.gameObject.SetActive(true);
            }
            
            Debug.Log("Boarded ship - Press F to exit");
        }
        
        private void Disembark()
        {
            if (_player == null)
                return;
            
            _isPiloting = false;
            
            // Disable ship input
            _shipInput.DisableInput();
            
            // Position player at exit point or beside ship
            Vector3 exitPos = _exitPoint != null 
                ? _exitPoint.position 
                : transform.position + transform.right * 3f + transform.up * 2f;
            
            _player.transform.position = exitPos;
            
            // Enable player
            _player.SetActive(true);
            
            // Enable player input
            if (_inputReader != null)
            {
                _inputReader.EnablePlayerInput();
            }
            
            // Switch cameras
            if (_shipCamera != null)
            {
                _shipCamera.gameObject.SetActive(false);
            }
            if (_playerCamera != null)
            {
                _playerCamera.gameObject.SetActive(true);
            }
            
            Debug.Log("Disembarked from ship");
        }
        
        // === Gizmos ===
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _boardingRadius);
            
            if (_exitPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_exitPoint.position, 0.3f);
            }
        }
    }
}
