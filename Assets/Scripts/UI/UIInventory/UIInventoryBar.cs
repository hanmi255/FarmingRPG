using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.UI.UIInventory
{
    [RequireComponent(typeof(RectTransform))]
    public class UIInventoryBar : MonoBehaviour
    {
        [SerializeField] private Sprite _blank16x16Sprite = null;
        [SerializeField] private UIInventorySlot[] _inventorySlots = null;
        public GameObject inventoryDraggedItem;
        [HideInInspector] public GameObject inventoryTextBoxGameobject;
        private RectTransform _rectTransform;
        private bool _isInventoryBarPositionAtBottom = true;
        public bool IsInventoryBarPositionAtBottom
        {
            get => _isInventoryBarPositionAtBottom;
            set => _isInventoryBarPositionAtBottom = value;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            EventHandler.InventoryUpdatedEvent += UpdateInventoryBar;
        }

        private void OnDisable()
        {
            EventHandler.InventoryUpdatedEvent -= UpdateInventoryBar;
        }

        private void Update()
        {
            SwitchInventoryBarPosition();
        }

        /// <summary>
        /// 更新物品栏显示
        /// </summary>
        private void UpdateInventoryBar(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
        {
            // 只更新玩家物品栏
            if (inventoryLocation != InventoryLocation.Player) return;

            // 先清空物品栏显示
            ClearInventorySlots();

            // 检查物品栏插槽和物品列表是否有效
            if (_inventorySlots.Length == 0 || inventoryList.Count == 0) return;

            // 遍历物品列表，更新物品栏显示
            for (int i = 0; i < _inventorySlots.Length && i < inventoryList.Count; i++)
            {
                int itemCode = inventoryList[i].itemCode;

                // 获取物品详细信息
                ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                if (itemDetails != null)
                {
                    // 更新物品栏插槽显示
                    _inventorySlots[i].inventorySlotImage.sprite = itemDetails.itemSprite;
                    _inventorySlots[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                    _inventorySlots[i].itemDetails = itemDetails;
                    _inventorySlots[i].itemQuantity = inventoryList[i].itemQuantity;
                    SetHighlightSlot(_inventorySlots[i]);
                }
            }
        }

        /// <summary>
        /// 清空物品栏显示
        /// </summary>
        private void ClearInventorySlots()
        {
            // 遍历所有物品栏插槽，重置为初始状态
            foreach (var slot in _inventorySlots)
            {
                slot.inventorySlotImage.sprite = _blank16x16Sprite;
                slot.textMeshProUGUI.text = "";
                slot.itemDetails = null;
                slot.itemQuantity = 0;
                SetHighlightSlot(slot);
            }
        }

        private void SwitchInventoryBarPosition()
        {
            Vector3 playerViewportPosition = PlayerUnit.Instance.GetPlayerViewporPosition();
            bool shouldPositionAtBottom = playerViewportPosition.y > 0.3f;

            // 只有当位置需要改变时才执行变换操作
            if (shouldPositionAtBottom && !IsInventoryBarPositionAtBottom)
            {
                SetInventoryBarToBottom();
            }
            else if (!shouldPositionAtBottom && IsInventoryBarPositionAtBottom)
            {
                SetInventoryBarToTop();
            }
        }

        private void SetInventoryBarToBottom()
        {
            _rectTransform.pivot = new Vector2(0.5f, 0f);
            _rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionAtBottom = true;
        }

        private void SetInventoryBarToTop()
        {
            _rectTransform.pivot = new Vector2(0.5f, 1f);
            _rectTransform.anchorMin = new Vector2(0.5f, 1f);
            _rectTransform.anchorMax = new Vector2(0.5f, 1f);
            _rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionAtBottom = false;
        }

        /// <summary>
        /// 设置高亮显示
        /// </summary>
        public void SetHighlightOnInventorySlots()
        {
            if (_inventorySlots == null || _inventorySlots.Length == 0)
                return;

            foreach (var slot in _inventorySlots)
            {
                SetHighlightSlot(slot);
            }
        }

        private void SetHighlightSlot(UIInventorySlot slot)
        {
            if (slot.isSelected && slot.itemDetails != null)
            {
                slot.inventorySlotHighlight.color = Color.white;
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.Player, slot.itemDetails.itemCode);
            }
        }

        /// <summary>
        /// 清除高亮显示
        /// </summary>
        public void ClearHighlightOnInventorySlots()
        {
            if (_inventorySlots == null || _inventorySlots.Length == 0)
                return;

            foreach (var slot in _inventorySlots)
            {
                if (slot.isSelected)
                {
                    slot.isSelected = false;
                    slot.inventorySlotHighlight.color = Color.clear;
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.Player);
                }
            }
        }
    }
}