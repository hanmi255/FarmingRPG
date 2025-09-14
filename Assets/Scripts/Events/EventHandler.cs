using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;

namespace Assets.Scripts.Events
{
    #region Structs
    /// <summary>
    /// 移动参数结构体，包含角色移动相关的所有参数
    /// </summary>
    public struct MovementParameters
    {
        #region Fields
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
        #endregion
    }

    /// <summary>
    /// 时间事件参数结构体，包含游戏中时间相关的所有参数
    /// </summary>
    public struct TimeEventParameters
    {
        #region Fields
        public int gameYear;
        public Season gameSeason;
        public int gameDay;
        public string gameDayOfWeek;
        public int gameHour;
        public int gameMinute;
        public int gameSecond;
        #endregion
    }
    #endregion

    #region Delegates
    /// <summary>
    /// 移动委托，用于处理移动事件
    /// </summary>
    /// <param name="movementParams">移动参数</param>
    public delegate void MovementDelegate(MovementParameters movementParams);
    #endregion

    /// <summary>
    /// 事件处理中心，用于管理游戏中的各种事件
    /// </summary>
    public static class EventHandler
    {
        #region Events
        // 放置选中物品事件
        public static event Action DropSelectedItemEvent;
        
        // 背包更新事件
        public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdatedEvent;
        
        // 移动事件
        public static event MovementDelegate MovementEvent;
        
        // 时间事件

        // Minute
        public static event Action<TimeEventParameters> AdvanceGameMinuteEvent;
        
        // Hour
        public static event Action<TimeEventParameters> AdvanceGameHourEvent;
        
        // Day
        public static event Action<TimeEventParameters> AdvanceGameDayEvent;
        
        // Season
        public static event Action<TimeEventParameters> AdvanceGameSeasonEvent;
        
        // Year
        public static event Action<TimeEventParameters> AdvanceGameYearEvent;
        
        // 场景切换事件

        // Before Scene Unload Fade Out
        public static event Action BeforeSceneUnloadFadeOutEvent;
        
        // Befor Scene Unload
        public static event Action BeforeSceneUnloadEvent;
        
        // After Scene Load
        public static event Action AfterSceneLoadEvent;
        
        // After Scene Load Fade In
        public static event Action AfterSceneLoadFadeInEvent;
        #endregion

        #region Event Invocation Methods
        /// <summary>
        /// 触发放置选中物品事件
        /// </summary>
        public static void CallDropSelectedItemEvent()
        {
            DropSelectedItemEvent?.Invoke();
        }

        /// <summary>
        /// 触发背包更新事件
        /// </summary>
        /// <param name="inventoryLocation">背包位置</param>
        /// <param name="inventoryList">物品列表</param>
        public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
        {
            InventoryUpdatedEvent?.Invoke(inventoryLocation, inventoryList);
        }

        /// <summary>
        /// 触发移动事件
        /// </summary>
        /// <param name="movementParams">移动参数</param>
        public static void CallMovementEvent(MovementParameters movementParams)
        {
            MovementEvent?.Invoke(movementParams);
        }

        /// <summary>
        /// 触发游戏分钟推进事件
        /// </summary>
        /// <param name="timeEventParams">时间事件参数</param>
        public static void CallAdvanceGameMinuteEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameMinuteEvent?.Invoke(timeEventParams);
        }

        /// <summary>
        /// 触发游戏小时推进事件
        /// </summary>
        /// <param name="timeEventParams">时间事件参数</param>
        public static void CallAdvanceGameHourEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameHourEvent?.Invoke(timeEventParams);
        }

        /// <summary>
        /// 触发游戏日期推进事件
        /// </summary>
        /// <param name="timeEventParams">时间事件参数</param>
        public static void CallAdvanceGameDayEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameDayEvent?.Invoke(timeEventParams);
        }

        /// <summary>
        /// 触发游戏季节推进事件
        /// </summary>
        /// <param name="timeEventParams">时间事件参数</param>
        public static void CallAdvanceGameSeasonEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameSeasonEvent?.Invoke(timeEventParams);
        }

        /// <summary>
        /// 触发游戏年份推进事件
        /// </summary>
        /// <param name="timeEventParams">时间事件参数</param>
        public static void CallAdvanceGameYearEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameYearEvent?.Invoke(timeEventParams);
        }

        /// <summary>
        /// 触发场景卸载前淡出事件
        /// </summary>
        public static void CallBeforeSceneUnloadFadeOutEvent()
        {
            BeforeSceneUnloadFadeOutEvent?.Invoke();
        }

        /// <summary>
        /// 触发场景卸载前事件
        /// </summary>
        public static void CallBeforeSceneUnloadEvent()
        {
            BeforeSceneUnloadEvent?.Invoke();
        }

        /// <summary>
        /// 触发场景加载后事件
        /// </summary>
        public static void CallAfterSceneLoadEvent()
        {
            AfterSceneLoadEvent?.Invoke();
        }

        /// <summary>
        /// 触发场景加载后淡入事件
        /// </summary>
        public static void CallAfterSceneLoadFadeInEvent()
        {
            AfterSceneLoadFadeInEvent?.Invoke();
        }
        #endregion
    }
}