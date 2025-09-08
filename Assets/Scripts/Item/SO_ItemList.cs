using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Item
{
    [CreateAssetMenu(fileName = "so_ItemList", menuName = "ScriptableObjects/Item/ItemList")]
    public class SO_ItemList : ScriptableObject
    {
        public List<ItemDetails> itemDetails;  // 物品详情列表
    }
}