using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Events;
using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    [RequireComponent(typeof(NPCPath))]
    public class NPCSchedule : MonoBehaviour
    {
        #region Fields
        [SerializeField] private SO_NPCScheduleEventList _so_NPCScheduleEventList = null;
        private SortedSet<NPCScheduleEvent> _npcScheduleEventSet = null;
        private NPCPath _npcPath;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _npcScheduleEventSet = new SortedSet<NPCScheduleEvent>(new NPCScheduleEventSort());

            foreach (var npcScheduleEvent in _so_NPCScheduleEventList.npcScheduleEventList)
            {
                _npcScheduleEventSet.Add(npcScheduleEvent);
            }

            _npcPath = GetComponent<NPCPath>();
        }

        private void OnEnable()
        {
            EventHandler.AdvanceGameMinuteEvent += AdvanceGameMinute;
        }

        private void OnDisable()
        {
            EventHandler.AdvanceGameMinuteEvent -= AdvanceGameMinute;
        }
        #endregion

        #region PrivateMethods
        private void AdvanceGameMinute(TimeEventParameters parameters)
        {
            int time = (parameters.gameHour * 100) + parameters.gameMinute;

            NPCScheduleEvent matchingEvent = null;

            foreach (var npcScheduleEvent in _npcScheduleEventSet)
            {
                if (npcScheduleEvent.Time == time)
                {
                    if (npcScheduleEvent.day != 0 && npcScheduleEvent.day != parameters.gameDay)
                        continue;

                    if (npcScheduleEvent.season != Season.None && npcScheduleEvent.season != parameters.gameSeason)
                        continue;

                    if (npcScheduleEvent.weather != Weather.None && npcScheduleEvent.weather != GameManager.Instance.currentWeather)
                        continue;

                    matchingEvent = npcScheduleEvent;
                    break;
                }
                else if (npcScheduleEvent.Time > time)
                    break;
            }

            if (matchingEvent != null)
            {
                _npcPath.BuildPath(matchingEvent);
            }
        }
        #endregion
    }
}
