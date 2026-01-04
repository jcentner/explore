using UnityEngine;

namespace Explorer.Core.MeshGeneration
{
    /// <summary>
    /// Generates a smooth icosphere mesh for this GameObject.
    /// Works in both edit mode and runtime.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SmoothSphereMesh : MonoBehaviour
    {
        // === Inspector Fields ===
        [Header("Sphere Settings")]
        [SerializeField]
        [Tooltip("Radius of the sphere mesh (before GameObject scale is applied).")]
        [Range(0.1f, 10f)]
        private float _radius = 0.5f;

        [SerializeField]
        [Tooltip("Subdivision level. Higher = smoother. 3-4 recommended for planets.")]
        [Range(0, 5)]
        private int _subdivisions = 3;

        [Header("Options")]
        [SerializeField]
        [Tooltip("Automatically regenerate mesh when parameters change in editor.")]
        private bool _autoRegenerate = true;

        // === Private Fields ===
        private MeshFilter _meshFilter;
        private float _lastRadius;
        private int _lastSubdivisions;

        // === Public Properties ===
        public float Radius => _radius;
        public int Subdivisions => _subdivisions;

        // === Unity Lifecycle ===
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            GenerateMesh();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_autoRegenerate && (_radius != _lastRadius || _subdivisions != _lastSubdivisions))
            {
                // Delay to avoid editor issues
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        GenerateMesh();
                    }
                };
            }
        }
        #endif

        // === Public Methods ===
        /// <summary>
        /// Regenerates the sphere mesh with current settings.
        /// </summary>
        [ContextMenu("Regenerate Mesh")]
        public void GenerateMesh()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            if (_meshFilter == null) return;

            // Clean up old mesh to prevent memory leaks
            if (_meshFilter.sharedMesh != null && _meshFilter.sharedMesh.name.StartsWith("IcoSphere_"))
            {
                if (Application.isPlaying)
                {
                    Destroy(_meshFilter.sharedMesh);
                }
                else
                {
                    DestroyImmediate(_meshFilter.sharedMesh);
                }
            }

            // Generate new mesh
            _meshFilter.sharedMesh = IcoSphereGenerator.Create(_radius, _subdivisions);

            // Update collider if present
            var sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.radius = _radius;
            }

            _lastRadius = _radius;
            _lastSubdivisions = _subdivisions;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }

        /// <summary>
        /// Sets sphere parameters and regenerates the mesh.
        /// </summary>
        public void SetParameters(float radius, int subdivisions)
        {
            _radius = Mathf.Max(0.1f, radius);
            _subdivisions = Mathf.Clamp(subdivisions, 0, 5);
            GenerateMesh();
        }
    }
}
