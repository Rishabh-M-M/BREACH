using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    public enum TrapGroupType
    {
        GravitySpikes,
        StasisField
    }

    public class TrapGroup : DefensiveStructure
    {
        [Header("Trap Group")]
        [SerializeField] private TrapGroupType trapType = TrapGroupType.GravitySpikes;
        [Tooltip("Optional testing shortcut. Disable this once UI buttons are wired.")]
        [SerializeField] private bool enableKeyboardTesting = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.T;

        [Tooltip("Assign the tiles explicitly to avoid nested groups controlling the same tiles.")]
        [SerializeField] private SpikeTrap[] gravitySpikeTiles;
        [SerializeField] private StasisFieldTile[] stasisFieldTiles;

        [Header("Gravity Spikes Settings")]
        [Min(0)][SerializeField] private int damagePerTick = 1;
        [Min(0.02f)][SerializeField] private float damageInterval = 0.1f;

        [Header("Stasis Field Pull Settings")]
        [Tooltip("Pull strength in world units per second. Start around 3-5 and tune in Play Mode.")]
        [Min(0f)][SerializeField] private float pullSpeed = 4f;
        [Tooltip("Duration of each pull application. Must be longer than the tile's pull refresh interval.")]
        [Min(0.1f)][SerializeField] private float pullApplicationDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip activeLoopSound;
        [Range(0f, 1f)][SerializeField] private float activeLoopVolume = 1f;
        [Tooltip("Assign a turret from the same room so audio follows TurretAudioController room audibility.")]
        [SerializeField] private Turret roomAudioReference;

        private AudioSource loopAudioSource;
        private bool holdsOperatingPower;

        public TrapGroupType Type => trapType;
        public bool IsGroupActive => isActive;

        protected override void Awake()
        {
            base.Awake();

            if (gravitySpikeTiles == null || gravitySpikeTiles.Length == 0)
                gravitySpikeTiles = GetComponentsInChildren<SpikeTrap>(true);

            if (stasisFieldTiles == null || stasisFieldTiles.Length == 0)
                stasisFieldTiles = GetComponentsInChildren<StasisFieldTile>(true);

            loopAudioSource = GetComponent<AudioSource>();
            if (loopAudioSource == null)
                loopAudioSource = gameObject.AddComponent<AudioSource>();

            loopAudioSource.playOnAwake = false;
            loopAudioSource.loop = true;
            loopAudioSource.spatialBlend = 0f;
            loopAudioSource.volume = Mathf.Clamp01(activeLoopVolume);
            loopAudioSource.clip = activeLoopSound;

            ConfigureTiles();
            SetTilesActive(false);
        }

        private void Start()
        {
            // Re-acquire operating power through TurnOn rather than assuming a
            // serialized active state is still funded.
            if (isActive)
            {
                base.TurnOff();
                TurnOn();
            }
        }

        private void Update()
        {
            if (enableKeyboardTesting && Input.GetKeyDown(toggleKey))
                Toggle();
        }

        private void ConfigureTiles()
        {
            if (gravitySpikeTiles != null)
            {
                foreach (SpikeTrap tile in gravitySpikeTiles)
                {
                    if (tile != null)
                        tile.SetCombatSettings(damagePerTick, damageInterval);
                }
            }

            if (stasisFieldTiles != null)
            {
                foreach (StasisFieldTile tile in stasisFieldTiles)
                {
                    if (tile != null)
                        tile.Configure(pullSpeed, pullApplicationDuration);
                }
            }
        }

        public void Toggle()
        {
            if (isActive)
                TurnOff();
            else
                TurnOn();
        }

        public override void TurnOn()
        {
            if (isActive)
                return;

            if (!HasConfiguredTiles())
            {
                Debug.LogError(
                    $"TrapGroup '{name}' cannot activate: no valid tile components are assigned for trap type '{trapType}'.",
                    this);
                UIManager.Instance?.ShowMessage("No trap tiles are assigned to this trap group.");
                return;
            }

            ConfigureTiles();

            PowerManager power = PowerManager.Instance;
            if (power != null)
            {
                if (!power.TryAllocate(PowerCost))
                {
                    UIManager.Instance?.ShowInsufficientPowerMessage();
                    return;
                }

                holdsOperatingPower = true;
            }

            // Activate the actual trap tiles as part of the same operation as
            // the group state, so the UI cannot report ON while tiles stay off.
            SetTilesActive(true);
            base.TurnOn();
            UpdateLoopAudio();
        }

        public override void TurnOff()
        {
            if (!isActive && !holdsOperatingPower)
            {
                StopLoopAudio();
                SetTilesActive(false);
                return;
            }

            SetTilesActive(false);
            base.TurnOff();

            if (holdsOperatingPower)
            {
                PowerManager.Instance?.Refund(PowerCost);
                holdsOperatingPower = false;
            }

            StopLoopAudio();
        }

        public override void OnPowerCut()
        {
            TurnOff();
        }

        private bool HasConfiguredTiles()
        {
            if (trapType == TrapGroupType.GravitySpikes)
            {
                if (gravitySpikeTiles == null)
                    return false;

                foreach (SpikeTrap tile in gravitySpikeTiles)
                {
                    if (tile != null)
                        return true;
                }

                return false;
            }

            if (stasisFieldTiles == null)
                return false;

            foreach (StasisFieldTile tile in stasisFieldTiles)
            {
                if (tile != null)
                    return true;
            }

            return false;
        }

        private void SetTilesActive(bool value)
        {
            if (trapType == TrapGroupType.GravitySpikes)
            {
                if (gravitySpikeTiles == null)
                    return;

                foreach (SpikeTrap tile in gravitySpikeTiles)
                {
                    if (tile != null)
                        tile.SetActive(value);
                }
            }
            else
            {
                if (stasisFieldTiles == null)
                    return;

                foreach (StasisFieldTile tile in stasisFieldTiles)
                {
                    if (tile != null)
                        tile.SetActive(value);
                }
            }
        }

        private void UpdateLoopAudio()
        {
            if (loopAudioSource == null || activeLoopSound == null || !isActive)
            {
                StopLoopAudio();
                return;
            }

            loopAudioSource.clip = activeLoopSound;
            loopAudioSource.volume = Mathf.Clamp01(activeLoopVolume);

            if (IsRoomAudible() && !loopAudioSource.isPlaying)
                loopAudioSource.Play();
            else if (!IsRoomAudible())
                loopAudioSource.Stop();
        }

        private void LateUpdate()
        {
            if (!isActive || activeLoopSound == null || loopAudioSource == null)
                return;

            bool audible = IsRoomAudible();

            if (audible && !loopAudioSource.isPlaying)
                loopAudioSource.Play();
            else if (!audible && loopAudioSource.isPlaying)
                loopAudioSource.Stop();
        }

        private bool IsRoomAudible()
        {
            TurretAudioController audioController = TurretAudioController.Instance;

            return audioController != null &&
                   roomAudioReference != null &&
                   audioController.IsRoomAudioAudible(roomAudioReference);
        }

        private void StopLoopAudio()
        {
            if (loopAudioSource != null)
                loopAudioSource.Stop();
        }

        private void OnDisable()
        {
            StopLoopAudio();
            SetTilesActive(false);

            if (isActive)
                base.TurnOff();

            if (holdsOperatingPower)
            {
                PowerManager.Instance?.Refund(PowerCost);
                holdsOperatingPower = false;
            }
        }

        private void OnDestroy()
        {
            SetTilesActive(false);
        }
    }
}
