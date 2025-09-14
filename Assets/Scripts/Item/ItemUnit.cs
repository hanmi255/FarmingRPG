using Assets.Scripts.Inventory;
using Assets.Scripts.Utilities.PropertyDrawers;
using UnityEngine;

namespace Assets.Scripts.Item
{
    /// <summary>
    /// 物品单位类，用于表示场景中的物品对象
    /// </summary>
    public class ItemUnit : MonoBehaviour
    {
        #region Fields

        [ItemCodeDescription]
        [SerializeField] 
        private int _itemCode;
        private SpriteRenderer _spriteRenderer;

        #endregion

        #region Properties

        public int ItemCode
        {
            get { return _itemCode; }
            set { _itemCode = value; }
        }

        #endregion

        #region Lifecycle Methods

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            // 如果物品代码不为0，则初始化物品
            if (ItemCode != 0)
            {
                Init(ItemCode);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化物品单位
        /// </summary>
        /// <param name="itemCode">要初始化的物品代码</param>
        public void Init(int itemCode)
        {
            // 确保物品代码有效
            if (itemCode != 0)
            {
                ItemCode = itemCode;
                ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);

                // 设置物品精灵
                _spriteRenderer.sprite = itemDetails.itemSprite;

                // 如果是可收割的场景物品，添加扰动效果组件
                if (itemDetails.itemType == Enums.ItemType.ReapableScenary)
                {
                    gameObject.AddComponent<ItemNudge>();
                }
            }
        }

        #endregion
    }
}