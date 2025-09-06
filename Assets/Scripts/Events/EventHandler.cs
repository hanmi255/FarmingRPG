using Assets.Scripts.Enums;

namespace Assets.Scripts.Events
{
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
        public bool isUsingToolUp;
        public bool isUsingToolDown;
        public bool isUsingToolLeft;
        public bool isUsingToolRight;

        // 工具抬起状态
        public bool isLiftingToolUp;
        public bool isLiftingToolDown;
        public bool isLiftingToolLeft;
        public bool isLiftingToolRight;

        // 拾取状态
        public bool isPickingUp;
        public bool isPickingDown;
        public bool isPickingLeft;
        public bool isPickingRight;

        // 工具挥动状态
        public bool isSwingingToolUp;
        public bool isSwingingToolDown;
        public bool isSwingingToolLeft;
        public bool isSwingingToolRight;

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
}