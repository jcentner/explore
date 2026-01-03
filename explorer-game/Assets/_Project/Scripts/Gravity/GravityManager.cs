using System.Collections.Generic;
using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Central registry of all active gravity sources.
    /// Provides queries for dominant gravity at any position.
    /// </summary>
    public class GravityManager : MonoBehaviour
    {
        // === Singleton ===
        private static GravityManager _instance;
        public static GravityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GravityManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GravityManager");
                        _instance = go.AddComponent<GravityManager>();
                    }
                }
                return _instance;
            }
        }

        // === Private Fields ===
        private readonly List<IGravitySource> _sources = new List<IGravitySource>();

        // === Unity Lifecycle ===
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // === Public Methods ===

        /// <summary>
        /// Register a gravity source with the manager.
        /// Called automatically by GravityBody.OnEnable().
        /// </summary>
        public void Register(IGravitySource source)
        {
            if (!_sources.Contains(source))
            {
                _sources.Add(source);
            }
        }

        /// <summary>
        /// Unregister a gravity source from the manager.
        /// Called automatically by GravityBody.OnDisable().
        /// </summary>
        public void Unregister(IGravitySource source)
        {
            _sources.Remove(source);
        }

        /// <summary>
        /// Get the dominant gravity source for a given world position.
        /// Priority rules: highest priority wins, then closest, then instance ID.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>Dominant gravity source, or null if none in range.</returns>
        public IGravitySource GetDominantSource(Vector3 worldPosition)
        {
            IGravitySource dominant = null;
            float dominantDistance = float.MaxValue;
            int dominantPriority = int.MinValue;
            int dominantInstanceId = int.MaxValue;

            foreach (var source in _sources)
            {
                float distance = Vector3.Distance(worldPosition, source.GravityCenter);
                
                // Skip if outside range
                if (distance > source.MaxRange)
                    continue;

                // Determine if this source beats the current dominant
                bool isBetter = false;

                if (source.Priority > dominantPriority)
                {
                    isBetter = true;
                }
                else if (source.Priority == dominantPriority)
                {
                    if (distance < dominantDistance)
                    {
                        isBetter = true;
                    }
                    else if (Mathf.Approximately(distance, dominantDistance))
                    {
                        // Tie-breaker: use instance ID for determinism
                        int instanceId = (source as Object)?.GetInstanceID() ?? 0;
                        if (instanceId < dominantInstanceId)
                        {
                            isBetter = true;
                        }
                    }
                }

                if (isBetter)
                {
                    dominant = source;
                    dominantDistance = distance;
                    dominantPriority = source.Priority;
                    dominantInstanceId = (source as Object)?.GetInstanceID() ?? 0;
                }
            }

            return dominant;
        }

        /// <summary>
        /// Get the gravity vector at a given world position from the dominant source.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>Gravity vector, or Vector3.zero if no sources in range.</returns>
        public Vector3 GetGravityAt(Vector3 worldPosition)
        {
            var source = GetDominantSource(worldPosition);
            return source?.CalculateGravity(worldPosition) ?? Vector3.zero;
        }

        /// <summary>
        /// Get the count of registered gravity sources (for debugging).
        /// </summary>
        public int SourceCount => _sources.Count;
    }
}
