using System.Collections.Generic;
using Assets.Scripts.Item;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utilities.PropertyDrawers.Editor
{
    [CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
    public class ItemCodeDescriptionDrawer : PropertyDrawer
    {
        private SO_ItemList _itemListCache;  // 缓存物品列表
        private string _lastAssetPath;       // 缓存物品列表的资源路径

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) * 2;
        }

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
    }
}