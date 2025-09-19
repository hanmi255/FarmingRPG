using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.HelperClasses;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.Scripts.UI
{
    public class CursorHighlight : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image _cursorImage = null;
        [SerializeField] private RectTransform _cursorRectTransform = null;
        [SerializeField] private Sprite _greenCursor = null;
        [SerializeField] private Sprite _transparentCursor = null;
        [SerializeField] private GridCursorHighlight _gridCursorHighlight = null;
        private Canvas _canvas;
        private Camera _mainCamera;

        private bool _cursorPositionIsValid = false;
        public bool CursorPositionIsValid
        {
            get => _cursorPositionIsValid;
            set => _cursorPositionIsValid = value;
        }

        private float _itemUseRadius = 0;
        public float ItemUseRadius
        {
            get => _itemUseRadius;
            set 
            {
                _itemUseRadius = value;
                // 同时更新缓存的一半值，避免重复计算
                _itemUseRadiusHalfValue = value * 0.5f;
            }
        }
        
        // 缓存ItemUseRadius的一半值，避免重复计算
        private float _itemUseRadiusHalfValue = 0f;

        private ItemType _selectedItemType;
        public ItemType SelectedItemType
        {
            get => _selectedItemType;
            set => _selectedItemType = value;
        }

        private bool _cursorEnabled = false;
        public bool CursorEnabled
        {
            get => _cursorEnabled;
            set => _cursorEnabled = value;
        }

        #endregion

        #region Lifecycle Methods
        private void Start()
        {
            _mainCamera = Camera.main;
            _canvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (!_cursorEnabled)
                return;

            DisplayCursor();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// 启用光标显示
        /// </summary>
        public void EnableCursor()
        {
            _cursorImage.color = Color.white;
            _cursorEnabled = true;
        }

        /// <summary>
        /// 禁用光标显示
        /// </summary>
        public void DisableCursor()
        {
            _cursorImage.color = Color.clear;
            _cursorEnabled = false;
        }

        public Vector3 GetWorldPositionForCursor()
        {
            Vector3 screenPosition = new(Input.mousePosition.x, Input.mousePosition.y, 0f);

            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

            return worldPosition;
        }

        public Vector2 GetRectTransformPositionForCursor()
        {
            Vector2 screenPosition = new(Input.mousePosition.x, Input.mousePosition.y);

            return RectTransformUtility.PixelAdjustPoint(screenPosition, _cursorRectTransform, _canvas);
        }
        #endregion

        #region Private Methods
        private void DisplayCursor()
        {
            Vector3 cursorWorldPosition = GetWorldPositionForCursor();

            Vector3 playerCenterPosition = PlayerUnit.Instance.GetPlayerCenterPosition();

            SetCursorValidity(cursorWorldPosition, playerCenterPosition);

            _cursorRectTransform.position = GetRectTransformPositionForCursor();
        }

        /// <summary>
        /// 设置光标有效性
        /// </summary>
        /// <param name="cursorWorldPosition">光标世界坐标</param>
        /// <param name="playerCenterPosition">玩家中心坐标</param>
        private void SetCursorValidity(Vector3 cursorWorldPosition, Vector3 playerCenterPosition)
        {
            SetCursorToValid();

            // 检查物品使用范围角落
            if (cursorWorldPosition.x > (playerCenterPosition.x + _itemUseRadiusHalfValue) && cursorWorldPosition.y > (playerCenterPosition.y + _itemUseRadiusHalfValue)
                || cursorWorldPosition.x < (playerCenterPosition.x - _itemUseRadiusHalfValue) && cursorWorldPosition.y > (playerCenterPosition.y + _itemUseRadiusHalfValue)
                || cursorWorldPosition.x < (playerCenterPosition.x - _itemUseRadiusHalfValue) && cursorWorldPosition.y < (playerCenterPosition.y - _itemUseRadiusHalfValue)
                || cursorWorldPosition.x > (playerCenterPosition.x + _itemUseRadiusHalfValue) && cursorWorldPosition.y < (playerCenterPosition.y - _itemUseRadiusHalfValue))
            {
                SetCursorToInvalid();
                return;
            }

            // 检查物品使用范围
            if (Mathf.Abs(cursorWorldPosition.x - playerCenterPosition.x) > _itemUseRadius
                || Mathf.Abs(cursorWorldPosition.y - playerCenterPosition.y) > _itemUseRadius)
            {
                SetCursorToInvalid();
                return;
            }

            // 获取选中物品的详细信息
            ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.Player);
            if (itemDetails == null)
            {
                SetCursorToInvalid();
                return;
            }

            // 检查工具类型的有效性
            switch (itemDetails.itemType)
            {
                case ItemType.WateringTool:
                case ItemType.HoeingTool:
                case ItemType.ChoppingTool:
                case ItemType.BreakingTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                    if (!SetCursorValidityTool(cursorWorldPosition, playerCenterPosition, itemDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.None:
                case ItemType.Count:
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置光标为有效状态（绿色）
        /// </summary>
        private void SetCursorToValid()
        {
            _cursorImage.sprite = _greenCursor;
            CursorPositionIsValid = true;

            _gridCursorHighlight.DisableCursor();
        }

        /// <summary>
        /// 设置光标为无效状态（透明）
        /// </summary>
        private void SetCursorToInvalid()
        {
            _cursorImage.sprite = _transparentCursor;
            CursorPositionIsValid = false;

            _gridCursorHighlight.EnableCursor();
        }

        /// <summary>
        /// 设置光标有效性
        /// </summary>
        /// <param name="cursorWorldPosition"></param>
        /// <param name="playerCenterPosition"></param>
        /// <param name="itemDetails"></param>
        /// <returns></returns>
        private bool SetCursorValidityTool(Vector3 cursorWorldPosition, Vector3 playerCenterPosition, ItemDetails itemDetails)
        {
            return itemDetails.itemType switch
            {
                ItemType.ReapingTool => SetCursorValidityReapingTool(cursorWorldPosition, playerCenterPosition, itemDetails),
                _ => false,
            };
        }

        /// <summary>
        /// 设置光标有效性（针对ReapingTool）
        /// </summary>
        /// <param name="cursorWorldPosition"></param>
        /// <param name="playerCenterPosition"></param>
        /// <param name="itemDetails"></param>
        /// <returns></returns>
        private bool SetCursorValidityReapingTool(Vector3 cursorWorldPosition, Vector3 playerCenterPosition, ItemDetails itemDetails)
        {
            if (!HelperMethods.GetComponentsAtCursorLocation(out List<ItemUnit> itemUnits, cursorWorldPosition))
                return false;

            foreach (var itemUnit in itemUnits)
            {
                ItemDetails details = InventoryManager.Instance.GetItemDetails(itemUnit.ItemCode);
                if (details != null && details.itemType == ItemType.ReapableScenary)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}