using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.TimeSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Lighting
{
    public class LightingControl : MonoBehaviour
    {
        [SerializeField] private LightingSchedule _lightingSchedule;
        [SerializeField] private bool _isLightFlicker = false;
        [SerializeField][Range(0f, 1f)] private float _lightFlickerIntensity = 0.5f;
        [SerializeField][Range(0f, 0.2f)] private float _lightFlickerTimeMin = 0.1f;
        [SerializeField][Range(0f, 0.2f)] private float _lightFlickerTimeMax = 0.2f;

        private const float FADE_IN_DURATION = 5f;

        private Light2D _light2D;
        private readonly Dictionary<(Season gameSeason, int gameHour), float> _lightingBrightnessDict = new();
        private float _currentLightIntensity;
        private float _flickerTimer;
        private Coroutine _fadeInLightCoroutine;

        private void Awake()
        {
            _light2D = GetComponentInChildren<Light2D>();

            if (_light2D == null)
                enabled = false;

            foreach (var lightingBrightness in _lightingSchedule.lightingBrightnessArray)
            {
                var key = (lightingBrightness.season, lightingBrightness.hour);
                _lightingBrightnessDict.Add(key, lightingBrightness.lightIntensity);
            }
        }

        private void OnEnable()
        {
            EventHandler.AdvanceGameHourEvent += AdvanceGameHourEvent;
            EventHandler.AfterSceneLoadEvent += AfterSceneLoadEvent;
        }

        private void Update()
        {
            if (_isLightFlicker)
                _flickerTimer -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (_isLightFlicker && _flickerTimer <= 0f)
            {
                LightFlicker();
            }
            else
            {
                _light2D.intensity = _currentLightIntensity;
            }
        }

        private void OnDisable()
        {
            EventHandler.AdvanceGameHourEvent -= AdvanceGameHourEvent;
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoadEvent;
        }

        private void AdvanceGameHourEvent(TimeEventParameters parameters)
        {
            SetLightingIntensity(parameters.gameSeason, parameters.gameHour, true);
        }

        private void AfterSceneLoadEvent()
        {
            SetLightingIntensityAfterSceneLoad();
        }

        private void SetLightingIntensity(Season gameSeason, int gameHour, bool isFading)
        {
            // 1. 规范化小时到 [0, 23]
            gameHour = ((gameHour % 24) + 24) % 24;

            // 2. 从 gameHour 向前回绕查找，找到第一个有配置的小时
            float targetIntensity = 1f;
            for (int offset = 0; offset < 24; offset++)
            {
                int hour = (gameHour - offset + 24) % 24;
                if (_lightingBrightnessDict.TryGetValue((gameSeason, hour), out targetIntensity))
                    break;
            }

            // 3. 淡入 或 立即生效
            if (isFading)
            {
                if (_fadeInLightCoroutine != null)
                    StopCoroutine(_fadeInLightCoroutine);

                _fadeInLightCoroutine = StartCoroutine(FadeInLightRoutine(targetIntensity));
            }
            else
            {
                // 立即生效时先停掉正在跑的淡入，避免协程随后又覆盖回来
                if (_fadeInLightCoroutine != null)
                {
                    StopCoroutine(_fadeInLightCoroutine);
                    _fadeInLightCoroutine = null;
                }

                _currentLightIntensity = targetIntensity;
            }
        }

        private void SetLightingIntensityAfterSceneLoad()
        {
            Season gameSeason = TimeManager.Instance.GetGameSeason();
            int gameHour = TimeManager.Instance.GetGameTime().Hours;

            SetLightingIntensity(gameSeason, gameHour, false);
        }

        private IEnumerator FadeInLightRoutine(float targetLightIntensity)
        {
            float elapsedTime = 0f;
            float startIntensity = _currentLightIntensity;

            while (elapsedTime < FADE_IN_DURATION)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / FADE_IN_DURATION);
                _currentLightIntensity = Mathf.Lerp(startIntensity, targetLightIntensity, t);
                yield return null;
            }

            _currentLightIntensity = targetLightIntensity;
            _fadeInLightCoroutine = null;
        }

        private void LightFlicker()
        {
            _light2D.intensity = Random.Range(_currentLightIntensity, _currentLightIntensity + (_currentLightIntensity * _lightFlickerIntensity));
            _flickerTimer = Random.Range(_lightFlickerTimeMin, _lightFlickerTimeMax);
        }
    }
}
