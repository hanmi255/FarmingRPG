namespace Assets.Scripts.Enums
{
    public enum AnimationName  // 动画名称
    {
        // 空闲动画
        IdleUp,
        IdleDown,
        IdleLeft,
        IdleRight,

        // 行走动画
        WalkUp,
        WalkDown,
        WalkLeft,
        WalkRight,

        // 奔跑动画
        RunUp,
        RunDown,
        RunLeft,
        RunRight,

        // 使用工具动画
        UseToolUp,
        UseToolDown,
        UseToolLeft,
        UseToolRight,

        // 挥动工具动画
        SwingToolUp,
        SwingToolDown,
        SwingToolLeft,
        SwingToolRight,

        // 抬起工具动画
        LiftToolUp,
        LiftToolDown,
        LiftToolLeft,
        LiftToolRight,

        // 握持工具动画
        HoldToolUp,
        HoldToolDown,
        HoldToolLeft,
        HoldToolRight,

        // 拾取物品动画
        PickUp,
        PickDown,
        PickLeft,
        PickRight,
        Count
    }

    public enum CharacterPartAnimator
    {
        Hat,
        Hair,
        Body,
        Arms,
        Tool,
        Count
    }

    public enum PartVariantColour
    {
        None,
        Count
    }

    public enum PartVariantType
    {
        None,
        Carry,
        Hoe,
        Pickaxe,
        Axe,
        Scythe,
        WateringCan,
        Count
    }

    public enum InventoryLocation  // 背包位置
    {
        Player,
        Chest,
        Count
    }

    public enum Season  // 季节
    {
        None,
        Spring,
        Summer,
        Autumn,
        Winter,
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