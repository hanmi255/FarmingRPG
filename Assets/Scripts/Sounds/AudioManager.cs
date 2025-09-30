using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.VFX;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Sounds
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        #region Fields
        [SerializeField] private GameObject _soundPrefab = null;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _ambientSoundSource = null;
        [SerializeField] private AudioSource _musicSource = null;

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer = null;

        [Header("Audio Snapshots")]
        [SerializeField] private AudioMixerSnapshot _ambientSoundSnapshot = null;
        [SerializeField] private AudioMixerSnapshot _musicSnapshot = null;

        [Header("Other")]
        [SerializeField] private SO_SoundList _so_soundList = null;
        [SerializeField] private SO_SceneSoundList _so_sceneSoundList = null;
        [SerializeField] private float _defaultSceneMusicPlayTimeSeconds = 120f;
        [SerializeField] private float _sceneMusicStartMinSeconds = 20f;
        [SerializeField] private float _sceneMusicStartMaxSeconds = 40f;
        [SerializeField] private float _musicTransitionSeconds = 8f;

        private Dictionary<SoundName, SoundItem> _soundDictionary;
        private Dictionary<SceneName, SceneSoundItem> _sceneSoundDictionary;

        private Coroutine _playSceneSoundsCoroutine;
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();

            _soundDictionary = new Dictionary<SoundName, SoundItem>();
            foreach (SoundItem soundItem in _so_soundList.soundDetails)
            {
                _soundDictionary.Add(soundItem.soundName, soundItem);
            }

            _sceneSoundDictionary = new Dictionary<SceneName, SceneSoundItem>();
            foreach (SceneSoundItem sceneSoundItem in _so_sceneSoundList.sceneSoundDetails)
            {
                _sceneSoundDictionary.Add(sceneSoundItem.sceneName, sceneSoundItem);
            }
        }

        private void OnEnable()
        {
            Events.EventHandler.AfterSceneLoadEvent += PlaySceneSounds;
        }

        private void OnDisable()
        {
            Events.EventHandler.AfterSceneLoadEvent -= PlaySceneSounds;
        }
        #endregion

        #region Public Methods
        public void PlaySound(SoundName soundName)
        {
            if (!_soundDictionary.TryGetValue(soundName, out SoundItem soundItem) || soundItem == null)
                return;

            GameObject soundObject = PoolManager.Instance.ReuseObject(_soundPrefab, Vector3.zero, Quaternion.identity);
            Sound sound = soundObject.GetComponent<Sound>();
            sound.SetSound(soundItem);
            soundObject.SetActive(true);

            StartCoroutine(DisableSound(soundObject, soundItem.soundClip.length));
        }
        #endregion

        #region Private Methods
        private IEnumerator DisableSound(GameObject soundObject, float duration)
        {
            yield return new WaitForSeconds(duration);
            soundObject.SetActive(false);
        }

        /// <summary>
        /// 播放场景声音
        /// </summary>
        private void PlaySceneSounds()
        {
            if (!Enum.TryParse(SceneManager.GetActiveScene().name, true, out SceneName currentSceneName))
                return;

            if (!_sceneSoundDictionary.TryGetValue(currentSceneName, out SceneSoundItem sceneSoundItem))
                return;

            // 停止当前播放的协程
            if (_playSceneSoundsCoroutine != null)
            {
                StopCoroutine(_playSceneSoundsCoroutine);
            }

            // 获取声音项
            _soundDictionary.TryGetValue(sceneSoundItem.ambientSoundForScene, out SoundItem ambientSoundItem);
            _soundDictionary.TryGetValue(sceneSoundItem.musicForScene, out SoundItem musicSoundItem);

            float musicPlayTime = _defaultSceneMusicPlayTimeSeconds;
            _playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsCoroutine(musicPlayTime, ambientSoundItem, musicSoundItem));
        }

        /// <summary>
        /// 播放场景声音协程
        /// </summary>
        private IEnumerator PlaySceneSoundsCoroutine(float musicPlayTime, SoundItem ambientSoundItem, SoundItem musicSoundItem)
        {
            if(ambientSoundItem == null || musicSoundItem == null)
                yield break;

            PlayAmbientSoundClip(ambientSoundItem, 0f);

            yield return new WaitForSeconds(UnityEngine.Random.Range(_sceneMusicStartMinSeconds, _sceneMusicStartMaxSeconds));

            PlayMusicSoundClip(musicSoundItem, _musicTransitionSeconds);

            yield return new WaitForSeconds(musicPlayTime);

            PlayAmbientSoundClip(ambientSoundItem, _musicTransitionSeconds);
        }

        /// <summary>
        /// 播放环境声音
        /// </summary>
        private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionSeconds)
        {
            PlaySoundClip(ambientSoundItem, _ambientSoundSource, _ambientSoundSnapshot, "AmbientVolume", transitionSeconds);
        }

        /// <summary>
        /// 播放音乐声音
        /// </summary>
        private void PlayMusicSoundClip(SoundItem musicSoundItem, float transitionSeconds)
        {
            PlaySoundClip(musicSoundItem, _musicSource, _musicSnapshot, "MusicVolume", transitionSeconds);
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        private void PlaySoundClip(SoundItem soundItem, AudioSource audioSource, AudioMixerSnapshot snapshot, string volumeParameter, float transitionSeconds)
        {
            _audioMixer.SetFloat(volumeParameter, ConvertSoundVolumeDecimalFractionToDecibels(soundItem.soundVolume));

            audioSource.clip = soundItem.soundClip;
            audioSource.Play();

            snapshot.TransitionTo(transitionSeconds);
        }

        /// <summary>
        /// 将声音音量转换为分贝
        /// </summary>
        private float ConvertSoundVolumeDecimalFractionToDecibels(float volumeDecimalFraction)
        {
            return volumeDecimalFraction * 100f - 80f;
        }
        #endregion
    }
}
