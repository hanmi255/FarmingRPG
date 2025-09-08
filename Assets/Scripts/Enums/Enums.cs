namespace Assets.Scripts.Enums
{
    public enum InventoryLocation  // 背包位置
    {
        Player,
        Chest,
        Count
    }

    public enum ToolEffect  // 工具效果
    {
        None,
        Watering
    }

    public enum Direction  // 方向
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum ItemType  // 物品类型
    {
        None,
        Seed,
        Commodity,
        WateringTool,
        HoeingTool,
        ChoppingTool,
        BreakingTool,
        ReapingTool,
        CollectingTool,
        ReapableScenary,
        Furniture,
        Count
    }
}