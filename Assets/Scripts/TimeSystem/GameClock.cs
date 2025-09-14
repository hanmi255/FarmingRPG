using Assets.Scripts.Events;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.TimeSystem
{
    /// <summary>
    /// 游戏时钟类，负责显示游戏中的时间、日期、季节和年份信息
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI _timeText = null;
        [SerializeField] private TextMeshProUGUI _dateText = null;
        [SerializeField] private TextMeshProUGUI _seasonText = null;
        [SerializeField] private TextMeshProUGUI _yearText = null;

        #endregion

        #region Lifecycle Methods

        private void OnEnable()
        {
            EventHandler.AdvanceGameMinuteEvent += UpdateGameTime;
        }

        private void OnDisable()
        {
            EventHandler.AdvanceGameMinuteEvent -= UpdateGameTime;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 更新游戏时间显示
        /// </summary>
        /// <param name="parameters">时间事件参数，包含游戏时间信息</param>
        private void UpdateGameTime(TimeEventParameters parameters)
        {
            // 将分钟数调整为10的倍数，确保时间显示符合游戏设计
            parameters.gameMinute -= parameters.gameMinute % 10;

            // 确定是上午还是下午
            string ampm = parameters.gameHour >= 12 ? "PM" : "AM";

            // 转换24小时制为12小时制显示
            int displayHour = parameters.gameHour;
            if (displayHour >= 13)
            {
                displayHour -= 12;
            }
            else if (displayHour == 0)
            {
                displayHour = 12;
            }

            // 格式化分钟数，确保始终显示两位数
            string minute = parameters.gameMinute.ToString("00");

            // 构建时间显示字符串
            string time = $"{displayHour} : {minute}{ampm}";

            // 更新UI文本显示
            _timeText.SetText(time);
            _dateText.SetText($"{parameters.gameDayOfWeek}. {parameters.gameDay}");
            _seasonText.SetText(parameters.gameSeason.ToString());
            _yearText.SetText($"Year {parameters.gameYear}");
        }

        #endregion
    }
}