using UnityEngine;

namespace Explorer.Player
{
    /// <summary>
    /// Initializes player components and wires up dependencies at runtime.
    /// This is a bootstrap component that should be placed on the Player object.
    /// </summary>
    public class PlayerInitializer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private Transform _cameraTarget;

        [Header("Model Visibility")]
        [SerializeField]
        [Tooltip("Renderers to hide in first-person mode. If empty, will auto-find in children.")]
        private Renderer[] _playerRenderers;

        [SerializeField]
        [Tooltip("Auto-find renderers in children if array is empty.")]
        private bool _autoFindRenderers = true;

        private CharacterMotorSpherical _motor;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotorSpherical>();

            // If InputReader not assigned, try to load from Resources
            if (_inputReader == null)
            {
                _inputReader = Resources.Load<InputReader>("InputReader");
                Debug.Log($"PlayerInitializer: Loaded InputReader from Resources: {_inputReader != null}");
            }

            // Wire up motor
            if (_motor != null && _inputReader != null)
            {
                _motor.SetInputReader(_inputReader);
                Debug.Log("PlayerInitializer: Wired up motor");
            }

            // Auto-find renderers if needed
            if (_autoFindRenderers && (_playerRenderers == null || _playerRenderers.Length == 0))
            {
                _playerRenderers = GetComponentsInChildren<Renderer>();
            }

            // Wire up camera
            if (_playerCamera != null)
            {
                if (_inputReader != null)
                {
                    _playerCamera.SetInputReader(_inputReader);
                }

                Transform target = _cameraTarget != null ? _cameraTarget : transform;
                _playerCamera.SetTarget(target);

                // Set up renderers for first-person visibility toggle
                if (_playerRenderers != null && _playerRenderers.Length > 0)
                {
                    _playerCamera.SetPlayerRenderers(_playerRenderers);
                }
            }

            // Enable input
            if (_inputReader != null)
            {
                _inputReader.EnablePlayerInput();
            }
        }

        private void OnDestroy()
        {
            if (_inputReader != null)
            {
                _inputReader.DisablePlayerInput();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find camera if not assigned
            if (_playerCamera == null)
            {
                _playerCamera = FindFirstObjectByType<PlayerCamera>();
            }
        }
#endif
    }
}
