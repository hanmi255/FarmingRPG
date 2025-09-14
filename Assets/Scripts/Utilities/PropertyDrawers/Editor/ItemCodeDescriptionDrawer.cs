using System.Collections.Generic;
using Assets.Scripts.Item;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utilities.PropertyDrawers.Editor
{
    /// <summary>
    /// 物品代码描述属性绘制器 - 用于在Unity编辑器中显示物品代码及其对应名称
    /// </summary>
    [CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
    public class ItemCodeDescriptionDrawer : PropertyDrawer
    {
        #region Fields

        private SO_ItemList _itemListCache;  // 缓存物品列表
        private string _lastAssetPath;       // 缓存物品列表的资源路径

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取属性的高度
        /// </summary>
        /// <param name="property">序列化属性</param>
        /// <param name="label">属性标签</param>
        /// <returns>属性的高度</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) * 2;
        }

        /// <summary>
        /// 绘制属性的GUI界面
        /// </summary>
        /// <param name="position">绘制位置</param>
        /// <param name="property">序列化属性</param>
        /// <param name="label">属性标签</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();

                var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), label, property.intValue);

                EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2), "Item Name", GetItemDescription(property.intValue));

                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = newValue;
                }
            }
            EditorGUI.EndProperty();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 获取物品描述信息
        /// </summary>
        /// <param name="itemCode">物品代码</param>
        /// <returns>物品描述信息</returns>
        private string GetItemDescription(int itemCode)
        {
            // 如果物品代号为默认值0，则返回空字符串
            if (itemCode == 0)
                return "";

            // 加载或重新加载物品列表
            string assetPath = "Assets/ScriptableObjects/Item/so_ItemList.asset";
            if (_itemListCache == null || _lastAssetPath != assetPath)
            {
                _itemListCache = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SO_ItemList)) as SO_ItemList;
                _lastAssetPath = assetPath;
            }

            // 如果无法加载物品列表，返回错误信息
            if (_itemListCache == null)
            {
                return "ERROR: Item list not found";
            }

            // 查找物品详细信息
            ItemDetails itemDetailsObject = _itemListCache.itemDetails.Find(x => x.itemCode == itemCode);

            // 返回物品名称或空字符串
            return itemDetailsObject != null ? itemDetailsObject.itemName : $"Unknown item ({itemCode})";
        }

        #endregion
    }
}