namespace PunchBotCore2.Models
{
    public class IndexData(PunchEntry lastEntry, TimeSpan weekSum, TimeSpan daySum, TimeSpan remainingTime, TimeSpan dayBreakSum, DateTime estimatedEnd)
    {
        public PunchEntry LastEntry { get; } = lastEntry;
        public TimeSpan WeekSum { get; } = weekSum;
        public TimeSpan DaySum { get; } = daySum;
        public TimeSpan RemainingTime { get; } = remainingTime;
        public string RemainingTimeSign => RemainingTime.Ticks < 0 ? "-" : "";
        public TimeSpan DayBreakSum { get; } = dayBreakSum;
        public DateTime EstimatedEnd { get; } = estimatedEnd;
    }
}