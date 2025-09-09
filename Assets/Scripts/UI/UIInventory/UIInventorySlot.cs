using Assets.Scripts.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.UIInventory
{
    public class UIInventorySlot : MonoBehaviour
    {
        [SerializeField] private Image _inventorySlotHighlight;
        public Image inventorySlotImage;
        public TextMeshProUGUI textMeshProUGUI;

        [HideInInspector] public ItemDetails itemDetails;
        [HideInInspector] public int itemQuantity;
    }
}