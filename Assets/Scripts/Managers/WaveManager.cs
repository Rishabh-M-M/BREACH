using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// WaveManager — the single brain of the wave spawning system (design doc
    /// Docs/WaveSystemDesign.md). Owns wave configuration, dispatch pacing,
    /// settlement accounting, rest-period countdown and the early-start entry.
    ///
    /// Architecture (D5): WaveManager issues orders; execution still happens in
    /// the existing EnemySpawner_Single instances via their public SpawnWave(int).
    /// Each spawner decides what comes through its own door; WaveManager controls
    /// rhythm and quantity. EnemyMover / EnemyAttack / EnemyTargetPoint are
    /// completely unaware of this system.
    ///
    /// GRASP: Controller (owns the wave-coordination responsibility) +
    ///        Indirection (settlement arrives via EventRegistry.OnEnemyResolved).
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        // ========== Inspector configuration ==========

        [Serializable]
        public class WaveGroup
        {
            [Tooltip("Existing spawner instance on this route/door. It uses its own Inspector settings (prefab, target points, interval).")]
            public EnemySpawner_Single spawner;

            [Tooltip("How many enemies this door spawns for this wave.")]
            public int count = 1;

            [Tooltip("Stagger delay from the start of the wave (left door first, right door later).")]
            public float startDelay = 0f;
        }

        [Serializable]
        public class Wave
        {
            public List<WaveGroup> groups = new List<WaveGroup>();

            [Tooltip("How long this wave stays active (in seconds) before the next wave starts. " +
                     "The next wave starts when this timer expires, regardless of how many enemies " +
                     "are still alive — pacing is driven by game time, not by clearing the wave.")]
            public float duration = 30f;

            [Tooltip("If true, the wave can end early (before its duration) when every enemy it " +
                     "spawned has been killed or breached. Leave false for strictly time-based pacing. " +
                     "Note: with overlapping waves, leftover enemies from the previous wave count into " +
                     "the current one, so early-clear may fire ahead of time.")]
            public bool advanceEarlyOnClear = false;
        }

        [Header("Global Pacing")]
        [Tooltip("Delay between game start and Wave 1 (time for the player to set up turrets).")]
        [SerializeField] private float firstWaveDelay = 5f;

        [Tooltip("Rest-period countdown between waves.")]
        [SerializeField] private float restDuration = 20f;

        [Tooltip("Whether the player may cut the rest period short.")]
        [SerializeField] private bool earlyStartEnabled = true;

        [Header("Wave Configuration")]
        [Tooltip("Ordered list of waves. The last wave cleared triggers Victory.")]
        [SerializeField] private List<Wave> waves = new List<Wave>();

        // ========== Runtime state ==========

        private int _orderedCount;      // enemies dispatched (ordered) in the current wave
        private int _resolvedCount;     // enemies resolved (killed or breached) in the current wave
        private bool _dispatchComplete;  // all groups of the current wave have been issued
        private int _pendingGroups;      // groups of the current wave not yet issued
        private bool _restActive;        // currently inside a rest countdown
        private bool _earlyStartRequested;
        private Coroutine _mainRoutine;

        // ========== Public read-only surface ==========

        /// <summary>
        /// Whether the player may cut the rest period short. Exposed so HUDs can
        /// grey out the early-start button when the feature is disabled.
        /// </summary>
        public bool EarlyStartEnabled => earlyStartEnabled;

        // ========== Lifecycle ==========

        protected override void Awake()
        {
            base.Awake();
            EventRegistry.OnEnemyResolved += HandleEnemyResolved;
        }

        private void Start()
        {
            _mainRoutine = StartCoroutine(WaveLoop());
        }

        private void OnDestroy()
        {
            EventRegistry.OnEnemyResolved -= HandleEnemyResolved;
        }

        // ========== Public entry (UI button / ContextMenu) ==========

        /// <summary>
        /// Cuts the rest-period countdown short and starts the next wave
        /// immediately (D2). Only valid while a rest period is active and
        /// early start is enabled.
        /// </summary>
        public void RequestEarlyStart()
        {
            if (!earlyStartEnabled)
            {
                Debug.Log("[WaveManager] Early start disabled.");
                return;
            }
            if (!_restActive)
            {
                Debug.Log("[WaveManager] RequestEarlyStart ignored — not in a rest period.");
                return;
            }

            _earlyStartRequested = true;
            Debug.Log("[WaveManager] Early start requested — cutting rest short.");
        }

        [ContextMenu("Test - Request Early Start")]
        private void TestRequestEarlyStart()
        {
            RequestEarlyStart();
        }

        // ========== Core loop ==========


        private IEnumerator WaveLoop()
        {
            if (waves == null || waves.Count == 0)
            {
                Debug.Log("[WaveManager] No waves configured — loop idle.");
                yield break;
            }

            // Opening delay so the player can set up before the first wave.
            yield return new WaitForSeconds(firstWaveDelay);

            for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                Wave wave = waves[waveIndex];
                RunWave(waveIndex, wave);

                // Time-based pacing: wait for this wave's active duration. The next
                // wave starts when the timer expires — NOT after all enemies are
                // killed. Optionally, the wave can end early if fully cleared.
                yield return WaveActiveDuration(wave);

                bool isLastWave = (waveIndex == waves.Count - 1);

                if (isLastWave)
                {
                    Debug.Log($"[WaveManager] Wave {waveIndex + 1}/{waves.Count} cleared — all waves done!");
                    EventRegistry.BroadcastAllWavesCleared();
                    GameManager.Instance.EndGame(true);
                    // GameOverScreen (zero-change path) receives Victory: timeScale=0,
                    // hit-stop, then the VICTORY panel.
                    yield break;
                }

                Debug.Log($"[WaveManager] Wave {waveIndex + 1}/{waves.Count} cleared → rest period.");
                GameManager.Instance.SetWaveClearState();
                yield return RestCountdown();
            }
        }

        private void RunWave(int waveIndex, Wave wave)
        {
            _orderedCount = 0;
            _resolvedCount = 0;
            _dispatchComplete = false;
            _pendingGroups = 0;
            _earlyStartRequested = false;
            _restActive = false;

            EventRegistry.BroadcastWaveStart(waveIndex);

            // Dispatch each group; groups stagger via their own startDelay so one
            // door can lead the other. Coroutines run on scaled time, so a game
            // over / victory (timeScale = 0) freezes everything in place naturally.
            if (wave.groups != null)
            {
                foreach (WaveGroup group in wave.groups)
                {
                    if (group == null || group.spawner == null || group.count <= 0)
                    {
                        continue;
                    }
                    _orderedCount += group.count;
                    _pendingGroups++;
                    StartCoroutine(DispatchGroup(group));
                }
            }

            if (_orderedCount == 0)
            {
                // Empty wave — count it as instantly dispatched so the loop
                // settles on the next check.
                _dispatchComplete = true;
            }
        }

        private IEnumerator DispatchGroup(WaveGroup group)
        {
            if (group.startDelay > 0f)
            {
                yield return new WaitForSeconds(group.startDelay);
            }

            group.spawner.SpawnWave(group.count);

            _pendingGroups--;
            if (_pendingGroups <= 0)
            {
                _dispatchComplete = true;
            }
        }

        /// <summary>
        /// Keeps the current wave active for <see cref="Wave.duration"/> seconds
        /// (time-based pacing). The timer is the single source of truth for when
        /// the next wave starts — leftover enemies do NOT hold the wave open.
        ///
        /// When <see cref="Wave.advanceEarlyOnClear"/> is enabled, the wave may
        /// end sooner if it is fully dispatched and resolved.
        /// </summary>
        private IEnumerator WaveActiveDuration(Wave wave)
        {
            float t = 0f;

            while (t < wave.duration)
            {
                // Optional early exit: the whole wave was dispatched and settled
                // before the timer ran out.
                if (wave.advanceEarlyOnClear
                    && _dispatchComplete
                    && _resolvedCount >= _orderedCount)
                {
                    break;
                }

                yield return null;
                t += Time.deltaTime;
            }

            Debug.Log($"[WaveManager] Wave active duration elapsed ({wave.duration}s).");
        }

        private IEnumerator RestCountdown()
        {
            _restActive = true;
            float t = 0f;

            while (t < restDuration && !_earlyStartRequested)
            {
                yield return null;
                t += Time.deltaTime;

                // Tick the HUD every frame so it can show whole seconds remaining.
                EventRegistry.BroadcastRestCountdown(restDuration - t);
            }

            // Negative sentinel: "no rest period active", HUDs hide themselves.
            EventRegistry.BroadcastRestCountdown(-1f);

            bool cutShort = _earlyStartRequested;
            _restActive = false;
            _earlyStartRequested = false;
            Debug.Log(cutShort
                ? "[WaveManager] Rest cut short by early start."
                : "[WaveManager] Rest countdown finished.");
        }

        // ========== Settlement accounting ==========

        private void HandleEnemyResolved()
        {
            _resolvedCount++;
            Debug.Log($"[WaveManager] Enemy resolved ({_resolvedCount}/{_orderedCount}).");
        }
    }
}
