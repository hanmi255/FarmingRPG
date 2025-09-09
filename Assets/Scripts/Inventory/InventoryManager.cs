using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Inventory
{
    public class InventoryManager : SingletonMonoBehaviour<InventoryManager>
    {
        private Dictionary<int, ItemDetails> _itemDetailsDic;            // 物品详情字典
        private List<InventoryItem>[] _inventoryLists;                   // 背包列表
        [HideInInspector] private int[] _inventoryListCapacityIntArray;  // 背包列表容量
        [SerializeField] private SO_ItemList _itemList = null;           // 物品列表

        protected override void Awake()
        {
            base.Awake();
            CreateInventoryLists();
            CreateItemDetailsDic();
        }

        /// <summary>
        /// 创建背包列表
        /// </summary>
        private void CreateInventoryLists()
        {
            _inventoryLists = new List<InventoryItem>[(int)InventoryLocation.Count];

            for (int i = 0; i < (int)InventoryLocation.Count; i++)
            {
                _inventoryLists[i] = new List<InventoryItem>();
            }

            _inventoryListCapacityIntArray = new int[(int)InventoryLocation.Count];
            _inventoryListCapacityIntArray[(int)InventoryLocation.Player] = Settings.playerInitialInventoryCapacity;
        }

        /// <summary>
        /// 创建物品详情字典
        /// </summary>
        private void CreateItemDetailsDic()
        {
            _itemDetailsDic = new Dictionary<int, ItemDetails>();

            if (_itemList?.itemDetails != null)
            {
                foreach (ItemDetails itemDetails in _itemList.itemDetails)
                {
                    _itemDetailsDic.Add(itemDetails.itemCode, itemDetails);
                }
            }
        }

        /// <summary>
        /// 添加物品到背包并销毁物品对象
        /// </summary>
        public void AddItem(InventoryLocation inventoryLocation, ItemUnit item, GameObject gameObjectToDestroy)
        {
            AddItem(inventoryLocation, item);
            Destroy(gameObjectToDestroy);
        }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        public void AddItem(InventoryLocation inventoryLocation, ItemUnit item)
        {
            int itemCode = item.ItemCode;
            List<InventoryItem> inventoryList = _inventoryLists[(int)inventoryLocation];

            // 如果已经存在该物品则增加数量
            // 否则添加物品
            int itemPosition = FindItemInInventory(inventoryLocation, itemCode);
            if (itemPosition != -1)
            {
                AddItemAtPosition(inventoryList, itemCode, itemPosition);
            }
            else
            {
                AddItemAtPosition(inventoryList, itemCode);
            }

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, _inventoryLists[(int)inventoryLocation]);
        }

        /// <summary>
        /// 从背包中移除物品
        /// </summary>
        public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
        {
            List<InventoryItem> inventoryList = _inventoryLists[(int)inventoryLocation];

            int itemPosition = FindItemInInventory(inventoryLocation, itemCode);
            if (itemPosition != -1)
            {
                RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
            }

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, _inventoryLists[(int)inventoryLocation]);
        }

        /// <summary>
        /// 交换两个物品的位置
        /// </summary>
        public void SwapInventoryItems(InventoryLocation player, int fromSlot, int toSlot)
        {
            if (fromSlot < _inventoryLists[(int)player].Count && toSlot < _inventoryLists[(int)player].Count
                && fromSlot != toSlot && fromSlot >= 0 && toSlot >= 0)
            {
                InventoryItem fromItem = _inventoryLists[(int)player][fromSlot];
                InventoryItem toItem = _inventoryLists[(int)player][toSlot];

                _inventoryLists[(int)player][fromSlot] = toItem;
                _inventoryLists[(int)player][toSlot] = fromItem;

                EventHandler.CallInventoryUpdatedEvent(player, _inventoryLists[(int)player]);
            }

        }

        /// <summary>
        /// 查找物品是否在背包中
        /// 返回物品在背包中的位置 或 -1 表示未找到
        /// </summary>
        private int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
        {
            List<InventoryItem> inventoryList = _inventoryLists[(int)inventoryLocation];

            for (int i = 0; i < inventoryList.Count; i++)
            {
                if (inventoryList[i].itemCode == itemCode)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 添加物品到背包指定位置 计数+1
        /// </summary>
        private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
        {
            InventoryItem inventoryItem = inventoryList[itemPosition];
            inventoryItem.itemQuantity++;
            inventoryList[itemPosition] = inventoryItem;
        }

        /// <summary>
        /// 添加物品到背包末尾
        /// </summary>
        private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
        {
            InventoryItem inventoryItem = new()
            {
                itemCode = itemCode,
                itemQuantity = 1
            };
            inventoryList.Add(inventoryItem);
        }

        /// <summary>
        /// 移除背包指定位置的物品 计数-1
        /// 如果物品数量为0 则移除索引
        /// </summary>
        private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
        {
            InventoryItem inventoryItem = inventoryList[itemPosition];
            inventoryItem.itemQuantity--;

            if (inventoryItem.itemQuantity <= 0)
            {
                inventoryList.RemoveAt(itemPosition);
            }
            else
            {
                inventoryList[itemPosition] = inventoryItem;
            }
        }

        /// <summary>
        /// 获取物品详情
        /// </summary>
        public ItemDetails GetItemDetails(int itemCode)
        {
            _itemDetailsDic.TryGetValue(itemCode, out ItemDetails itemDetails);
            return itemDetails;
        }
    }
}