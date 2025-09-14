using System.Collections;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Item
{
    /// <summary>
    /// 遮挡物品淡入淡出效果类，用于控制物品在被遮挡时的透明度变化
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ObscuringItemFader : MonoBehaviour
    {
        #region Fields

        private SpriteRenderer _spriteRenderer;
        private Color _currentColor;

        #endregion

        #region Lifecycle Methods

        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 淡入效果，将物品逐渐变为不透明
        /// </summary>
        public void FadeIn()
        {
            StartCoroutine(FadeInCoroutine());
        }

        /// <summary>
        /// 淡出效果，将物品逐渐变为透明
        /// </summary>
        public void FadeOut()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// 淡入协程，控制物品逐渐变为不透明的过程
        /// </summary>
        /// <returns>IEnumerator用于协程控制</returns>
        private IEnumerator FadeInCoroutine()
        {
            _currentColor = _spriteRenderer.color;
            float currentAlpha = _currentColor.a;
            float distance = 1f - currentAlpha;

            // 持续更新透明度直到接近完全不透明
            while (distance > 0.01f)
            {
                currentAlpha += distance / (Settings.fadeInSeconds * Time.deltaTime);
                _currentColor.a = currentAlpha;
                _spriteRenderer.color = _currentColor;
                yield return null;

                // 更新距离和当前颜色
                distance = 1f - currentAlpha;
            }

            // 确保最终完全不透明
            _currentColor.a = 1f;
            _spriteRenderer.color = _currentColor;
        }

        /// <summary>
        /// 淡出协程，控制物品逐渐变为透明的过程
        /// </summary>
        /// <returns>IEnumerator用于协程控制</returns>
        private IEnumerator FadeOutCoroutine()
        {
            _currentColor = _spriteRenderer.color;
            float currentAlpha = _currentColor.a;
            float distance = currentAlpha - Settings.targetAlpha;

            // 持续更新透明度直到接近目标透明度
            while (distance > 0.01f)
            {
                currentAlpha -= distance / (Settings.fadeOutSeconds * Time.deltaTime);
                _currentColor.a = currentAlpha;
                _spriteRenderer.color = _currentColor;
                yield return null;

                // 更新距离
                distance = currentAlpha - Settings.targetAlpha;
            }

            // 确保最终达到目标透明度
            _currentColor.a = Settings.targetAlpha;
            _spriteRenderer.color = _currentColor;
        }

        #endregion
    }
}