using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Misc
{
    public static class Settings
    {
        // 玩家与物体接触时暗淡
        public const float fadeInSeconds = 0.25f;
        public const float fadeOutSeconds = 0.35f;
        public const float targetAlpha = 0.45f;

        // 瓦片地图
        public const float gridCellSize = 1.0f;
        public static Vector2 cursorSize = Vector2.one;

        // 玩家数据
        public const float walkingSpeed = 2.666f;
        public const float runningSpeed = 5.333f;
        public static float useToolAnimationPause = 0.25f;
        public static float afterUseToolAnimationPause = 0.2f;

        // 背包容量
        public static int playerInitialInventoryCapacity = 24;
        public static int playerMaximumInventoryCapacity = 48;


        // 基本移动输入
        public static int inputX;
        public static int inputY;
        public static int isWalking;
        public static int isRunning;
        public static int toolEffect;

        // 工具
        public const string WateringTool = "WateringCan";
        public const string HoeingTool = "Hoe";
        public const string ChoppingTool = "Axe";
        public const string BreakingTool = "Pickaxe";
        public const string ReapingTool = "Scythe";
        public const string CollectingTool = "Basket";

        // 方向枚举
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        // 动作类型枚举
        public enum ActionType
        {
            UsingTool,
            LiftingTool,
            Picking,
            SwingingTool,
            Idle
        }

        // 使用嵌套字典存储所有方向相关的动画参数
        private static Dictionary<ActionType, Dictionary<Direction, int>> _directionalAnimations;

        // 获取指定动作类型和方向的动画参数
        public static int GetDirectionalAnimation(ActionType actionType, Direction direction)
        {
            if (_directionalAnimations.TryGetValue(actionType, out Dictionary<Direction, int> directionDict))
            {
                if (directionDict.TryGetValue(direction, out int hash))
                {
                    return hash;
                }
            }
            return 0; // 返回默认值，避免错误
        }

        public const float secondsPerGameSecond = 0.012f;  // 游戏时间秒数

        static Settings()
        {
            // 初始化基本参数
            inputX = Animator.StringToHash("inputX");
            inputY = Animator.StringToHash("inputY");
            isWalking = Animator.StringToHash("isWalking");
            isRunning = Animator.StringToHash("isRunning");
            toolEffect = Animator.StringToHash("toolEffect");

            // 初始化方向相关动画参数字典
            _directionalAnimations = new Dictionary<ActionType, Dictionary<Direction, int>>
            {
                [ActionType.UsingTool] = new Dictionary<Direction, int>
                {
                    [Direction.Up] = Animator.StringToHash("isUsingToolUp"),
                    [Direction.Down] = Animator.StringToHash("isUsingToolDown"),
                    [Direction.Left] = Animator.StringToHash("isUsingToolLeft"),
                    [Direction.Right] = Animator.StringToHash("isUsingToolRight")
                },
                [ActionType.LiftingTool] = new Dictionary<Direction, int>
                {
                    [Direction.Up] = Animator.StringToHash("isLiftingToolUp"),
                    [Direction.Down] = Animator.StringToHash("isLiftingToolDown"),
                    [Direction.Left] = Animator.StringToHash("isLiftingToolLeft"),
                    [Direction.Right] = Animator.StringToHash("isLiftingToolRight")
                },
                [ActionType.Picking] = new Dictionary<Direction, int>
                {
                    [Direction.Up] = Animator.StringToHash("isPickingUp"),
                    [Direction.Down] = Animator.StringToHash("isPickingDown"),
                    [Direction.Left] = Animator.StringToHash("isPickingLeft"),
                    [Direction.Right] = Animator.StringToHash("isPickingRight")
                },
                [ActionType.SwingingTool] = new Dictionary<Direction, int>
                {
                    [Direction.Up] = Animator.StringToHash("isSwingingToolUp"),
                    [Direction.Down] = Animator.StringToHash("isSwingingToolDown"),
                    [Direction.Left] = Animator.StringToHash("isSwingingToolLeft"),
                    [Direction.Right] = Animator.StringToHash("isSwingingToolRight")
                },
                [ActionType.Idle] = new Dictionary<Direction, int>
                {
                    [Direction.Up] = Animator.StringToHash("isIdleUp"),
                    [Direction.Down] = Animator.StringToHash("isIdleDown"),
                    [Direction.Left] = Animator.StringToHash("isIdleLeft"),
                    [Direction.Right] = Animator.StringToHash("isIdleRight")
                }
            };
        }
    }
}