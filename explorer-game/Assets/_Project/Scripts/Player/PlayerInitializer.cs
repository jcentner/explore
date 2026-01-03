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

        private CharacterMotorSpherical _motor;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotorSpherical>();

            // If InputReader not assigned, try to load from Resources
            if (_inputReader == null)
            {
                _inputReader = Resources.Load<InputReader>("InputReader");
            }

            // Wire up motor
            if (_motor != null && _inputReader != null)
            {
                _motor.SetInputReader(_inputReader);
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
