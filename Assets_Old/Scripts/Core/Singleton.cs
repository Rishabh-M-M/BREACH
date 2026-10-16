using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Minimal singleton base. All managers inherit from this.
    /// Usage：public class PowerManager : Singleton&lt;PowerManager&gt; {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} detected; destroying this instance.");
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}