using System;
using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Misc;
using Assets.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.UIInventory
{
    public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Camera _mainCamera;
        private Canvas _parentCanvas;
        private Transform _parentItem;
        private GameObject _draggedItem;

        public Image inventorySlotHighlight;
        public Image inventorySlotImage;
        public TextMeshProUGUI textMeshProUGUI;
        [SerializeField] private UIInventoryBar _inventoryBar = null;
        [SerializeField] private GameObject _inventoryTextBoxPrefab = null;
        [HideInInspector] public bool isSelected = false;
        [HideInInspector] public ItemDetails itemDetails;
        [SerializeField] private GameObject _itemPrefab = null;
        [HideInInspector] public int itemQuantity;
        [SerializeField] private int _slotNumber = 0;

        private TextParameters _textParameters;
        private string _itemTypeDescription;

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _parentItem = GameObject.FindWithTag(Tags.ItemsParentTransform).transform;
        }

        private void SetTextParameters()
        {
            _itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            _textParameters = new()
            {
                textTop1 = itemDetails.itemName,
                textTop2 = _itemTypeDescription,
                textTop3 = "",
                textBottom1 = itemDetails.itemDescription,
                textBottom2 = "",
                textBottom3 = "",
            };
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemDetails == null)
                return;

            // 禁止玩家输入移动
            PlayerUnit.Instance.DisablePlayerInputAndResetMovement();
            // 实例化拖拽物品
            _draggedItem = Instantiate(_inventoryBar.inventoryDraggedItem, _inventoryBar.transform);
            Image draggedItemImage = _draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = itemDetails.itemSprite;

            SetSelectedItem();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_draggedItem == null)
                return;

            _draggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_draggedItem == null)
                return;

            Destroy(_draggedItem);

            // 交换物品位置
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>()._slotNumber;

                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.Player, _slotNumber, toSlotNumber);
                DestoryInventoryTextBox();
                ClearSelectedItem();
            }
            // 直接放置物品到地面
            else if (itemDetails != null && itemDetails.canBeDropped)
            {
                DropSelectedItemAtMousePosition();
            }

            PlayerUnit.Instance.EnablePlayerInput();
        }

        private void DropSelectedItemAtMousePosition()
        {
            if (itemDetails == null && !isSelected)
                return;

            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));

            GameObject itemGameObject = Instantiate(_itemPrefab, worldPosition, Quaternion.identity, _parentItem);
            ItemUnit item = itemGameObject.GetComponent<ItemUnit>();
            item.ItemCode = itemDetails.itemCode;

            InventoryManager.Instance.RemoveItem(InventoryLocation.Player, item.ItemCode);

            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.Player, item.ItemCode) == -1)
            {
                ClearSelectedItem();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (itemQuantity == 0)
                return;

            _inventoryBar.inventoryTextBoxGameobject = Instantiate(_inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            _inventoryBar.inventoryTextBoxGameobject.transform.SetParent(_parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = _inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            SetTextParameters();

            inventoryTextBox.SetTextBoxText(_textParameters);

            RectTransform textBoxRectTransform = _inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>();
            Vector3 textBoxPosition = transform.position;

            if (_inventoryBar.IsInventoryBarPositionAtBottom)
            {
                textBoxRectTransform.pivot = new Vector2(0.5f, 0f);
                textBoxPosition.y += 50f;
            }
            else
            {
                textBoxRectTransform.pivot = new Vector2(0.5f, 1f);
                textBoxPosition.y -= 50f;
            }

            _inventoryBar.inventoryTextBoxGameobject.transform.position = textBoxPosition;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DestoryInventoryTextBox();
        }

        private void DestoryInventoryTextBox()
        {
            if (_inventoryBar.inventoryTextBoxGameobject == null)
                return;

            Destroy(_inventoryBar.inventoryTextBoxGameobject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (isSelected)
            {
                ClearSelectedItem();
            }
            else
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }

        private void SetSelectedItem()
        {
            _inventoryBar.ClearHighlightOnInventorySlots();
            isSelected = true;
            _inventoryBar.SetHighlightOnInventorySlots();
            InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.Player, itemDetails.itemCode);

            if (itemDetails.canBeCarried)
            {
                PlayerUnit.Instance.ShowCarriedItem(itemDetails.itemCode);
            }
            else
            {
                PlayerUnit.Instance.ClearCarriedItem();
            }
        }

        private void ClearSelectedItem()
        {
            _inventoryBar.ClearHighlightOnInventorySlots();
            isSelected = false;
            InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.Player);

            PlayerUnit.Instance.ClearCarriedItem();
        }
    }
}