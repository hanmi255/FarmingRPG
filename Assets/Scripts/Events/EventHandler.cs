using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Inventory;

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

    public struct TimeEventParameters
    {
        public int gameYear;
        public Season gameSeason;
        public int gameDay;
        public string gameDayOfWeek;
        public int gameHour;
        public int gameMinute;
        public int gameSecond;
    }

    public delegate void MovementDelegate(MovementParameters movementParams);

    public static class EventHandler
    {
        // 放置选中物品事件
        public static event Action DropSelectedItemEvent;
        public static void CallDropSelectedItemEvent()
        {
            DropSelectedItemEvent?.Invoke();
        }

        // 背包更新事件
        public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdatedEvent;
        public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
        {
            InventoryUpdatedEvent?.Invoke(inventoryLocation, inventoryList);
        }

        // 移动事件
        public static event MovementDelegate MovementEvent;
        public static void CallMovementEvent(MovementParameters movementParams)
        {
            MovementEvent?.Invoke(movementParams);
        }

        // 时间事件

        // Minute
        public static event Action<TimeEventParameters> AdvanceGameMinuteEvent;
        public static void CallAdvanceGameMinuteEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameMinuteEvent?.Invoke(timeEventParams);
        }

        // Hour
        public static event Action<TimeEventParameters> AdvanceGameHourEvent;
        public static void CallAdvanceGameHourEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameHourEvent?.Invoke(timeEventParams);
        }

        // Day
        public static event Action<TimeEventParameters> AdvanceGameDayEvent;
        public static void CallAdvanceGameDayEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameDayEvent?.Invoke(timeEventParams);
        }

        // Season
        public static event Action<TimeEventParameters> AdvanceGameSeasonEvent;
        public static void CallAdvanceGameSeasonEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameSeasonEvent?.Invoke(timeEventParams);
        }

        // Year
        public static event Action<TimeEventParameters> AdvanceGameYearEvent;
        public static void CallAdvanceGameYearEvent(TimeEventParameters timeEventParams)
        {
            AdvanceGameYearEvent?.Invoke(timeEventParams);
        }

        // 场景切换事件

        // Before Scene Unload Fade Out
        public static event Action BeforeSceneUnloadFadeOutEvent;
        public static void CallBeforeSceneUnloadFadeOutEvent()
        {
            BeforeSceneUnloadFadeOutEvent?.Invoke();
        }

        // Befor Scene Unload
        public static event Action BeforeSceneUnloadEvent;
        public static void CallBeforeSceneUnloadEvent()
        {
            BeforeSceneUnloadEvent?.Invoke();
        }

        // After Scene Load
        public static event Action AfterSceneLoadEvent;
        public static void CallAfterSceneLoadEvent()
        {
            AfterSceneLoadEvent?.Invoke();
        }

        // After Scene Load Fade In
        public static event Action AfterSceneLoadFadeInEvent;
        public static void CallAfterSceneLoadFadeInEvent()
        {
            AfterSceneLoadFadeInEvent?.Invoke();
        }
    }
}