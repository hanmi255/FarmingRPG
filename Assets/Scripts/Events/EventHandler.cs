public struct MovementParameters
{
    // 基本移动输入
    public float inputX;
    public float inputY;
    public bool isWalking;
    public bool isRunning;
    public bool isIdle;
    public bool isCarrying;
    public ToolEffect toolEffect;

    // 工具使用状态
    public bool isUsingToolRight;
    public bool isUsingToolLeft;
    public bool isUsingToolUp;
    public bool isUsingToolDown;

    // 工具抬起状态
    public bool isLiftingToolRight;
    public bool isLiftingToolLeft;
    public bool isLiftingToolUp;
    public bool isLiftingToolDown;

    // 拾取状态
    public bool isPickingRight;
    public bool isPickingLeft;
    public bool isPickingUp;
    public bool isPickingDown;

    // 工具挥动状态
    public bool isSwingingToolRight;
    public bool isSwingingToolLeft;
    public bool isSwingingToolUp;
    public bool isSwingingToolDown;

    // 空闲方向状态
    public bool isIdleUp;
    public bool isIdleDown;
    public bool isIdleLeft;
    public bool isIdleRight;
}

public delegate void MovementDelegate(MovementParameters movementParams);

public static class EventHandler
{
    public static event MovementDelegate MovementEvent;

    public static void CallMovementEvent(MovementParameters movementParams)
    {
        MovementEvent?.Invoke(movementParams);
    }
}