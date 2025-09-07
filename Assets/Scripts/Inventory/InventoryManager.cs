using System.Collections.Generic;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Inventory
{
    public class InventoryManager : SingletonMonoBehaviour<InventoryManager>
    {
        private Dictionary<int, ItemDetails> _itemDetailsDic;

        [SerializeField] private SO_ItemList _itemList = null;

        private void Start()
        {
            CreateItemDetailsDic();
        }

        /// <summary>
        /// 创建物品详情字典
        /// </summary>
        private void CreateItemDetailsDic()
        {
            _itemDetailsDic = new Dictionary<int, ItemDetails>();

            foreach (ItemDetails itemDetails in _itemList.itemDetails)
            {
                _itemDetailsDic.Add(itemDetails.itemCode, itemDetails);
            }
        }

        public ItemDetails GetItemDetails(int itemCode)
        {
            if (_itemDetailsDic.TryGetValue(itemCode, out ItemDetails itemDetails))
            {
                return itemDetails;
            }
            else
            {
                return null;
            }
        }
    }
}
