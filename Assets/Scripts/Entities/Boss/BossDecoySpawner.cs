using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Boss
{
    public class BossDecoySpawner : MonoBehaviour
    {
        [Header("Boss Decoy")]
        [SerializeField] private GameObject bossDecoyPrefab;

        [Header("Camera Jam Close-Up")]
        [SerializeField] private CameraUIController cameraUIController;
        [SerializeField] private float cameraCloseUpDistance = 0.6f;

        [Header("Turret Jam Anchors")]
        [SerializeField] private Transform room1TurretAnchor;
        [SerializeField] private Transform room2TurretAnchor;
        [SerializeField] private Transform room3TurretAnchor;
        [SerializeField] private Transform room4TurretAnchor;

        private readonly Dictionary<int, GameObject> cameraJamDecoys = new();
        private readonly Dictionary<int, GameObject> turretJamDecoys = new();

        private void Start()
        {
            BossJamManager manager = BossJamManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[BossDecoySpawner] No BossJamManager found; decoy spawning disabled.");
                return;
            }

            foreach (int roomIndex in manager.RoomIndices)
            {
                RoomJamState state = manager.GetState(roomIndex);
                if (state == null) continue;

                state.OnCameraJamStarted += HandleCameraJamStarted;
                state.OnCameraJamCleared += HandleCameraJamCleared;
                state.OnTurretJamStarted += HandleTurretJamStarted;
                state.OnTurretJamCleared += HandleTurretJamCleared;
            }
        }

        private Transform GetTurretAnchor(int roomIndex)
        {
            switch (roomIndex)
            {
                case 1: return room1TurretAnchor;
                case 2: return room2TurretAnchor;
                case 3: return room3TurretAnchor;
                case 4: return room4TurretAnchor;
                default: return null;
            }
        }

        private void HandleCameraJamStarted(int roomIndex)
        {
            if (bossDecoyPrefab == null || cameraUIController == null) return;
            if (cameraJamDecoys.ContainsKey(roomIndex)) return;

            Transform cameraPoint = cameraUIController.GetRoomCameraPoint(roomIndex);
            if (cameraPoint == null)
            {
                Debug.LogWarning($"[BossDecoySpawner] No camera point for room {roomIndex}; can't spawn camera jam decoy.");
                return;
            }

            BossAlarmAudio.Instance.PlayAlarm();
            Vector3 spawnPosition = cameraPoint.position + cameraPoint.forward * cameraCloseUpDistance;
            Quaternion spawnRotation = Quaternion.LookRotation(cameraPoint.position - spawnPosition);

            GameObject decoy = Instantiate(bossDecoyPrefab, spawnPosition, spawnRotation);
            cameraJamDecoys[roomIndex] = decoy;
            decoy.GetComponent<BossSpawnEffect>()?.PlaySpawnEffect();
        }

        private void HandleCameraJamCleared(int roomIndex)
        {
            if (!cameraJamDecoys.TryGetValue(roomIndex, out GameObject decoy)) return;
            cameraJamDecoys.Remove(roomIndex);
            if (decoy == null) return;

            BossSpawnEffect effect = decoy.GetComponent<BossSpawnEffect>();
            if (effect != null) effect.PlayDespawnEffect(() => Destroy(decoy));
            else Destroy(decoy);
        }

        private void HandleTurretJamStarted(int roomIndex)
        {
            if (bossDecoyPrefab == null) return;
            if (turretJamDecoys.ContainsKey(roomIndex)) return;

            Transform anchor = GetTurretAnchor(roomIndex);
            if (anchor == null)
            {
                Debug.LogWarning($"[BossDecoySpawner] No turret anchor assigned for room {roomIndex}; can't spawn turret jam decoy.");
                return;
            }

            BossAlarmAudio.Instance.PlayAlarm();
            GameObject decoy = Instantiate(bossDecoyPrefab, anchor.position, anchor.rotation);
            turretJamDecoys[roomIndex] = decoy;
            decoy.GetComponent<BossSpawnEffect>()?.PlaySpawnEffect();
        }

        private void HandleTurretJamCleared(int roomIndex)
        {
            if (!turretJamDecoys.TryGetValue(roomIndex, out GameObject decoy)) return;
            turretJamDecoys.Remove(roomIndex);
            if (decoy == null) return;

            BossSpawnEffect effect = decoy.GetComponent<BossSpawnEffect>();
            if (effect != null) effect.PlayDespawnEffect(() => Destroy(decoy));
            else Destroy(decoy);
        }
    }
}