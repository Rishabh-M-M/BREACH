
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Minimal singleton base. All managers inherit from this.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual bool PersistAcrossScenes => true;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[Singleton] Duplicate {typeof(T).Name} detected; destroying this instance."
                );

                Destroy(gameObject);
                return;
            }

            Instance = (T)this;

            if (PersistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            // Only clear the instance if this object owns it.
            if (Instance == this)
            {
                Instance = null;

                Debug.Log(
                    $"[Singleton] {typeof(T).Name} instance cleared."
                );
            }
        }
    }
}
