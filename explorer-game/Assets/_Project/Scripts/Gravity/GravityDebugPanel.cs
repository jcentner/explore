using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Runtime debug panel showing gravity information and tuning controls.
    /// Displays active gravity sources, contributions, and allows parameter adjustment.
    /// </summary>
    public class GravityDebugPanel : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Display")]
        [SerializeField]
        [Tooltip("Text component for debug output.")]
        private TextMeshProUGUI _debugText;

        [SerializeField]
        [Tooltip("Key to toggle debug panel visibility.")]
        private KeyCode _toggleKey = KeyCode.F3;

        [SerializeField]
        [Tooltip("Show detailed contribution breakdown.")]
        private bool _showContributions = true;

        [Header("Runtime Tuning")]
        [SerializeField]
        [Tooltip("Reference to the player's GravitySolver for tuning.")]
        private GravitySolver _targetSolver;

        // === Private Fields ===
        private GravityManager _gravityManager;
        private Canvas _canvas;
        private bool _isVisible = true;
        private Transform _playerTransform;
        private Transform _lastPlayerParent;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();

            // Auto-create debug text if not assigned
            if (_debugText == null)
            {
                var textGO = new GameObject("DebugText");
                textGO.transform.SetParent(transform, false);

                var rectTransform = textGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = new Vector2(10, 10);
                rectTransform.offsetMax = new Vector2(-10, -10);

                _debugText = textGO.AddComponent<TextMeshProUGUI>();
                _debugText.fontSize = 14;
                _debugText.color = Color.white;
                _debugText.alignment = TextAlignmentOptions.TopLeft;
            }
        }

        private void Start()
        {
            _gravityManager = GravityManager.Instance;
            FindTargetSolver();
        }

        private void Update()
        {
            // Check if player's parent changed (boarded/exited ship)
            if (_playerTransform != null && _playerTransform.parent != _lastPlayerParent)
            {
                _lastPlayerParent = _playerTransform.parent;
                _targetSolver = null; // Force re-find
            }
            
            // Toggle visibility using new Input System
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            {
                _isVisible = !_isVisible;
                if (_debugText != null)
                    _debugText.gameObject.SetActive(_isVisible);
            }

            if (_isVisible && _debugText != null)
            {
                UpdateDebugText();
            }
        }

        // === Private Methods ===

        private void FindTargetSolver()
        {
            // Try to find player
            var player = GameObject.FindGameObjectWithTag(Tags.PLAYER);
            if (player != null)
            {
                _playerTransform = player.transform;
                _lastPlayerParent = _playerTransform.parent;
                
                // Check parent first (might be in ship) - ship's solver takes priority
                _targetSolver = player.GetComponentInParent<GravitySolver>();
                if (_targetSolver != null && _targetSolver.gameObject != player)
                    return; // Found ship's solver
                
                // Check player itself
                _targetSolver = player.GetComponent<GravitySolver>();
                if (_targetSolver != null)
                    return;
            }

            // Fallback to any solver
            _targetSolver = FindFirstObjectByType<GravitySolver>();
        }

        private void UpdateDebugText()
        {
            if (_gravityManager == null || _targetSolver == null)
            {
                FindTargetSolver();
                if (_targetSolver == null)
                {
                    _debugText.text = "<color=yellow>No GravitySolver found</color>";
                    return;
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b><color=cyan>GRAVITY DEBUG</color></b>");
            sb.AppendLine($"Press {_toggleKey} to toggle");
            sb.AppendLine();

            // Current state
            Vector3 gravity = _targetSolver.CurrentGravity;
            float magnitude = gravity.magnitude;
            bool isZeroG = _targetSolver.IsInZeroG;

            sb.AppendLine($"<b>Gravity:</b> {magnitude:F2} m/s²");
            sb.AppendLine($"<b>Direction:</b> {gravity.normalized:F2}");
            sb.AppendLine($"<b>Zero-G:</b> {(isZeroG ? "<color=magenta>YES</color>" : "No")}");
            sb.AppendLine();

            // Dominant source
            var dominant = _targetSolver.DominantSource;
            if (dominant != null)
            {
                var body = dominant as GravityBody;
                string sourceName = body != null ? body.gameObject.name : "Unknown";
                sb.AppendLine($"<b>Dominant:</b> {sourceName}");
            }
            else
            {
                sb.AppendLine("<b>Dominant:</b> <color=yellow>None</color>");
            }

            // Contributions breakdown
            if (_showContributions)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Contributors:</b>");

                var contributions = _targetSolver.GravityContributions;
                if (contributions != null && contributions.Count > 0)
                {
                    foreach (var contrib in contributions)
                    {
                        var body = contrib.Source as GravityBody;
                        string name = body != null ? body.gameObject.name : "Unknown";
                        string colorTag = contrib.InfluencePercent > 0.5f ? "green" :
                                         contrib.InfluencePercent > 0.2f ? "yellow" : "gray";

                        sb.AppendLine($"  <color={colorTag}>{name}</color>: {contrib.Magnitude:F2} m/s² ({contrib.InfluencePercent * 100:F0}%)");
                    }
                }
                else
                {
                    sb.AppendLine("  <color=gray>None</color>");
                }
            }

            // Solver settings
            sb.AppendLine();
            sb.AppendLine("<b>Solver Settings:</b>");
            sb.AppendLine($"  Orientation Speed: {_targetSolver.OrientationBlendSpeed:F0}°/s");
            sb.AppendLine($"  Zero-G Threshold: {_targetSolver.ZeroGThreshold:F2} m/s²");

            _debugText.text = sb.ToString();
        }

        // === Public Methods ===

        /// <summary>
        /// Set the target GravitySolver to monitor.
        /// </summary>
        public void SetTarget(GravitySolver solver)
        {
            _targetSolver = solver;
        }
    }
}
