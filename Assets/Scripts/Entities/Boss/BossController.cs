using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Boss
{
    /// <summary>
    /// The periodic "brain" of the boss encounter. Once
    /// BossJamManager.IsBossEncounterActive becomes true, every ~IntervalSeconds
    /// it picks one random eligible room and one random system (camera or
    /// turret) and starts a jam via BossJamManager's guarded triggers.
    /// Doesn't spawn decoys or play audio directly - those already react to
    /// the jam-started events independently.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        private enum JamSystem { Camera, Turret }

        [Header("Timing")]
        [Tooltip("Seconds between boss jam attempts, once the encounter is active.")]
        [SerializeField] private float intervalSeconds = 20f;

        private Coroutine loopRoutine;
        private int lastPickedRoom = -1; // -1 = no previous pick yet

        private void OnEnable()
        {
            loopRoutine = StartCoroutine(BossLoop());
        }

        private void OnDisable()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
        }

        private IEnumerator BossLoop()
        {
            while (true)
            {
                // Idle until the encounter actually starts (wave 5, via BossJamManager).
                yield return new WaitUntil(() => BossJamManager.Instance.IsBossEncounterActive);

                while (BossJamManager.Instance.IsBossEncounterActive)
                {
                    yield return new WaitForSeconds(intervalSeconds);

                    if (!BossJamManager.Instance.IsBossEncounterActive) break;

                    TryPickAndStartJam();
                }
            }
        }

        private void TryPickAndStartJam()
        {
            List<int> eligibleRooms = GetEligibleRooms();
            if (eligibleRooms.Count == 0)
            {
                Debug.Log("[BossController] No eligible room to jam this tick (all disabled, or only the previous room remains).");
                return;
            }

            int roomIndex = eligibleRooms[Random.Range(0, eligibleRooms.Count)];
            JamSystem system = (Random.value < 0.5f) ? JamSystem.Camera : JamSystem.Turret;

            bool started = system == JamSystem.Camera
                ? BossJamManager.Instance.TryStartCameraJam(roomIndex)
                : BossJamManager.Instance.TryStartTurretJam(roomIndex);

            if (started)
            {
                lastPickedRoom = roomIndex;
                Debug.Log($"[BossController] Picked room {roomIndex}, system {system}.");
            }
        }

        private List<int> GetEligibleRooms()
        {
            List<int> eligible = new List<int>();

            foreach (int roomIndex in BossJamManager.Instance.RoomIndices)
            {
                if (roomIndex == lastPickedRoom) continue;

                Room room = BossJamManager.Instance.GetRoom(roomIndex);
                if (room != null && room.IsPermanentlyDisabled) continue;

                eligible.Add(roomIndex);
            }

            return eligible;
        }

        [ContextMenu("Test - Force Pick Now")]
        private void TestForcePick() => TryPickAndStartJam();
    }
}