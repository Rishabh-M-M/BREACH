
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;
using ProjectBreach.Boss;

namespace ProjectBreach.Core
{
    public class WaveManager : Singleton<WaveManager>
    {
        // ========== Inspector configuration ==========

        [Serializable]
        public class WaveGroup
        {
            [Tooltip("Existing spawner instance on this route/door.")]
            public EnemySpawner_Single spawner;

            [Tooltip("How many enemies this door spawns for this wave.")]
            public int count = 1;

            [Tooltip("Stagger delay from the start of the wave.")]
            public float startDelay = 0f;
        }

        [Serializable]
        public class Wave
        {
            public List<WaveGroup> groups = new List<WaveGroup>();

            [Tooltip("How long this wave stays active.")]
            public float duration = 30f;

            [Tooltip("Allow this wave to end early when all dispatched enemies are resolved.")]
            public bool advanceEarlyOnClear = false;
        }

        [Header("Global Pacing")]
        [SerializeField] private float firstWaveDelay = 5f;
        [SerializeField] private float restDuration = 20f;
        [SerializeField] private bool earlyStartEnabled = true;

        [Header("Wave Configuration")]
        [SerializeField] private List<Wave> waves = new List<Wave>();

        [Header("DOMINUS Cinematic")]
        [SerializeField] private int bossWaveIndex = 4;
        [SerializeField] private BossWaveIntroOverlay bossIntroOverlay;

        // ========== Runtime state ==========

        private int _orderedCount;
        private int _resolvedCount;
        private bool _dispatchComplete;
        private int _pendingGroups;
        private bool _restActive;
        private bool _earlyStartRequested;
        private Coroutine _mainRoutine;
        private bool _bossBonusApplied;

        public bool EarlyStartEnabled => earlyStartEnabled;

        // This manager holds direct references to the scene's spawner objects,
        // so it must NOT persist across scene loads. If it did, a restart after
        // a loss would keep a stale instance whose spawner references point at
        // destroyed objects (every group skipped -> nothing spawns) and whose
        // Start() would never re-run the wave loop.
        protected override bool PersistAcrossScenes => false;

        // ========== Lifecycle ==========

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            EventRegistry.OnEnemyResolved += HandleEnemyResolved;
        }

        private void Start()
        {
            if (BossSkipIntroController.IsSkipToBossRun)
            {
                ApplySkipPowerBonus();
                _mainRoutine = StartCoroutine(SkipToBossRoutine());
            }
            else
            {
                _mainRoutine = StartCoroutine(WaveLoop());
            }
        }

        private void OnDestroy()
        {
            EventRegistry.OnEnemyResolved -= HandleEnemyResolved;
        }

        // ========== Early start ==========

        public void RequestEarlyStart()
        {
            if (!earlyStartEnabled)
            {
                Debug.Log("[WaveManager] Early start disabled.");
                return;
            }

            if (!_restActive)
            {
                Debug.Log("[WaveManager] Early start ignored - not in a rest period.");
                return;
            }

            _earlyStartRequested = true;
            Debug.Log("[WaveManager] Early start requested.");
        }

        [ContextMenu("Test - Request Early Start")]
        private void TestRequestEarlyStart()
        {
            RequestEarlyStart();
        }

        // ========== Normal gameplay loop ==========

        private IEnumerator WaveLoop()
        {
            if (waves == null || waves.Count == 0)
            {
                Debug.Log("[WaveManager] No waves configured.");
                yield break;
            }

            if (waves.Count <= bossWaveIndex)
            {
                Debug.LogError("[WaveManager] DOMINUS wave index is missing.");
                yield break;
            }

            yield return new WaitForSeconds(firstWaveDelay);

            for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                // Play cinematic immediately before the DOMINUS wave.
                if (waveIndex == bossWaveIndex)
                {
                    if (bossIntroOverlay != null)
                    {
                        Debug.Log("[WaveManager] Playing DOMINUS cinematic.");
                        yield return bossIntroOverlay.PlayAndWait();
                    }
                    else
                    {
                        Debug.LogError("[WaveManager] Boss intro overlay is not assigned.");
                    }
                }

                Wave wave = waves[waveIndex];

                RunWave(waveIndex, wave);

                yield return WaveActiveDuration(wave);

                bool isLastWave = (waveIndex == waves.Count - 1);

                if (isLastWave)
                {
                    Debug.Log(
                        $"[WaveManager] Wave {waveIndex + 1}/{waves.Count} cleared - all waves done!"
                    );

                    EventRegistry.BroadcastAllWavesCleared();
                    GameManager.Instance.EndGame(true);

                    yield break;
                }

                Debug.Log(
                    $"[WaveManager] Wave {waveIndex + 1}/{waves.Count} cleared - rest period."
                );

                GameManager.Instance.SetWaveClearState();

                yield return RestCountdown();
            }
        }

        // ========== Skip-to-boss route ==========

        private IEnumerator SkipToBossRoutine()
        {
            if (waves == null || waves.Count <= bossWaveIndex)
            {
                Debug.LogError("[WaveManager] DOMINUS wave index is missing.");
                yield break;
            }

            Debug.Log("[WaveManager] Skip route detected. Skipping normal waves.");

            // Apply the bonus only once.
            Wave bossWave = waves[bossWaveIndex];

            RunWave(bossWaveIndex, bossWave);

            yield return WaveActiveDuration(bossWave);

            Debug.Log("[WaveManager] Skipped-to-boss wave cleared.");

            EventRegistry.BroadcastAllWavesCleared();
            GameManager.Instance.EndGame(true);
        }

        private void ApplySkipPowerBonus()
        {
            if (_bossBonusApplied)
                return;

            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.IncreaseMaxPower(200);
                _bossBonusApplied = true;

                Debug.Log("[WaveManager] Added 200 bonus maximum power.");
            }
            else
            {
                Debug.LogError("[WaveManager] PowerManager not found.");
            }
        }

        // ========== Wave dispatch ==========

        private void RunWave(int waveIndex, Wave wave)
        {
            _orderedCount = 0;
            _resolvedCount = 0;
            _dispatchComplete = false;
            _pendingGroups = 0;
            _earlyStartRequested = false;
            _restActive = false;

            EventRegistry.BroadcastWaveStart(waveIndex);

            // Wave 5 starts the DOMINUS interference abilities.
            if (waveIndex == bossWaveIndex)
            {
                if (BossJamManager.Instance != null)
                {
                    BossJamManager.Instance.ActivateBossEncounter();
                }
            }

            if (wave.groups != null)
            {
                foreach (WaveGroup group in wave.groups)
                {
                    if (group == null ||
                        group.spawner == null ||
                        group.count <= 0)
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

        // ========== Wave timer and early clearing ==========

        private IEnumerator WaveActiveDuration(Wave wave)
        {
            float t = 0f;

            while (t < wave.duration)
            {
                // End early only when every dispatched enemy is resolved.
                if (wave.advanceEarlyOnClear &&
                    _dispatchComplete &&
                    _resolvedCount >= _orderedCount)
                {
                    break;
                }

                yield return null;
                t += Time.deltaTime;
            }

            Debug.Log(
                $"[WaveManager] Wave active duration elapsed ({wave.duration}s)."
            );
        }

        // ========== Rest countdown ==========

        private IEnumerator RestCountdown()
        {
            _restActive = true;
            float t = 0f;

            while (t < restDuration && !_earlyStartRequested)
            {
                yield return null;
                t += Time.deltaTime;

                EventRegistry.BroadcastRestCountdown(restDuration - t);
            }

            EventRegistry.BroadcastRestCountdown(-1f);

            bool cutShort = _earlyStartRequested;

            _restActive = false;
            _earlyStartRequested = false;

            Debug.Log(
                cutShort
                    ? "[WaveManager] Rest cut short by early start."
                    : "[WaveManager] Rest countdown finished."
            );
        }

        // ========== Enemy settlement accounting ==========

        private void HandleEnemyResolved()
        {
            _resolvedCount++;

            Debug.Log(
                $"[WaveManager] Enemy resolved ({_resolvedCount}/{_orderedCount})."
            );
        }
    }
}
