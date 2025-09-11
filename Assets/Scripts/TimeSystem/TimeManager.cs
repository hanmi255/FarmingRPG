using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.TimeSystem
{
    public class TimeManager : SingletonMonoBehaviour<TimeManager>
    {
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

        private void GameTick()
        {
            _gameTick += Time.deltaTime;
            if (_gameTick >= Settings.secondsPerGameSecond)
            {
                _gameTick -= Settings.secondsPerGameSecond;
                UpdateGameSecond();
            }
        }

        private void UpdateGameSecond()
        {
            _gameSecond++;
            if (_gameSecond <= 59) 
            {
                SetTimeEventParameters();
                Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
                return;
            }

            _gameSecond = 0;
            _gameMinute++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameMinuteEvent(_parameters);

            if (_gameMinute <= 59) {
                Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
                return;
            }

            _gameMinute = 0;
            _gameHour++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameHourEvent(_parameters);

            if (_gameHour <= 23) {
                Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
                return;
            }

            _gameHour = 0;
            _gameDay++;
            _gameDayOfWeek = GetDayOfWeek();
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameDayEvent(_parameters);

            if (_gameDay <= 30) {
                Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
                return;
            }

            _gameDay = 1;
            _gameSeason++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameSeasonEvent(_parameters);

            if ((int)_gameSeason <= 3) {
                Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
                return;
            }

            _gameSeason = 0;
            _gameYear++;
            SetTimeEventParameters();
            EventHandler.CallAdvanceGameYearEvent(_parameters);
            
            Debug.Log("Time: " + _gameHour + ":" + _gameMinute + ":" + _gameSecond);
        }

        private string GetDayOfWeek()
        {
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
    }
}