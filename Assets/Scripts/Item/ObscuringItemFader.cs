using System.Collections;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Item
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ObscuringItemFader : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Color _currentColor;

        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        public void FadeIn()
        {
            StartCoroutine(FadeInCoroutine());
        }

        public void FadeOut()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator FadeInCoroutine()
        {
            _currentColor = _spriteRenderer.color;
            float currentAlpha = _currentColor.a;
            float distance = 1f - currentAlpha;

            while (distance > 0.01f)
            {
                currentAlpha += distance / (Settings.fadeInSeconds * Time.deltaTime);
                _currentColor.a = currentAlpha;
                _spriteRenderer.color = _currentColor;
                yield return null;

                // 更新距离和当前颜色
                distance = 1f - currentAlpha;
            }

            _currentColor.a = 1f;
            _spriteRenderer.color = _currentColor;
        }

        private IEnumerator FadeOutCoroutine()
        {
            _currentColor = _spriteRenderer.color;
            float currentAlpha = _currentColor.a;
            float distance = currentAlpha - Settings.targetAlpha;

            while (distance > 0.01f)
            {
                currentAlpha -= distance / (Settings.fadeOutSeconds * Time.deltaTime);
                _currentColor.a = currentAlpha;
                _spriteRenderer.color = _currentColor;
                yield return null;

                // 更新距离
                distance = currentAlpha - Settings.targetAlpha;
            }

            _currentColor.a = Settings.targetAlpha;
            _spriteRenderer.color = _currentColor;
        }
    }
}