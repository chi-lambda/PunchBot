namespace PunchBotCore2.Models
{
    public class Week
    {
        public List<Activity> TimeSpans { get; set; } = [];
        public TimeSpan Sum => TimeSpans.Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
    }
}