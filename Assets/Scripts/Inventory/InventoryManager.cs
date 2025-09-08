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
            DebugPrintInventoryList(inventoryList);
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

            DebugPrintInventoryList(inventoryList);
        }

        /// <summary>
        /// 获取物品详情
        /// </summary>
        public ItemDetails GetItemDetails(int itemCode)
        {
            _itemDetailsDic.TryGetValue(itemCode, out ItemDetails itemDetails);
            return itemDetails;
        }

        private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
        {
#if UNITY_EDITOR
            foreach (InventoryItem inventoryItem in inventoryList)
            {
                Debug.Log($"Item Name: {Instance.GetItemDetails(inventoryItem.itemCode)?.itemName} Quantity: {inventoryItem.itemQuantity}");
            }
            Debug.Log("-----------------------------");
#endif
        }
    }
}