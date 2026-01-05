using System.Collections.Generic;
using UnityEngine;
using Explorer.Core;

namespace Explorer.Gravity
{
    /// <summary>
    /// Data about a single gravity source's contribution at a position.
    /// Used for debugging, UI, and understanding gravity composition.
    /// </summary>
    public struct GravityContribution
    {
        /// <summary>The gravity source.</summary>
        public IGravitySource Source;
        /// <summary>Direction toward the source (normalized).</summary>
        public Vector3 Direction;
        /// <summary>Gravity magnitude from this source in m/s².</summary>
        public float Magnitude;
        /// <summary>Distance from query position to source center.</summary>
        public float DistanceToSource;
        /// <summary>This source's contribution as fraction of total (0-1).</summary>
        public float InfluencePercent;
    }

    /// <summary>
    /// Central registry of all active gravity sources.
    /// Provides queries for accumulated gravity (multi-body) and dominant source (orientation).
    /// </summary>
    public class GravityManager : MonoBehaviour
    {
        // === Singleton ===
        private static GravityManager _instance;
        private static bool _applicationIsQuitting;

        public static GravityManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GravityManager>();
                    if (_instance == null && !_applicationIsQuitting)
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

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
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
        /// Get accumulated gravity from all sources at a given world position.
        /// Each source contributes based on its inverse-square falloff.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>Combined gravity vector from all sources in range.</returns>
        public Vector3 GetAccumulatedGravity(Vector3 worldPosition)
        {
            Vector3 totalGravity = Vector3.zero;

            foreach (var source in _sources)
            {
                // Skip sources that don't participate in accumulation
                if (source.Mode == GravityMode.Dominant)
                    continue;

                Vector3 contribution = source.CalculateGravity(worldPosition);
                totalGravity += contribution;
            }

            return totalGravity;
        }

        /// <summary>
        /// Get all gravity contributions at a position, sorted by magnitude (descending).
        /// Useful for debugging and UI gravity indicator.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>List of contributions from all sources in range.</returns>
        public List<GravityContribution> GetAllContributors(Vector3 worldPosition)
        {
            var contributions = new List<GravityContribution>();
            float totalMagnitude = 0f;

            foreach (var source in _sources)
            {
                Vector3 gravity = source.CalculateGravity(worldPosition);
                float magnitude = gravity.magnitude;

                if (magnitude < 0.001f)
                    continue;

                float distance = Vector3.Distance(worldPosition, source.GravityCenter);

                contributions.Add(new GravityContribution
                {
                    Source = source,
                    Direction = gravity.normalized,
                    Magnitude = magnitude,
                    DistanceToSource = distance,
                    InfluencePercent = 0f // Calculate after we have total
                });

                totalMagnitude += magnitude;
            }

            // Calculate influence percentages
            if (totalMagnitude > 0.001f)
            {
                for (int i = 0; i < contributions.Count; i++)
                {
                    var c = contributions[i];
                    c.InfluencePercent = c.Magnitude / totalMagnitude;
                    contributions[i] = c;
                }
            }

            // Sort by magnitude descending
            contributions.Sort((a, b) => b.Magnitude.CompareTo(a.Magnitude));

            return contributions;
        }

        /// <summary>
        /// Get the dominant gravity source for a given world position.
        /// Now based on strongest gravity magnitude at position (not distance/priority).
        /// Priority is used only as tie-breaker.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>Dominant gravity source, or null if none in range.</returns>
        public IGravitySource GetDominantSource(Vector3 worldPosition)
        {
            IGravitySource dominant = null;
            float dominantMagnitude = 0f;
            int dominantPriority = int.MinValue;

            foreach (var source in _sources)
            {
                // Skip sources that can't be dominant
                if (source.Mode == GravityMode.Accumulate)
                    continue;

                Vector3 gravity = source.CalculateGravity(worldPosition);
                float magnitude = gravity.magnitude;

                // Skip if no contribution
                if (magnitude < 0.001f)
                    continue;

                // Determine if this source beats the current dominant
                bool isBetter = false;

                if (magnitude > dominantMagnitude + 0.001f) // Clear winner by magnitude
                {
                    isBetter = true;
                }
                else if (Mathf.Abs(magnitude - dominantMagnitude) <= 0.001f) // Tied magnitude
                {
                    // Use priority as tie-breaker
                    if (source.Priority > dominantPriority)
                    {
                        isBetter = true;
                    }
                }

                if (isBetter)
                {
                    dominant = source;
                    dominantMagnitude = magnitude;
                    dominantPriority = source.Priority;
                }
            }

            return dominant;
        }

        /// <summary>
        /// Get the gravity vector at a given world position from the dominant source only.
        /// For accumulated gravity, use GetAccumulatedGravity instead.
        /// </summary>
        /// <param name="worldPosition">Position to query.</param>
        /// <returns>Gravity vector from dominant source, or Vector3.zero if none.</returns>
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
