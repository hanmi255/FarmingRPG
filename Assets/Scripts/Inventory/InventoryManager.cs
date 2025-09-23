using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using Assets.Scripts.Player;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.UI.UIInventory;
using UnityEngine;

namespace Assets.Scripts.Inventory
{
    [RequireComponent(typeof(GenerateGUID))]
    /// <summary>
    /// 背包管理器，负责管理游戏中的各种背包和物品
    /// </summary>
    public class InventoryManager : SingletonMonoBehaviour<InventoryManager>, ISaveable
    {
        #region Fields

        #region Private Fields
        private Dictionary<int, ItemDetails> _itemDetailsDic;            // 物品详情字典
        [HideInInspector] public List<InventoryItem>[] inventoryLists;   // 背包列表
        [HideInInspector] public int[] inventoryListCapacityIntArray;    // 背包列表容量
        private int[] selectedInventoryItem;                             // index是 inventory_list, value是 item_code
        private string _iSaveableUniqueID;                               // 保存ID
        private GameObjectSave _gameObjectSave;                          // 游戏对象保存
        private UIInventoryBar _uiInventoryBar;                          // UI背包栏

        public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

        public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }
        #endregion

        #region Serialization Fields
        [SerializeField] private SO_ItemList _itemList = null;           // 物品列表
        #endregion

        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 初始化背包管理器，创建背包列表和物品详情字典
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            CreateInventoryLists();
            CreateItemDetailsDic();

            // 初始化 selectedInventoryItem
            selectedInventoryItem = new int[(int)InventoryLocation.Count];
            // 使用-1填充数组，表示没有选中任何物品
            System.Array.Fill(selectedInventoryItem, -1);

            _iSaveableUniqueID = GetComponent<GenerateGUID>().GUID;
            _gameObjectSave = new GameObjectSave();

            _uiInventoryBar = FindObjectOfType<UIInventoryBar>();
        }

        private void Start()
        {
            _uiInventoryBar = FindObjectOfType<UIInventoryBar>();
        }

        private void OnEnable()
        {
            ISaveableRegister();
        }

        private void OnDisable()
        {
            ISaveableDeregister();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 添加物品到背包并销毁物品对象
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="item">物品单元</param>
        /// <param name="gameObjectToDestroy">要销毁的游戏对象</param>
        public void AddItem(InventoryLocation inventoryLocation, ItemUnit item, GameObject gameObjectToDestroy)
        {
            AddItem(inventoryLocation, item.ItemCode);
            Destroy(gameObjectToDestroy);
        }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="item">物品单元</param>
        public void AddItem(InventoryLocation inventoryLocation, ItemUnit item)
        {
            AddItem(inventoryLocation, item.ItemCode);
        }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="itemCode">物品代码</param>
        public void AddItem(InventoryLocation inventoryLocation, int itemCode)
        {
            List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

            int itemPosition = FindItemInInventory(inventoryLocation, itemCode);
            if (itemPosition != -1)
            {
                AddItemAtPosition(inventoryList, itemPosition);
            }
            else
            {
                AddNewItemToInventory(inventoryList, itemCode);
            }

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }

        /// <summary>
        /// 从背包中移除物品
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="itemCode">物品代码</param>
        public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
        {
            List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

            int itemPosition = FindItemInInventory(inventoryLocation, itemCode);
            if (itemPosition != -1)
            {
                RemoveItemAtPosition(inventoryList, itemPosition);
            }

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }

        /// <summary>
        /// 交换两个物品的位置
        /// </summary>
        /// <param name="player">玩家背包位置</param>
        /// <param name="fromSlot">起始槽位</param>
        /// <param name="toSlot">目标槽位</param>
        public void SwapInventoryItems(InventoryLocation player, int fromSlot, int toSlot)
        {
            // 检查槽位索引是否有效
            if (fromSlot < inventoryLists[(int)player].Count && toSlot < inventoryLists[(int)player].Count
                && fromSlot != toSlot && fromSlot >= 0 && toSlot >= 0)
            {
                InventoryItem fromItem = inventoryLists[(int)player][fromSlot];
                InventoryItem toItem = inventoryLists[(int)player][toSlot];

                inventoryLists[(int)player][fromSlot] = toItem;
                inventoryLists[(int)player][toSlot] = fromItem;

                EventHandler.CallInventoryUpdatedEvent(player, inventoryLists[(int)player]);
            }

        }

        /// <summary>
        /// 查找物品是否在背包中
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="itemCode">物品代码</param>
        /// <returns>返回物品在背包中的位置 或 -1 表示未找到</returns>
        public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
        {
            List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

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
        /// 获取物品详情
        /// </summary>
        /// <param name="itemCode">物品代码</param>
        /// <returns>物品详情</returns>
        public ItemDetails GetItemDetails(int itemCode)
        {
            _itemDetailsDic.TryGetValue(itemCode, out ItemDetails itemDetails);
            return itemDetails;
        }

        /// <summary>
        /// 获取选中物品详情
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <returns>物品详情</returns>
        public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
        {
            int itemCode = GetSelectedInventoryItem(inventoryLocation);

            return GetItemDetails(itemCode);
        }

        /// <summary>
        /// 获取物品类型描述
        /// </summary>
        /// <param name="itemType">物品类型</param>
        /// <returns>物品类型描述</returns>
        public string GetItemTypeDescription(ItemType itemType)
        {
            string itemTypeDescription = itemType switch
            {
                ItemType.WateringTool => Settings.WateringTool,
                ItemType.HoeingTool => Settings.HoeingTool,
                ItemType.ChoppingTool => Settings.ChoppingTool,
                ItemType.BreakingTool => Settings.BreakingTool,
                ItemType.ReapingTool => Settings.ReapingTool,
                ItemType.CollectingTool => Settings.CollectingTool,
                _ => itemType.ToString()
            };
            return itemTypeDescription;
        }

        /// <summary>
        /// 设置选中的背包物品
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="itemCode">物品代码</param>
        public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
        {
            selectedInventoryItem[(int)inventoryLocation] = itemCode;
        }

        /// <summary>
        /// 清空选中的背包物品
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
        {
            selectedInventoryItem[(int)inventoryLocation] = -1;
        }
        #endregion

        #region ISaveable Interface Methods
        /// <summary>
        /// 注册可保存对象到保存管理器
        /// </summary>
        public void ISaveableRegister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Add(this);
        }

        /// <summary>
        /// 从保存管理器中注销可保存对象
        /// </summary>
        public void ISaveableDeregister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <returns>游戏对象保存数据</returns>
        public GameObjectSave ISaveableSave()
        {
            _gameObjectSave.sceneData.Remove(Settings.PersistentSceneName);

            SceneSave sceneSave = new()
            {
                listInventoryItems = inventoryLists,
                intArrayDictionary = new Dictionary<string, int[]>()
            };

            sceneSave.intArrayDictionary.Add("inventoryListCapacityIntArray", inventoryListCapacityIntArray);

            _gameObjectSave.sceneData.Add(Settings.PersistentSceneName, sceneSave);

            return _gameObjectSave;
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <param name="gameSave">游戏保存数据</param>
        public void ISaveableLoad(GameSave gameSave)
        {
            if (!gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out var gameObjectSave))
                return;

            _gameObjectSave = gameObjectSave;

            if (!_gameObjectSave.sceneData.TryGetValue(Settings.PersistentSceneName, out var sceneSave))
                return;

            if (sceneSave.listInventoryItems == null)
                return;

            inventoryLists = sceneSave.listInventoryItems;

            // 更新库存 玩家背包 UI
            for (int i = 0; i < (int)InventoryLocation.Count; i++)
            {
                EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
            }

            PlayerUnit.Instance.ClearCarriedItem();

            _uiInventoryBar.ClearHighlightOnInventorySlots();

            if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityIntArray", out int[] inventoryListCapacityIntArray))
            {
                this.inventoryListCapacityIntArray = inventoryListCapacityIntArray;
            }
        }

        public void ISaveableStoreScene(string sceneName)
        {
            // Nothing to store
        }

        public void ISaveableRestoreScene(string sceneName)
        {
            // Nothing to restore
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 创建背包列表
        /// </summary>
        private void CreateInventoryLists()
        {
            inventoryLists = new List<InventoryItem>[(int)InventoryLocation.Count];

            for (int i = 0; i < (int)InventoryLocation.Count; i++)
            {
                inventoryLists[i] = new List<InventoryItem>();
            }

            inventoryListCapacityIntArray = new int[(int)InventoryLocation.Count];
            inventoryListCapacityIntArray[(int)InventoryLocation.Player] = Settings.playerInitialInventoryCapacity;
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
        /// 添加物品到背包指定位置 计数+1
        /// </summary>
        /// <param name="inventoryList">背包列表</param>
        /// <param name="itemPosition">物品位置</param>
        private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemPosition)
        {
            InventoryItem inventoryItem = inventoryList[itemPosition];
            inventoryItem.itemQuantity++;
            inventoryList[itemPosition] = inventoryItem;
        }

        /// <summary>
        /// 添加新物品到背包末尾
        /// </summary>
        /// <param name="inventoryList">背包列表</param>
        /// <param name="itemCode">物品代码</param>
        private void AddNewItemToInventory(List<InventoryItem> inventoryList, int itemCode)
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
        /// <param name="inventoryList">背包列表</param>
        /// <param name="itemCode">物品代码</param>
        /// <param name="itemPosition">物品位置</param>
        private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemPosition)
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
        /// 获取选中物品
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <returns>物品代码</returns>
        private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
        {
            return selectedInventoryItem[(int)inventoryLocation];
        }
        #endregion
    }
}