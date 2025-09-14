using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.HelperClasses
{
    /// <summary>
    /// 辅助方法类，提供各种通用的辅助功能方法
    /// </summary>
    public static class HelperMethods
    {
        #region Public Methods
        /// <summary>
        /// 在指定的矩形区域内查找具有指定组件类型的对象
        /// </summary>
        /// <typeparam name="T">要查找的组件类型</typeparam>
        /// <param name="componentList">返回找到的组件列表</param>
        /// <param name="point">矩形区域的中心点</param>
        /// <param name="size">矩形区域的大小</param>
        /// <param name="angle">矩形区域的旋转角度</param>
        /// <returns>如果找到至少一个组件则返回true，否则返回false</returns>
        public static bool GetComponentsAtBoxLocation<T>(out List<T> componentList, Vector2 point, Vector2 size, float angle)
        {
            componentList = new List<T>();

            // 查找与指定矩形区域重叠的所有碰撞体
            Collider2D[] colliders = Physics2D.OverlapBoxAll(point, size, angle);

            // 遍历所有找到的碰撞体，查找指定类型的组件
            foreach (Collider2D collider in colliders)
            {
                // 首先尝试在父对象中查找组件
                T component = collider.gameObject.GetComponentInParent<T>();
                if (component != null)
                {
                    componentList.Add(component);
                    continue;
                }

                // 如果在父对象中未找到，则在子对象中查找组件
                component = collider.gameObject.GetComponentInChildren<T>();
                if (component != null)
                {
                    componentList.Add(component);
                }
            }

            // 返回是否找到组件
            return componentList.Count > 0;
        }
        #endregion
    }
}