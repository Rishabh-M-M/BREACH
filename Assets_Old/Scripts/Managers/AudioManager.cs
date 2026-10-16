using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Central audio playback (TeamDoc 8.7).
    ///
    /// Every lookup is name-based and null-safe. The project currently has zero
    /// audio assets, so this is written to be wired up incrementally: an empty
    /// table must not throw, it must just do nothing.
    ///
    /// GRASP: Indirection -- callers say "play WaveStart", they never hold a
    /// reference to an AudioSource or a clip, so assets can be swapped freely.
    ///
    /// Must exist in the scene: Singleton has no lazy creation.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [System.Serializable]
        private struct NamedClip
        {
            public string key;
            public AudioClip clip;
        }

        [Header("Channels")]
        [Tooltip("Dedicated music player so music can duck under SFX without touching them.")]
        [SerializeField] private AudioSource musicSource;

        [Tooltip("Dedicated SFX player. Created at runtime if left empty.")]
        [SerializeField] private AudioSource sfxSource;

        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        [Header("Clip Table (key -> clip)")]
        [Tooltip("Keys used by the event bindings below. Empty is fine.")]
        [SerializeField] private List<NamedClip> clips = new List<NamedClip>();

        [Header("Event Bindings")]
        [Tooltip("Played when a wave starts.")]
        [SerializeField] private string waveStartKey = "WaveStart";

        [Tooltip("Played when the core takes breach damage.")]
        [SerializeField] private string breachKey = "Breach";

        [Tooltip("Played on the victory panel.")]
        [SerializeField] private string victoryKey = "Victory";

        [Tooltip("Played on the game over panel.")]
        [SerializeField] private string gameOverKey = "GameOver";

        private readonly Dictionary<string, AudioClip> _lookup = new Dictionary<string, AudioClip>();

        // Read accessors so UI can seed sliders from the live value (MainMenu fix).
        public float MasterVolume => masterVolume;

        private void OnEnable()
        {
            EventRegistry.OnWaveStart += HandleWaveStart;
            EventRegistry.OnCoreHPChanged += HandleCoreHPChanged;
            EventRegistry.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventRegistry.OnWaveStart -= HandleWaveStart;
            EventRegistry.OnCoreHPChanged -= HandleCoreHPChanged;
            EventRegistry.OnGameStateChanged -= HandleGameStateChanged;
        }

        protected override void Awake()
        {
            // Registers the singleton. Must run before anything resolves Instance.
            base.Awake();

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource != null)
            {
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            ApplyVolumes();
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup.Clear();

            foreach (NamedClip entry in clips)
            {
                if (string.IsNullOrEmpty(entry.key) || entry.clip == null)
                {
                    continue;
                }

                _lookup[entry.key] = entry.clip;
            }
        }

        // ========== Public API ==========

        public void PlaySfx(string key)
        {
            if (sfxSource == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (!_lookup.TryGetValue(key, out AudioClip clip) || clip == null)
            {
                // Silent by design while the asset pipeline is still empty.
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(string key)
        {
            if (musicSource == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (!_lookup.TryGetValue(key, out AudioClip clip) || clip == null)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void SetMasterVolume(float v)
        {
            masterVolume = Mathf.Clamp01(v);
            ApplyVolumes();
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            ApplyVolumes();
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (sfxSource != null)
            {
                sfxSource.volume = masterVolume * sfxVolume;
            }

            if (musicSource != null)
            {
                musicSource.volume = masterVolume * musicVolume;
            }
        }

        // ========== Event bindings ==========

        private void HandleWaveStart(int waveIndex)
        {
            PlaySfx(waveStartKey);
        }

        private void HandleCoreHPChanged(int current, int max)
        {
            PlaySfx(breachKey);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Victory)
            {
                PlaySfx(victoryKey);
            }
            else if (state == GameState.GameOver)
            {
                PlaySfx(gameOverKey);
            }
        }
    }
}
