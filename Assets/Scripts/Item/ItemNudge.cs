using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Sounds;
using UnityEngine;

namespace Assets.Scripts.Item
{
    /// <summary>
    /// 物品扰动效果类，用于实现物品被碰撞时的晃动动画效果
    /// </summary>
    public class ItemNudge : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// 用于协程暂停的 WaitForSeconds 对象
        /// </summary>
        private WaitForSeconds _pause;

        /// <summary>
        /// 标记是否正在播放动画
        /// </summary>
        private bool _isAnimating = false;

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// Awake 在脚本实例被加载时调用
        /// </summary>
        private void Awake()
        {
            _pause = new WaitForSeconds(0.04f);
        }

        #endregion

        #region Physics Event Handlers

        /// <summary>
        /// 当碰撞体进入触发区域时调用，启动相应的旋转动画
        /// </summary>
        /// <param name="collision">与之发生碰撞的碰撞体</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 如果正在播放动画，则不处理新的碰撞
            if (_isAnimating) return;

            // 根据物品相对位置决定旋转方向
            if (gameObject.transform.position.x < collision.transform.position.x)
            {
                StartCoroutine(RotateAntiClock());
            }
            else
            {
                StartCoroutine(RotateClock());
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                AudioManager.Instance.PlaySound(SoundName.EffectRustle);
            }
        }

        /// <summary>
        /// 当碰撞体离开触发区域时调用，启动相应的旋转动画
        /// </summary>
        /// <param name="collision">与之发生碰撞的碰撞体</param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            // 如果正在播放动画，则不处理新的碰撞
            if (_isAnimating) return;

            // 根据物品相对位置决定旋转方向
            if (gameObject.transform.position.x > collision.transform.position.x)
            {
                StartCoroutine(RotateAntiClock());
            }
            else
            {
                StartCoroutine(RotateClock());
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                AudioManager.Instance.PlaySound(SoundName.EffectRustle);
            }
        }

        #endregion

        #region Animation Coroutine Methods

        /// <summary>
        /// 逆时针旋转动画协程
        /// </summary>
        /// <returns>IEnumerator 用于协程控制</returns>
        private IEnumerator RotateAntiClock()
        {
            _isAnimating = true;

            // 先顺时针旋转一小段
            for (int i = 0; i < 4; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
                yield return _pause;
            }

            // 再逆时针旋转回到原位并超过一点
            for (int i = 0; i < 5; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
                yield return _pause;
            }

            // 最后回到原位
            gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);

            yield return _pause;
            _isAnimating = false;
        }

        /// <summary>
        /// 顺时针旋转动画协程
        /// </summary>
        /// <returns>IEnumerator 用于协程控制</returns>
        private IEnumerator RotateClock()
        {
            _isAnimating = true;

            // 先逆时针旋转一小段
            for (int i = 0; i < 4; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
                yield return _pause;
            }

            // 再顺时针旋转回到原位并超过一点
            for (int i = 0; i < 5; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
                yield return _pause;
            }

            // 最后回到原位
            gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);

            yield return _pause;
            _isAnimating = false;
        }

        #endregion
    }
}