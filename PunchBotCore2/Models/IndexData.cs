namespace PunchBotCore2.Models;

public record IndexData(PunchEntry? LastEntry, TimeSpan WeekSum, TimeSpan DaySum, TimeSpan RemainingTime, TimeSpan DayBreakSum, DateTime EstimatedEnd)
{
    public string RemainingTimeSign => RemainingTime.Ticks < 0 ? "-" : "";
}