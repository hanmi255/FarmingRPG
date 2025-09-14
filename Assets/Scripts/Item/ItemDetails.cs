using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Item
{
    /// <summary>
    /// 物品详细信息类，用于存储游戏中各种物品的详细属性和配置
    /// </summary>
    [System.Serializable]
    public class ItemDetails
    {
        #region Item Identification Fields

        public int itemCode;
        public ItemType itemType;

        #endregion

        #region Item Description Fields

        public string itemName;
        public string itemDescription;

        #endregion

        #region Item Visual Fields

        public Sprite itemSprite;

        #endregion

        #region Item Usage Parameters Fields

        public short itemUseGridRadius;
        public float itemUseRadius;

        #endregion

        #region Item Properties and Functions Fields

        public bool isStartingItem;
        public bool canBePickedUp;
        public bool canBeDropped;
        public bool canBeEaten;
        public bool canBeCarried;

        #endregion
    }
}