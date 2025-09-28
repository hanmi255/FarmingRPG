using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.NPC
{
    public class NPCScheduleEventSort : IComparer<NPCScheduleEvent>
    {
        public int Compare(NPCScheduleEvent event1, NPCScheduleEvent event2)
        {
            // 处理引用和null情况
            if (ReferenceEquals(event1, event2)) return 0;
            if (event1 is null) return -1;
            if (event2 is null) return 1;

            // 按照Time升序比较
            int timeComparison = Comparer.Default.Compare(event1.Time, event2.Time);
            if (timeComparison != 0) return timeComparison;

            // 如果Time相等，按照优先级升序比较
            return event1.priority.CompareTo(event2.priority);
        }
    }
}