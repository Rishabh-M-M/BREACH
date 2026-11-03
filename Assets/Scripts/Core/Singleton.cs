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

        /// <summary>
        /// Whether this singleton should survive scene loads (DontDestroyOnLoad).
        /// Managers that hold direct references to scene objects (e.g. WaveManager
        /// references its scene's spawners) must override this to false so they are
        /// recreated fresh on every scene load — otherwise a restart leaves a stale
        /// instance pointing at destroyed objects and it never re-runs its Start loop.
        /// </summary>
        protected virtual bool PersistAcrossScenes => true;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} detected; destroying this instance.");
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
            if (PersistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }
    }
}