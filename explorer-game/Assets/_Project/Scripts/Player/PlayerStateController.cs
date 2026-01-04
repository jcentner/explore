using System;
using UnityEngine;

namespace Explorer.Player
{
    /// <summary>
    /// Central state controller for the player.
    /// Manages transitions between OnFoot and InShip states,
    /// coordinates input switching, camera transitions, and player visibility.
    /// </summary>
    public class PlayerStateController : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("References")]
        [SerializeField, Tooltip("Player's character motor (disabled when in ship)")]
        private CharacterMotorSpherical _characterMotor;
        
        [SerializeField, Tooltip("Player's camera (disabled when in ship)")]
        private Camera _playerCamera;
        
        [SerializeField, Tooltip("Player's visual mesh root (hidden when in ship)")]
        private GameObject _playerVisuals;
        
        [SerializeField, Tooltip("Input reader for switching input modes")]
        private InputReader _inputReader;
        
        [Header("Transitions")]
        [SerializeField, Tooltip("Duration of camera transition fade")]
        private float _transitionDuration = 0.3f;
        
        // === Events ===
        /// <summary>Fired when player state changes. Args: oldState, newState</summary>
        public event Action<PlayerState, PlayerState> OnStateChanged;
        
        /// <summary>Fired when player boards a ship.</summary>
        public event Action OnBoarded;
        
        /// <summary>Fired when player disembarks from a ship.</summary>
        public event Action OnDisembarked;
        
        // === Public Properties ===
        /// <summary>Current player state.</summary>
        public PlayerState CurrentState { get; private set; } = PlayerState.OnFoot;
        
        /// <summary>True if player is currently piloting a ship.</summary>
        public bool IsPiloting => CurrentState == PlayerState.InShip;
        
        /// <summary>True if player is transitioning between states.</summary>
        public bool IsTransitioning => CurrentState == PlayerState.BoardingShip || 
                                        CurrentState == PlayerState.DisembarkingShip;
        
        /// <summary>The ship currently being piloted (null if on foot).</summary>
        public Transform CurrentShip { get; private set; }
        
        // === Private Fields ===
        private Camera _shipCamera;
        private IPilotable _pilotable;
        private Vector3 _disembarkPosition;
        private CanvasGroup _fadeCanvasGroup;
        
        // === Singleton Access ===
        public static PlayerStateController Instance { get; private set; }
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            // Simple singleton for easy access
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple PlayerStateControllers found! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Auto-find references if not assigned
            if (_characterMotor == null)
                _characterMotor = GetComponent<CharacterMotorSpherical>();
            
            if (_playerCamera == null)
                _playerCamera = Camera.main;
            
            if (_inputReader == null)
                _inputReader = Resources.Load<InputReader>("InputReader");
            
            // Create fade overlay for transitions
            CreateFadeOverlay();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        // === Public Methods ===
        
        /// <summary>
        /// Begin boarding a ship. Starts transition sequence.
        /// </summary>
        /// <param name="ship">The ship transform to board.</param>
        /// <param name="shipCamera">The ship's camera to switch to.</param>
        /// <param name="pilotable">The ship's input controller (implements IPilotable).</param>
        public void BoardShip(Transform ship, Camera shipCamera, IPilotable pilotable)
        {
            if (CurrentState != PlayerState.OnFoot)
            {
                Debug.LogWarning($"Cannot board ship from state {CurrentState}");
                return;
            }
            
            CurrentShip = ship;
            _shipCamera = shipCamera;
            _pilotable = pilotable;
            
            StartCoroutine(BoardingSequence());
        }
        
        /// <summary>
        /// Begin disembarking from the current ship.
        /// </summary>
        /// <param name="exitPosition">World position to place player after disembarking.</param>
        public void DisembarkShip(Vector3 exitPosition)
        {
            if (CurrentState != PlayerState.InShip)
            {
                Debug.LogWarning($"Cannot disembark from state {CurrentState}");
                return;
            }
            
            _disembarkPosition = exitPosition;
            StartCoroutine(DisembarkingSequence());
        }
        
        /// <summary>
        /// Force immediate state change (no transition). Use for initialization or testing.
        /// </summary>
        public void ForceState(PlayerState newState)
        {
            var oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(oldState, newState);
        }
        
        // === Private Methods ===
        
        private void SetState(PlayerState newState)
        {
            if (CurrentState == newState) return;
            
            var oldState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"PlayerState: {oldState} → {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }
        
        private System.Collections.IEnumerator BoardingSequence()
        {
            SetState(PlayerState.BoardingShip);
            
            // Fade out
            yield return FadeOut();
            
            // Disable player controls and visuals
            if (_characterMotor != null)
                _characterMotor.enabled = false;
            
            if (_playerVisuals != null)
                _playerVisuals.SetActive(false);
            
            // Disable player input, enable ship input
            _inputReader?.DisablePlayerInput();
            _pilotable?.EnableInput();
            
            // Switch cameras
            if (_playerCamera != null)
                _playerCamera.gameObject.SetActive(false);
            
            if (_shipCamera != null)
                _shipCamera.gameObject.SetActive(true);
            
            // Fade in
            yield return FadeIn();
            
            SetState(PlayerState.InShip);
            OnBoarded?.Invoke();
        }
        
        private System.Collections.IEnumerator DisembarkingSequence()
        {
            SetState(PlayerState.DisembarkingShip);
            
            // Fade out
            yield return FadeOut();
            
            // Disable ship input
            _pilotable?.DisableInput();
            
            // Position player at exit point
            transform.position = _disembarkPosition;
            
            // Align player to ship's up (or gravity - will adjust on next frame)
            if (CurrentShip != null)
            {
                transform.rotation = Quaternion.LookRotation(
                    CurrentShip.forward,
                    CurrentShip.up
                );
            }
            
            // Enable player visuals and controls
            if (_playerVisuals != null)
                _playerVisuals.SetActive(true);
            
            if (_characterMotor != null)
                _characterMotor.enabled = true;
            
            // Switch cameras
            if (_shipCamera != null)
                _shipCamera.gameObject.SetActive(false);
            
            if (_playerCamera != null)
                _playerCamera.gameObject.SetActive(true);
            
            // Enable player input
            _inputReader?.EnablePlayerInput();
            
            // Clear ship reference
            CurrentShip = null;
            _shipCamera = null;
            _pilotable = null;
            
            // Fade in
            yield return FadeIn();
            
            SetState(PlayerState.OnFoot);
            OnDisembarked?.Invoke();
        }
        
        private void CreateFadeOverlay()
        {
            // Create a UI canvas for fade transitions
            var fadeGO = new GameObject("FadeOverlay");
            fadeGO.transform.SetParent(transform);
            
            var canvas = fadeGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // On top of everything
            
            _fadeCanvasGroup = fadeGO.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
            _fadeCanvasGroup.interactable = false;
            
            // Add black image
            var imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(fadeGO.transform);
            
            var image = imageGO.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            
            var rect = imageGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / _transitionDuration);
                yield return null;
            }
            _fadeCanvasGroup.alpha = 1f;
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _transitionDuration);
                yield return null;
            }
            _fadeCanvasGroup.alpha = 0f;
        }
    }
}
