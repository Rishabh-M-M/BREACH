using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Central audio component for an enemy. Handles one-shot SFX (attack,
/// death) plus an optional looping ambient/movement sound (e.g. a hover
/// hum, footsteps, engine whir -- whatever loop makes sense while moving).
///
/// Audibility follows the exact same policy as TurretAudioController.
/// IsRoomAudioAudible: the Main Control Room (room 0) is audible whenever
/// the security-camera screen is closed, and once it's open, only whichever
/// room's feed is currently being viewed is audible. Every sound is gated
/// by this check right before it plays, the same way turret audio works.
///
/// Attach to the enemy's root GameObject, alongside EnemyMover/EnemyHealth.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("One-Shot Clips")]
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip deathClip;

    [Header("One-Shot Volumes")]
    [Range(0f, 1f)] [SerializeField] private float attackVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1f;

    [Header("Ambient Movement Loop (optional)")]
    [Tooltip("Continuous sound played automatically while the enemy is alive and actively moving toward its target.")]
    [SerializeField] private AudioClip movementLoopClip;
    [Range(0f, 1f)] [SerializeField] private float movementVolume = 0.6f;

    [Header("One-Shot Settings")]
    [SerializeField] private float pitchVariance = 0.05f;

    private AudioSource oneShotSource;
    private AudioSource loopSource;
    private EnemyMover mover;
    private EnemyHealth health;

    private void Awake()
    {
        oneShotSource = GetComponent<AudioSource>();
        oneShotSource.loop = false;
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;

        // A second AudioSource, added at runtime, handles the loop so it
        // never gets cut off by one-shot clips sharing the same source.
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f;
        loopSource.volume = movementVolume;
        loopSource.clip = movementLoopClip;

        mover = GetComponent<EnemyMover>();
        health = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (mover == null || movementLoopClip == null) return;

        bool isDead = health != null && health.IsDead;
        bool isMoving = !isDead && !mover.HasArrived;
        bool audible = IsAudible();

        if (isMoving && audible && !loopSource.isPlaying)
        {
            loopSource.Play();
        }
        else if ((!isMoving || !audible) && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    public void PlayAttack()
    {
        if (!IsAudible()) return;
        PlayRandomOneShot(attackClips, attackVolume);
    }

    public void PlayDeath()
    {
        if (loopSource.isPlaying)
        {
            loopSource.Stop();
        }

        if (!IsAudible()) return;

        if (deathClip != null)
        {
            oneShotSource.pitch = 1f;
            oneShotSource.PlayOneShot(deathClip, deathVolume);
        }
    }

    /// <summary>
    /// Same policy as TurretAudioController.IsRoomAudioAudible, just keyed
    /// off this enemy's own CurrentRoomIndex instead of a Turret's OwnerRoom.
    /// </summary>
    private bool IsAudible()
    {
        if (mover == null) return false;

        int roomIndex = mover.CurrentRoomIndex;

        if (!CameraUIController.IsSecurityCameraOpen)
        {
            return roomIndex == 0;
        }

        int viewedRoom = CameraUIController.CurrentViewedRoomIndex;
        if (viewedRoom <= 0) return false;

        return roomIndex == viewedRoom;
    }

    private void PlayRandomOneShot(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        oneShotSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        oneShotSource.PlayOneShot(clip, volume);
    }
}