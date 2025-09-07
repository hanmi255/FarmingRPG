using UnityEngine;
using Assets.Scripts.Item;
using Assets.Scripts.Inventory;

namespace Assets.Scripts.Player
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.TryGetComponent<ItemUnit>(out var unit))
            {
                // 获取物品详情
                ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(unit.ItemCode);

                Debug.Log("物品详情：" + itemDetails.itemName);
            }
        }
    }
}
