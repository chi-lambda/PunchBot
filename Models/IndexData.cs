using System;

namespace PunchBotCore2.Models
{
    public class IndexData
    {
        public PunchEntry LastEntry { get; }
        public TimeSpan WeekSum { get; }
        public TimeSpan DaySum { get; }
        public TimeSpan RemainingTime { get; }
        public string RemainingTimeSign => RemainingTime.Ticks < 0 ? "-" : "";
        public TimeSpan DayBreakSum { get; }
        public DateTime EstimatedEnd{ get; }

        public IndexData(PunchEntry lastEntry, TimeSpan weekSum, TimeSpan daySum, TimeSpan remainingTime, TimeSpan dayBreakSum, DateTime estimatedEnd)
        {
            LastEntry = lastEntry;
            WeekSum = weekSum;
            DaySum = daySum;
            RemainingTime = remainingTime;
            DayBreakSum = dayBreakSum;
            EstimatedEnd = estimatedEnd;
        }
    }
}