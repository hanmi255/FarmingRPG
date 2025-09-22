using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.UI.UIPauseMenu
{
    public class PauseMenuInventoryManagement : MonoBehaviour
    {
        #region Fields
        [SerializeField] private PauseMenuInventoryManagementSlot[] inventoryManagementSlots = null;
        [SerializeField] private Sprite _transparentSprite = null;
        public GameObject inventoryManagementDraggedItemPrefab;
        [HideInInspector] public GameObject inventoryTextBoxGameObject;
        #endregion

        #region Lifecycle Methods
        private void OnEnable()
        {
            EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

            if (InventoryManager.Instance != null)
            {
                PopulatePlayerInventory(InventoryLocation.Player, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.Player]);
            }
        }

        private void OnDisable()
        {
            EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;

            DestroyInventoryTextBoxGameObject();
        }
        #endregion

        #region Public Methods
        public void DestroyInventoryTextBoxGameObject()
        {
            if (inventoryTextBoxGameObject != null)
            {
                Destroy(inventoryTextBoxGameObject);
            }
        }

        /// <summary>
        /// 销毁当前所有正在拖拽的物品
        /// </summary>
        public void DestroyCurrentlyDraggedItems()
        {
            if (inventoryManagementSlots == null) return;

            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.Player].Count; i++)
            {
                if (inventoryManagementSlots[i]?.draggedItem != null)
                {
                    Destroy(inventoryManagementSlots[i].draggedItem);
                }
            }
        }
        #endregion

        #region Private Methods
        private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
        {
            if (inventoryLocation != InventoryLocation.Player)
                return;

            InitializeInventoryManagementSlots();

            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.Player].Count; i++)
            {
                inventoryManagementSlots[i].itemDetails = InventoryManager.Instance.GetItemDetails(inventoryList[i].itemCode);
                inventoryManagementSlots[i].itemQuantity = inventoryList[i].itemQuantity;

                if (inventoryManagementSlots[i].itemDetails == null)
                    continue;

                inventoryManagementSlots[i].inventoryManagementSlotImage.sprite = inventoryManagementSlots[i].itemDetails.itemSprite;
                inventoryManagementSlots[i].textMeshProUGUI.text = inventoryManagementSlots[i].itemQuantity.ToString();
            }
        }

        private void InitializeInventoryManagementSlots()
        {
            for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)
            {
                inventoryManagementSlots[i].greyedOutImage.SetActive(false);
                inventoryManagementSlots[i].itemDetails = null;
                inventoryManagementSlots[i].itemQuantity = 0;
                inventoryManagementSlots[i].inventoryManagementSlotImage.sprite = _transparentSprite;
                inventoryManagementSlots[i].textMeshProUGUI.text = "";
            }

            for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.Player]; i < Settings.playerMaximumInventoryCapacity; i++)
            {
                inventoryManagementSlots[i].greyedOutImage.SetActive(true);
            }
        }
    }

    #endregion
}