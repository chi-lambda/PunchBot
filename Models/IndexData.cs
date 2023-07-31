using System;

namespace PunchBotCore2.Models
{
    public class IndexData
    {
        public PunchEntry LastEntry { get; set; }
        public TimeSpan WeekSum { get; set; }
        public TimeSpan DaySum { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public string RemainingTimeSign => RemainingTime.Ticks < 0 ? "-" : "";
        public TimeSpan DayBreakSum { get; set; }

        public IndexData(PunchEntry lastEntry, TimeSpan weekSum, TimeSpan daySum, TimeSpan remainingTime, TimeSpan dayBreakSum)
        {
            LastEntry = lastEntry;
            WeekSum = weekSum;
            DaySum = daySum;
            RemainingTime = remainingTime;
            DayBreakSum = dayBreakSum;
        }
    }
}