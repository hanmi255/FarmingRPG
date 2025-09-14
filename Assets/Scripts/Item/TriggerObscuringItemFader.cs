using UnityEngine;

namespace Assets.Scripts.Item
{
    /// <summary>
    /// 触发遮挡物品淡入淡出效果类，当玩家进入或离开触发区域时控制遮挡物品的透明度变化
    /// </summary>
    public class TriggerObscuringItemFader : MonoBehaviour
    {
        #region Physics Event Handlers

        /// <summary>
        /// 当碰撞体进入触发区域时调用，触发射出效果使遮挡物品淡出
        /// </summary>
        /// <param name="collision">与之发生碰撞的碰撞体</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 获取碰撞对象及其子对象上的所有遮挡物品淡入淡出组件
            ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

            // 对每个组件执行淡出效果
            foreach (ObscuringItemFader fader in obscuringItemFaders)
            {
                fader.FadeOut();
            }
        }

        /// <summary>
        /// 当碰撞体离开触发区域时调用，触发射入效果使遮挡物品淡入
        /// </summary>
        /// <param name="collision">与之发生碰撞的碰撞体</param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            // 获取碰撞对象及其子对象上的所有遮挡物品淡入淡出组件
            ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

            // 对每个组件执行淡入效果
            foreach (ObscuringItemFader fader in obscuringItemFaders)
            {
                fader.FadeIn();
            }
        }

        #endregion
    }
}