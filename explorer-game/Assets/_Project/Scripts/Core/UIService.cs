using UnityEngine;

namespace Explorer.Core
{
    /// <summary>
    /// Generic service locator for UI interfaces.
    /// Allows gameplay code to access UI without direct assembly references.
    /// 
    /// Usage:
    /// - UI assembly registers: UIService&lt;IInteractionPrompt&gt;.Register(this);
    /// - Gameplay code consumes: UIService&lt;IInteractionPrompt&gt;.Instance?.Show("Press F");
    /// </summary>
    /// <typeparam name="T">The interface type to register/retrieve.</typeparam>
    public static class UIService<T> where T : class
    {
        private static T _instance;
        
        /// <summary>
        /// The currently registered instance, or null if none registered.
        /// Safe to use with null-conditional: UIService&lt;T&gt;.Instance?.Method()
        /// </summary>
        public static T Instance => _instance;
        
        /// <summary>
        /// Whether an instance is currently registered.
        /// </summary>
        public static bool IsRegistered => _instance != null;
        
        /// <summary>
        /// Register a UI service implementation.
        /// Call this from the UI component's initialization (Awake/OnEnable).
        /// </summary>
        /// <param name="instance">The instance implementing interface T.</param>
        public static void Register(T instance)
        {
            if (_instance != null && instance != null && _instance != instance)
            {
                Debug.LogWarning($"[UIService<{typeof(T).Name}>] Already registered. Overwriting with new instance.");
            }
            _instance = instance;
        }
        
        /// <summary>
        /// Unregister a UI service implementation.
        /// Call this from the UI component's cleanup (OnDisable/OnDestroy).
        /// Only unregisters if the instance matches the current one.
        /// </summary>
        /// <param name="instance">The instance to unregister.</param>
        public static void Unregister(T instance)
        {
            if (_instance == instance)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Clear the registered instance regardless of what it is.
        /// Use with caution - primarily for testing or scene cleanup.
        /// </summary>
        public static void Clear()
        {
            _instance = null;
        }
    }
}
