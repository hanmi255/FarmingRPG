using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Misc;
using Assets.Scripts.VFX;
using UnityEngine;

namespace Assets.Scripts.Sounds
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        [SerializeField] private GameObject _soundPrefab = null;

        [Header("Other")]
        [SerializeField] private SO_SoundList _so_soundList = null;

        private Dictionary<SoundName, SoundItem> _soundDictionary;

        protected override void Awake()
        {
            base.Awake();

            _soundDictionary = new Dictionary<SoundName, SoundItem>();

            foreach (SoundItem soundItem in _so_soundList.soundDetails)
            {
                _soundDictionary.Add(soundItem.soundName, soundItem);
            }
        }

        public void PlaySound(SoundName soundName)
        {
            if(!_soundDictionary.TryGetValue(soundName, out SoundItem soundItem) || soundItem == null)
                return;

            GameObject soundObject = PoolManager.Instance.ReuseObject(_soundPrefab, Vector3.zero, Quaternion.identity);
            Sound sound = soundObject.GetComponent<Sound>();
            sound.SetSound(soundItem);
            soundObject.SetActive(true);

            StartCoroutine(DisableSound(soundObject, soundItem.soundClip.length));
        }

        private IEnumerator DisableSound(GameObject soundObject, float duration)
        {
            yield return new WaitForSeconds(duration);
            soundObject.SetActive(false);
        }
    }
}
