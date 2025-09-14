namespace Assets.Scripts.Inventory
{
    /// <summary>
    /// 库存物品结构体，用于表示背包中单个物品的信息
    /// </summary>
    [System.Serializable]
    public struct InventoryItem
    {
        #region Fields
        public int itemCode;
        public int itemQuantity;
        #endregion
    }
}