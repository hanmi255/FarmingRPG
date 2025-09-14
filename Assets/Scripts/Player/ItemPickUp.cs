using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;
using Assets.Scripts.Item;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<ItemUnit>(out var item))
            {
                // 获取物品详情
                ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

                // 如果物品可以被拾取则添加物品到背包并销毁物品
                if (itemDetails.canBePickedUp == true)
                {
                    InventoryManager.Instance.AddItem(InventoryLocation.Player, item, collision.gameObject);
                }
            }
        }
    }
}
