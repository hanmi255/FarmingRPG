using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.Map;
using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class GridCursorHighlight : MonoBehaviour
    {
        private Canvas _canvas;
        private Grid _grid;
        private Camera _mainCamera;
        [SerializeField] private Image _cursorImage = null;
        [SerializeField] private RectTransform _cursorRectTransform = null;
        [SerializeField] private Sprite _greenCursor = null;
        [SerializeField] private Sprite _redCursor = null;

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

        public void EnableCursor()
        {
            _cursorImage.color = Color.white;
            _cursorEnabled = true;
        }

        public void DisableCursor()
        {
            _cursorImage.color = Color.clear;
            _cursorEnabled = false;
        }

        private void AfterSceneLoad()
        {
            _grid = FindObjectOfType<Grid>();
        }

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

        private Vector3Int GetGridPositionForCursor()
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));

            return _grid.WorldToCell(worldPosition);
        }

        private Vector3Int GetGridPositionForPlayer()
        {
            return _grid.WorldToCell(PlayerUnit.Instance.transform.position);
        }

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
                    if (!IsCursorValidToDropItem(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Commodity:
                    if (!IsCursorValidToDropItem(gridPropertyDetails))
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

        private Vector3 GetRectTransformPositionForCursor(Vector3Int gridPosition)
        {
            Vector3 gridWorldPosition = _grid.CellToWorld(gridPosition);
            Vector2 gridScreenPosition = _mainCamera.WorldToScreenPoint(gridWorldPosition);

            return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, _cursorRectTransform, _canvas);
        }

        private void SetCursorToValid()
        {
            _cursorImage.sprite = _greenCursor;
            CursorPositionIsValid = true;
        }

        private void SetCursorToInvalid()
        {
            _cursorImage.sprite = _redCursor;
            CursorPositionIsValid = false;
        }

        private bool IsCursorValidToDropItem(GridPropertyDetails gridPropertyDetails)
        {
            return gridPropertyDetails.canDropItem;
        }
    }
}