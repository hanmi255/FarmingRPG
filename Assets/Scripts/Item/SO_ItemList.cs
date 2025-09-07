using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Item
{
    [CreateAssetMenu(fileName = "so_ItemList", menuName = "ScriptableObjects/Item/ItemList")]
    public class SO_ItemList : ScriptableObject
    {
        [SerializeField]
        public List<ItemDetails> itemDetails;
    }
}