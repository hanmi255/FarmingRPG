using Assets.Scripts.Events;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.TimeSystem
{
    public class GameClock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText = null;
        [SerializeField] private TextMeshProUGUI _dateText = null;
        [SerializeField] private TextMeshProUGUI _seasonText = null;
        [SerializeField] private TextMeshProUGUI _yearText = null;

        private void OnEnable()
        {
            EventHandler.AdvanceGameMinuteEvent += UpdateGameTime;
        }

        private void OnDisable()
        {
            EventHandler.AdvanceGameMinuteEvent -= UpdateGameTime;
        }

        private void UpdateGameTime(TimeEventParameters parameters)
        {
            parameters.gameMinute -= parameters.gameMinute % 10;

            string ampm = parameters.gameHour >= 12 ? "PM" : "AM";

            int displayHour = parameters.gameHour;
            if (displayHour >= 13)
            {
                displayHour -= 12;
            }
            else if (displayHour == 0)
            {
                displayHour = 12;
            }

            string minute = parameters.gameMinute.ToString("00");

            string time = $"{displayHour} : {minute}{ampm}";

            _timeText.SetText(time);
            _dateText.SetText($"{parameters.gameDayOfWeek}. {parameters.gameDay}");
            _seasonText.SetText(parameters.gameSeason.ToString());
            _yearText.SetText($"Year {parameters.gameYear}");
        }
    }
}