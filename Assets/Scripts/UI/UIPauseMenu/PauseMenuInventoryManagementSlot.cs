using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using Assets.Scripts.UI.UIInventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.UIPauseMenu
{
    public class PauseMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        public Image inventoryManagementSlotImage;
        public TextMeshProUGUI textMeshProUGUI;
        public GameObject greyedOutImage;

        [SerializeField] private PauseMenuInventoryManagement _inventoryManagement = null;
        [SerializeField] private GameObject _inventoryTextBoxPrefab = null;

        [HideInInspector] public ItemDetails itemDetails;
        [HideInInspector] public int itemQuantity;
        [SerializeField] private int slotIndex;

        // private Vector3 _startingPosition;
        public GameObject draggedItem;
        private Canvas _parentCanvas;

        private TextParameters _textParameters;
        private string _itemTypeDescription;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemQuantity == 0)
                return;

            draggedItem = Instantiate(_inventoryManagement.inventoryManagementDraggedItemPrefab, _inventoryManagement.transform);

            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggedItem == null)
                return;

            draggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggedItem == null)
                return;

            Destroy(draggedItem);

            if (eventData.pointerCurrentRaycast.gameObject != null
                && eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null)
            {
                int toSlotIndex = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotIndex;

                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.Player, slotIndex, toSlotIndex);

                _inventoryManagement.DestroyInventoryTextBoxGameObject();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (itemQuantity == 0)
                return;

            _inventoryManagement.inventoryTextBoxGameObject = Instantiate(_inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            _inventoryManagement.inventoryTextBoxGameObject.transform.SetParent(_parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = _inventoryManagement.inventoryTextBoxGameObject.GetComponent<UIInventoryTextBox>();

            SetTextParameters();

            inventoryTextBox.SetTextBoxText(_textParameters);

            RectTransform textBoxRectTransform = _inventoryManagement.inventoryTextBoxGameObject.GetComponent<RectTransform>();
            Vector3 textBoxPosition = transform.position;

            if (slotIndex > 23)
            {
                textBoxRectTransform.pivot = new Vector2(0.5f, 0f);
                textBoxPosition.y += 50f;
            }
            else
            {
                textBoxRectTransform.pivot = new Vector2(0.5f, 1f);
                textBoxPosition.y -= 50f;
            }

            _inventoryManagement.inventoryTextBoxGameObject.transform.position = textBoxPosition;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _inventoryManagement.DestroyInventoryTextBoxGameObject();
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
        #endregion
    }
}