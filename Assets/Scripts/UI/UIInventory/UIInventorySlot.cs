using Assets.Scripts.Enums;
using Assets.Scripts.Events;
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
    /// <summary>
    /// UI物品栏插槽类 - 负责处理物品栏中单个插槽的显示和交互逻辑
    /// </summary>
    public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Fields

        [SerializeField] private UIInventoryBar _inventoryBar = null;
        [SerializeField] private GameObject _inventoryTextBoxPrefab = null;
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private int _slotNumber = 0;

        public Image inventorySlotHighlight;
        public Image inventorySlotImage;
        public TextMeshProUGUI textMeshProUGUI;

        [HideInInspector] public bool isSelected = false;
        [HideInInspector] public ItemDetails itemDetails;
        [HideInInspector] public int itemQuantity;

        private Camera _mainCamera;
        private Canvas _parentCanvas;
        private Transform _parentItem;
        private GridCursorHighlight _gridCursorHighlight;
        private CursorHighlight _cursorHighlight;
        public GameObject draggedItem;

        private TextParameters _textParameters;
        private string _itemTypeDescription;

        #endregion

        #region Lifecycle Methods

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
            EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
            EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventory;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
            EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
            EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventory;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _gridCursorHighlight = FindObjectOfType<GridCursorHighlight>();
            _cursorHighlight = FindObjectOfType<CursorHighlight>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 开始拖拽操作
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemDetails == null)
                return;

            // 禁止玩家输入移动
            PlayerUnit.Instance.DisablePlayerInputAndResetMovement();
            // 实例化拖拽物品
            draggedItem = Instantiate(_inventoryBar.inventoryDraggedItem, _inventoryBar.transform);
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = itemDetails.itemSprite;

            SetSelectedItem();
        }

        /// <summary>
        /// 拖拽过程中更新物品位置
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (draggedItem == null)
                return;

            draggedItem.transform.position = Input.mousePosition;
        }

        /// <summary>
        /// 结束拖拽操作
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggedItem == null)
                return;

            Destroy(draggedItem);

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

        /// <summary>
        /// 鼠标指针进入插槽区域
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
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

        /// <summary>
        /// 鼠标指针离开插槽区域
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            DestoryInventoryTextBox();
        }

        /// <summary>
        /// 鼠标点击插槽
        /// </summary>
        /// <param name="eventData">指针事件数据</param>
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

        /// <summary>
        /// 清除选中物品
        /// </summary>
        public void ClearSelectedItem()
        {
            ClearCusors();

            _inventoryBar.ClearHighlightOnInventorySlots();
            isSelected = false;
            InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.Player);

            PlayerUnit.Instance.ClearCarriedItem();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 设置文本参数
        /// </summary>
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

        /// <summary>
        /// 在鼠标位置放置选中的物品
        /// </summary>
        private void DropSelectedItemAtMousePosition()
        {
            // 只有当选中的物品是当前槽位的物品时才执行放置操作
            if (itemDetails == null || !isSelected)
                return;

            // 如果光标位置无效，则返回
            // 否则放置物品
            if (!_gridCursorHighlight.CursorPositionIsValid)
                return;

            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));
            Vector3 dropPosition = new(worldPosition.x, worldPosition.y - Settings.halfGridCellSize, 0);

            GameObject itemGameObject = Instantiate(_itemPrefab, dropPosition, Quaternion.identity, _parentItem);
            ItemUnit item = itemGameObject.GetComponent<ItemUnit>();
            item.ItemCode = itemDetails.itemCode;

            InventoryManager.Instance.RemoveItem(InventoryLocation.Player, item.ItemCode);

            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.Player, item.ItemCode) == -1)
            {
                ClearSelectedItem();
            }
        }

        /// <summary>
        /// 从背包中移除选中的道具
        /// </summary>
        private void RemoveSelectedItemFromInventory()
        {
            if (itemDetails == null || !isSelected)
                return;

            int itemCode = itemDetails.itemCode;

            InventoryManager.Instance.RemoveItem(InventoryLocation.Player, itemCode);

            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.Player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }

        /// <summary>
        /// 销毁物品信息文本框
        /// </summary>
        private void DestoryInventoryTextBox()
        {
            if (_inventoryBar.inventoryTextBoxGameobject == null)
                return;

            Destroy(_inventoryBar.inventoryTextBoxGameobject);
        }

        /// <summary>
        /// 设置选中物品
        /// </summary>
        private void SetSelectedItem()
        {
            _inventoryBar.ClearHighlightOnInventorySlots();
            isSelected = true;
            _inventoryBar.SetHighlightOnInventorySlots();

            // 设置光标高亮
            _gridCursorHighlight.ItemUseGridRadius = itemDetails.itemUseGridRadius;
            _cursorHighlight.ItemUseRadius = itemDetails.itemUseRadius;

            // 设置是否启用光标高亮
            if (itemDetails.itemUseGridRadius > 0)
            {
                _gridCursorHighlight.EnableCursor();
            }
            else
            {
                _gridCursorHighlight.DisableCursor();
            }

            if (itemDetails.itemUseRadius > 0f)
            {
                _cursorHighlight.EnableCursor();
            }
            else
            {
                _cursorHighlight.DisableCursor();
            }

            // 设置选中的物品类型
            _gridCursorHighlight.SelectedItemType = itemDetails.itemType;
            _cursorHighlight.SelectedItemType = itemDetails.itemType;

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

        /// <summary>
        /// 清除光标显示
        /// </summary>
        private void ClearCusors()
        {
            _gridCursorHighlight.DisableCursor();
            _gridCursorHighlight.SelectedItemType = ItemType.None;

            _cursorHighlight.DisableCursor();
            _cursorHighlight.SelectedItemType = ItemType.None;
        }

        /// <summary>
        /// 场景加载完成后获取物品父级变换组件
        /// </summary>
        private void AfterSceneLoad()
        {
            _parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
        }

        #endregion
    }
}