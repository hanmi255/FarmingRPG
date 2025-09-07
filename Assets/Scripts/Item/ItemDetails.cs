using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Item
{
    [System.Serializable]
    public struct ItemDetails
    {
        // 物品标识
        public int itemCode;
        public ItemType itemType;

        // 物品描述
        public string itemShortDescription;
        public string itemLongDescription;

        // 物品视觉效果
        public Sprite itemSprite;

        // 物品使用参数
        public short itemUseGridRadius;
        public float itemUseRadius;

        // 物品属性和功能
        public bool isStartingItem;
        public bool canBePickedUp;
        public bool canBeDropped;
        public bool canBeEaten;
        public bool canBeCarried;
    }
}