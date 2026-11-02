using UnityEngine;

namespace ProjectBreach.Boss
{
    [RequireComponent(typeof(AudioSource))]
    public class BossAlarmAudio : Core.Singleton<BossAlarmAudio>
    {
        [SerializeField] private AudioClip alarmClip;
        [SerializeField] private float volume = 1f;

        private AudioSource audioSource;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // force 2D/global, ignores position
        }

        public void PlayAlarm()
        {
            if (alarmClip == null)
            {
                Debug.LogWarning("[BossAlarmAudio] No alarm clip assigned.");
                return;
            }

            audioSource.PlayOneShot(alarmClip, volume);
        }
    }
}