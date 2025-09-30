using UnityEngine;

namespace Assets.Scripts.Sounds
{
    [RequireComponent(typeof(AudioSource))]
    public class Sound : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if(_audioSource.clip != null)
            {
                _audioSource.Play();
            }
        }

        private void OnDisable()
        {
            _audioSource.Stop();
        }

        public void SetSound(SoundItem soundItem)
        {
            _audioSource.clip = soundItem.soundClip;
            _audioSource.pitch = Random.Range(soundItem.soundPitchRandomVariationMin, soundItem.soundPitchRandomVariationMax);
            _audioSource.volume = soundItem.soundVolume;
        }
    }
}
