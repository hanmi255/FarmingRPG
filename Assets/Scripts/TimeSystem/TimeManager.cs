using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using Assets.Scripts.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.TimeSystem
{
    [RequireComponent(typeof(GenerateGUID))]
    /// <summary>
    /// 时间管理器 - 负责游戏内时间的推进和管理
    /// </summary>
    public class TimeManager : SingletonMonoBehaviour<TimeManager>, ISaveable
    {
        #region Fields

        // 游戏时间相关字段
        private int _gameYear = 1;
        private Season _gameSeason = Season.Spring;
        private int _gameDay = 1;
        private string _gameDayOfWeek = "Mon";
        private int _gameHour = 6;
        private int _gameMinute = 30;
        private int _gameSecond = 0;
        private bool _gameClockPaused = false;
        private float _gameTick = 0f;

        private TimeEventParameters _parameters;

        private string _iSaveableUniqueID;                               // 保存ID
        private GameObjectSave _gameObjectSave;                          // 游戏对象保存

        public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

        public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            base.Awake();

            _iSaveableUniqueID = GetComponent<GenerateGUID>().GUID;
            _gameObjectSave = new GameObjectSave();
        }

        private void OnEnable()
        {
            ISaveableRegister();

            Events.EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
            Events.EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        }


        private void OnDisable()
        {
            ISaveableDeregister();

            Events.EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
            Events.EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        }

        private void Start()
        {
            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameMinuteEvent(_parameters);
        }

        private void Update()
        {
            if (_gameClockPaused)
                return;

            GameTick();
        }

        #endregion

        #region ISaveable Interface Methods
        /// <summary>
        /// 注册可保存对象到保存管理器
        /// </summary>
        public void ISaveableRegister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Add(this);
        }

        /// <summary>
        /// 从保存管理器中注销可保存对象
        /// </summary>
        public void ISaveableDeregister()
        {
            SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <returns>游戏对象保存数据</returns>
        public GameObjectSave ISaveableSave()
        {
            _gameObjectSave.sceneData.Remove(Settings.PersistentSceneName);

            SceneSave sceneSave = new()
            {
                intDictionary = new Dictionary<string, int>(),
                stringDictionary = new Dictionary<string, string>()
            };

            sceneSave.intDictionary.Add("gameYear", _gameYear);
            sceneSave.intDictionary.Add("gameDay", _gameDay);
            sceneSave.intDictionary.Add("gameHour", _gameHour);
            sceneSave.intDictionary.Add("gameMinute", _gameMinute);
            sceneSave.intDictionary.Add("gameSecond", _gameSecond);

            sceneSave.stringDictionary.Add("gameDayOfWeek", _gameDayOfWeek);
            sceneSave.stringDictionary.Add("gameSeason", _gameSeason.ToString());

            _gameObjectSave.sceneData.Add(Settings.PersistentSceneName, sceneSave);

            return _gameObjectSave;
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <param name="gameSave">游戏保存数据</param>
        public void ISaveableLoad(GameSave gameSave)
        {
            if (!gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out var gameObjectSave))
                return;

            _gameObjectSave = gameObjectSave;

            if (!_gameObjectSave.sceneData.TryGetValue(Settings.PersistentSceneName, out var sceneSave))
                return;

            if(sceneSave.intDictionary == null || sceneSave.stringDictionary == null)
                return;

            if(sceneSave.intDictionary.TryGetValue("gameYear", out int gameYear))
                _gameYear = gameYear;
            if(sceneSave.intDictionary.TryGetValue("gameDay", out int gameDay))
                _gameDay = gameDay;
            if(sceneSave.intDictionary.TryGetValue("gameHour", out int gameHour))
                _gameHour = gameHour;
            if(sceneSave.intDictionary.TryGetValue("gameMinute", out int gameMinute))
                _gameMinute = gameMinute;
            if(sceneSave.intDictionary.TryGetValue("gameSecond", out int gameSecond))
                _gameSecond = gameSecond;

            if(sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string gameDayOfWeek))
                _gameDayOfWeek = gameDayOfWeek;
            if(sceneSave.stringDictionary.TryGetValue("gameSeason", out string gameSeason))
                _gameSeason = (Season)Enum.Parse(typeof(Season), gameSeason);

            _gameTick = 0f;

            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameMinuteEvent(_parameters);
        }

        public void ISaveableStoreScene(string sceneName)
        {
            // Nothing to store
        }

        public void ISaveableRestoreScene(string sceneName)
        {
            // Nothing to restore
        }
        #endregion

        #region Private Methods

        private void BeforeSceneUnload()
        {
            _gameClockPaused = true;
        }

        private void AfterSceneLoad()
        {
            _gameClockPaused = false;
        }

        /// <summary>
        /// 游戏时间滴答更新
        /// </summary>
        private void GameTick()
        {
            _gameTick += Time.deltaTime;
            if (_gameTick >= Settings.secondsPerGameSecond)
            {
                _gameTick -= Settings.secondsPerGameSecond;
                UpdateGameSecond();
            }
        }

        /// <summary>
        /// 更新游戏秒数
        /// </summary>
        private void UpdateGameSecond()
        {
            _gameSecond++;
            if (_gameSecond <= 59)
            {
                SetTimeEventParameters();
                return;
            }

            _gameSecond = 0;
            _gameMinute++;
            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameMinuteEvent(_parameters);

            if (_gameMinute <= 59)
                return;

            _gameMinute = 0;
            _gameHour++;
            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameHourEvent(_parameters);

            if (_gameHour <= 23)
                return;

            _gameHour = 0;
            _gameDay++;
            _gameDayOfWeek = GetDayOfWeek();
            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameDayEvent(_parameters);

            if (_gameDay <= 30)
                return;

            _gameDay = 1;
            _gameSeason++;
            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameSeasonEvent(_parameters);

            if ((int)_gameSeason <= 3)
                return;

            _gameSeason = 0;
            _gameYear++;

            // 重置年
            if (_gameYear > 9999)
                _gameYear = 1;

            SetTimeEventParameters();
            Events.EventHandler.CallAdvanceGameYearEvent(_parameters);
        }

        /// <summary>
        /// 获取当前星期几
        /// </summary>
        /// <returns>星期几的字符串表示</returns>
        private string GetDayOfWeek()
        {
            // 计算总天数并取模得到星期几
            int totalDays = (((int)_gameSeason) * 30) + _gameDay;
            int dayOfWeek = totalDays % 7;
            return dayOfWeek switch
            {
                0 => "Sun",
                1 => "Mon",
                2 => "Tue",
                3 => "Wed",
                4 => "Thu",
                5 => "Fri",
                6 => "Sat",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// 设置时间事件参数
        /// </summary>
        private void SetTimeEventParameters()
        {
            _parameters = new TimeEventParameters
            {
                gameYear = _gameYear,
                gameSeason = _gameSeason,
                gameDay = _gameDay,
                gameDayOfWeek = _gameDayOfWeek,
                gameHour = _gameHour,
                gameMinute = _gameMinute,
                gameSecond = _gameSecond
            };
        }

        #endregion

        // Test
        public void TestAdvanceGameMinute()
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameSecond();
            }
        }

        public void TestAdvanceGameDay()
        {
            for (int i = 0; i < 86400; i++)
            {
                UpdateGameSecond();
            }
        }
    }
}