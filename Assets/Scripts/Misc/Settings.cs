using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Misc
{
    public static class Settings
    {
        // 基本移动输入
        public static int inputX;
        public static int inputY;
        public static int isWalking;
        public static int isRunning;
        public static int toolEffect;

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