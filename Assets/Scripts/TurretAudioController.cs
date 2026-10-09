using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// One shared audio configuration for every turret in the game.
    ///
    /// Put exactly one TurretAudioController in the scene and assign the
    /// one shared audio configuration here. Individual turrets automatically
    /// create their own local AudioSources, so no audio references need to be
    /// assigned on every turret.
    /// </summary>
    public class TurretAudioController : Singleton<TurretAudioController>
    {
        [Header("Turret Fire Sounds")]
        [Tooltip("Base turret firing sound.")]
        [SerializeField] private AudioClip baseFireSound;

        [Tooltip("Double Barrel upgrade firing sound.")]
        [SerializeField] private AudioClip doubleBarrelFireSound;

        [Tooltip("Sharpshooter upgrade firing sound.")]
        [SerializeField] private AudioClip sharpshooterFireSound;

        [Tooltip("Helios Incinerator laser sound. This clip loops while the beam fires and stops on cooldown; use a seamless loop if possible.")]
        [SerializeField] private AudioClip heliosIncineratorFireSound;

        [Tooltip("Shared looping sound used whenever a turret head is rotating.")]
        [SerializeField] private AudioClip rotationSound;

        [Tooltip("Played when a turret successfully allocates operating power.")]
        [SerializeField] private AudioClip powerOnSound;

        [Tooltip("Played when a turret releases its operating power.")]
        [SerializeField] private AudioClip powerOffSound;

        [Header("Shared Volumes")]
        [SerializeField, Range(0f, 1f)] private float fireVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float rotationVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float powerVolume = 1f;

        [Header("Rotation Audio")]
        [Tooltip("Minimum head rotation angle per frame before the rotation sound starts.")]
        [SerializeField] private float rotationAudioThreshold = 0.05f;

        public AudioClip GetFireSound(TurretVariant variant)
        {
            switch (variant)
            {
                case TurretVariant.DoubleBarrel:
                    return doubleBarrelFireSound;

                case TurretVariant.Sharpshooter:
                    return sharpshooterFireSound;

                case TurretVariant.HeliosIncinerator:
                    return heliosIncineratorFireSound;

                default:
                    return baseFireSound;
            }
        }

        public AudioClip RotationSound => rotationSound;
        public AudioClip PowerOnSound => powerOnSound;
        public AudioClip PowerOffSound => powerOffSound;

        private const float VolumeGain = 20f;

        public float FireVolume => fireVolume * VolumeGain;
        public float RotationVolume => rotationVolume * VolumeGain;
        public float PowerVolume => powerVolume * VolumeGain;
        public float RotationAudioThreshold => rotationAudioThreshold;

        private void OnValidate()
        {
            fireVolume = Mathf.Clamp01(fireVolume);
            rotationVolume = Mathf.Clamp01(rotationVolume);
            powerVolume = Mathf.Clamp01(powerVolume);
        }

        /// <summary>
        /// Returns true only when the camera is currently viewing the room
        /// that owns this turret. Turret combat continues regardless of the
        /// camera; this only controls whether turret audio is audible.
        /// </summary>
        public bool IsRoomAudioAudible(Turret turret)
        {
            if (turret == null)
                return false;

            // Room 0 is the Main Control Room. It is audible only while
            // the security-camera screen is closed.
            if (!CameraUIController.IsSecurityCameraOpen)
            {
                return GetTurretRoomIndex(turret) == 0;
            }

            // When the security-camera screen is open, only the room
            // currently being viewed is audible.
            int viewedRoom = CameraUIController.CurrentViewedRoomIndex;
            if (viewedRoom <= 0)
                return false;

            return GetTurretRoomIndex(turret) == viewedRoom;
        }

        private int GetTurretRoomIndex(Turret turret)
        {
            // Prefer the actual Room registration.
            if (turret.OwnerRoom != null &&
                turret.OwnerRoom.RoomIndex > 0)
            {
                return turret.OwnerRoom.RoomIndex;
            }

            // Fallback to the turret naming convention used by the game:
            // Room1Turret1, Room2Turret3, Room3Turret1, Room4Turret2, etc.
            string turretName = turret.name;

            for (int room = 1; room <= 4; room++)
            {
                if (turretName.StartsWith("Room" + room))
                    return room;
            }

            // Turrets without a Room# prefix and without a Room component
            // belong to the Main Control Room.
            return 0;
        }
    }
}
