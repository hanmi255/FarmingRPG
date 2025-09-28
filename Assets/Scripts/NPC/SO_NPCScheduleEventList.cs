using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    [CreateAssetMenu(fileName ="so_NPCScheduleEventList", menuName = "ScriptableObjects/NPC/so_NPCScheduleEventList")]
    public class SO_NPCScheduleEventList : ScriptableObject
    {
        public List<NPCScheduleEvent> npcScheduleEventList;
    }
}