using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// only Singleton Class.all Manager inherents this。
    /// Usage：public class PowerManager : Singleton&lt;PowerManager&gt; {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] 重复的 {typeof(T).Name}，已销毁");
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}