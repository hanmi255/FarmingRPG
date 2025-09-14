using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.TimeSystem
{
    /// <summary>
    /// 时间管理器 - 负责游戏内时间的推进和管理
    /// </summary>
    public class TimeManager : SingletonMonoBehaviour<TimeManager>
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

        #endregion

        #region Lifecycle Methods

        private void Start()
        {
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameMinuteEvent(_parameters);
        }

        private void Update()
        {
            if (_gameClockPaused)
                return;

            GameTick();
        }

        #endregion

        #region Private Methods

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
            EventHandler.CallAdvanceGameMinuteEvent(_parameters);

            if (_gameMinute <= 59)
                return;

            _gameMinute = 0;
            _gameHour++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameHourEvent(_parameters);

            if (_gameHour <= 23)
                return;

            _gameHour = 0;
            _gameDay++;
            _gameDayOfWeek = GetDayOfWeek();
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameDayEvent(_parameters);

            if (_gameDay <= 30)
                return;

            _gameDay = 1;
            _gameSeason++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameSeasonEvent(_parameters);

            if ((int)_gameSeason <= 3)
                return;

            _gameSeason = 0;
            _gameYear++;
            
            // 重置年
            if (_gameYear > 9999)
                _gameYear = 1;

            SetTimeEventParameters();
            EventHandler.CallAdvanceGameYearEvent(_parameters);
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
    }
}