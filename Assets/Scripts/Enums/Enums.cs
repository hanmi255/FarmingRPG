namespace Assets.Scripts.Enums
{
    /// <summary>
    /// 动画名称枚举，定义角色的所有可能动画状态
    /// </summary>
    public enum AnimationName
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

    /// <summary>
    /// 角色部位动画器枚举，定义角色的各个可动画部位
    /// </summary>
    public enum CharacterPartAnimator
    {
        Hat,
        Hair,
        Body,
        Arms,
        Tool,
        Count
    }

    /// <summary>
    /// 部位变体颜色枚举，定义角色部位的颜色变体
    /// </summary>
    public enum PartVariantColour
    {
        None,
        Count
    }

    /// <summary>
    /// 部位变体类型枚举，定义角色部位的类型变体
    /// </summary>
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

    /// <summary>
    /// 网格布尔属性枚举，定义网格单元格的布尔属性类型
    /// </summary>
    public enum GridBoolProperty
    {
        Diggable,
        CanDropItem,
        CanPlaceFurniture,
        IsPath,
        IsNPCObstacle
    }


    /// <summary>
    /// 网格属性详情类型枚举
    /// </summary>
    public enum GridPropertyDetailsType
    {
        Diggable,
        Waterable
    }

    /// <summary>
    /// 背包位置枚举，定义物品存储的位置
    /// </summary>
    public enum InventoryLocation
    {
        Player,
        Chest,

        Count
    }

    /// <summary>
    /// 场景名称枚举，定义游戏中的场景
    /// </summary>
    public enum SceneName
    {
        Farm,
        Field,
        CabinHome
    }

    /// <summary>
    /// 季节枚举，定义游戏中的季节
    /// </summary>
    public enum Season
    {
        None,
        Spring,
        Summer,
        Autumn,
        Winter,

        Count
    }

    /// <summary>
    /// 工具效果枚举，定义工具使用时的效果类型
    /// </summary>
    public enum ToolEffect
    {
        None,
        Watering
    }

    /// <summary>
    /// 收获动作效果枚举，定义收获动作效果类型
    /// </summary>
    public enum HarvestActionEffect
    {
        None,
        DeciduousLeavesFalling,
        PineConesFalling,
        ChoppingTreeTrunk,
        BreakingStone,
        Reaping
    }

    /// <summary>
    /// 方向枚举，定义游戏中的基本方向
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 物品类型枚举，定义游戏中物品的类型分类
    /// </summary>
    public enum ItemType
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