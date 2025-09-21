using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Map;
using Assets.Scripts.Player;
using Assets.Scripts.HelperClasses;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Assets.Scripts.Misc;
using Assets.Scripts.Crop;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 网格光标高亮类 - 负责在游戏场景中显示光标位置并验证光标位置的有效性
    /// </summary>
    public class GridCursorHighlight : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image _cursorImage = null;
        [SerializeField] private RectTransform _cursorRectTransform = null;
        [SerializeField] private Sprite _greenCursor = null;
        [SerializeField] private Sprite _redCursor = null;
        [SerializeField] private SO_CropDetailsList _so_cropDetailsList = null;

        private Canvas _canvas;
        private Grid _grid;
        private Camera _mainCamera;

        private bool _cursorPositionIsValid = false;
        public bool CursorPositionIsValid
        {
            get => _cursorPositionIsValid;
            set => _cursorPositionIsValid = value;
        }

        private int _itemUseGridRadius = 0;
        public int ItemUseGridRadius
        {
            get => _itemUseGridRadius;
            set => _itemUseGridRadius = value;
        }

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

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        }

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

        /// <summary>
        /// 获取光标所在的网格位置
        /// </summary>
        /// <returns>光标所在的网格坐标</returns>
        public Vector3Int GetGridPositionForCursor()
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));

            return _grid.WorldToCell(worldPosition);
        }

        /// <summary>
        /// 获取玩家所在的网格位置
        /// </summary>
        /// <returns>玩家所在的网格坐标</returns>
        public Vector3Int GetGridPositionForPlayer()
        {
            return _grid.WorldToCell(PlayerUnit.Instance.transform.position);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 场景加载完成后获取网格组件
        /// </summary>
        private void AfterSceneLoad()
        {
            _grid = FindObjectOfType<Grid>();
        }

        /// <summary>
        /// 显示光标并验证光标位置的有效性
        /// </summary>
        /// <returns>光标所在的网格坐标</returns>
        private Vector3Int DisplayCursor()
        {
            if (_grid == null)
                return Vector3Int.zero;

            Vector3Int cursorGridPosition = GetGridPositionForCursor();

            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            SetCursorValidity(cursorGridPosition, playerGridPosition);

            _cursorRectTransform.position = GetRectTransformPositionForCursor(cursorGridPosition);

            return cursorGridPosition;
        }

        /// <summary>
        /// 设置光标有效性 - 根据物品类型和网格属性验证光标位置是否有效
        /// </summary>
        /// <param name="cursorGridPosition">光标网格位置</param>
        /// <param name="playerGridPosition">玩家网格位置</param>
        private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
        {
            SetCursorToValid();

            // 检查物品使用范围
            if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
                || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
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

            // 获取光标所在位置的网格信息
            GridPropertyDetails gridPropertyDetails = GridPropertyManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);
            if (gridPropertyDetails == null)
            {
                SetCursorToInvalid();
                return;
            }

            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                case ItemType.Commodity:
                    if (!IsCursorValidToDropItem(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.WateringTool:
                case ItemType.HoeingTool:
                case ItemType.ChoppingTool:
                case ItemType.BreakingTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                    if (!IsCursorValidToUseTool(gridPropertyDetails, itemDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.None:
                    break;
                case ItemType.Count:
                    break;
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
        }

        /// <summary>
        /// 设置光标为无效状态（红色）
        /// </summary>
        private void SetCursorToInvalid()
        {
            _cursorImage.sprite = _redCursor;
            CursorPositionIsValid = false;
        }

        /// <summary>
        /// 检查光标位置是否可以放置物品
        /// </summary>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        /// <returns>是否可以放置物品</returns>
        private bool IsCursorValidToDropItem(GridPropertyDetails gridPropertyDetails)
        {
            return gridPropertyDetails.canDropItem;
        }

        /// <summary>
        /// 检查光标位置是否可以使用工具
        /// </summary>
        /// <param name="gridPropertyDetails">网格属性详情</param>
        /// <param name="itemDetails">物品详情</param>
        /// <returns>是否可以使用工具</returns>
        private bool IsCursorValidToUseTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.HoeingTool:
                    // 检查地块是否可挖掘且未被挖掘过
                    if (!gridPropertyDetails.isDiggable || gridPropertyDetails.daysSinceLastDig != -1)
                        return false;

                    Vector3 cursorWorldPosition = GetWorldPositionForCursor();
                    cursorWorldPosition = new Vector3(cursorWorldPosition.x + 0.5f, cursorWorldPosition.y + 0.5f, 0f);
                    HelperMethods.GetComponentsAtBoxLocation(out List<ItemUnit> itemList, cursorWorldPosition, Settings.cursorSize, 0f);

                    // 检查区域内是否有可收获的物品
                    foreach (ItemUnit itemUnit in itemList)
                    {
                        ItemDetails itemDetail = InventoryManager.Instance.GetItemDetails(itemUnit.ItemCode);
                        if (itemDetail != null && itemDetail.itemType == ItemType.ReapableScenary)
                        {
                            return false;
                        }
                    }

                    return true;

                case ItemType.WateringTool:
                    return gridPropertyDetails.daysSinceLastDig > -1 && gridPropertyDetails.daysSinceLastWater == -1;

                case ItemType.ChoppingTool:
                case ItemType.CollectingTool:
                case ItemType.BreakingTool:
                    if (gridPropertyDetails.seedItemCode == -1)
                        return false;

                    CropDetails cropDetails = _so_cropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

                    if (cropDetails == null)
                        return false;

                    if (gridPropertyDetails.growthDays < cropDetails.growthDays[^1])
                        return false;

                    return cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取光标的屏幕世界坐标
        /// </summary>
        /// <returns>光标的屏幕世界坐标</returns>
        private Vector3 GetWorldPositionForCursor()
        {
            return _grid.CellToWorld(GetGridPositionForCursor());
        }

        /// <summary>
        /// 获取光标在屏幕上的RectTransform位置
        /// </summary>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>光标在屏幕上的位置</returns>
        private Vector3 GetRectTransformPositionForCursor(Vector3Int gridPosition)
        {
            Vector3 gridWorldPosition = _grid.CellToWorld(gridPosition);
            Vector2 gridScreenPosition = _mainCamera.WorldToScreenPoint(gridWorldPosition);

            return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, _cursorRectTransform, _canvas);
        }
        #endregion
    }
}