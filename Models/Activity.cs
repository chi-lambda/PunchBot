using LiteDB;

namespace PunchBotCore2.Models
{
    public class Activity
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Description { get; set; } = string.Empty;

        [BsonIgnore]
        public TimeSpan Duration => (End ?? DateTime.Now) - Start;
    }
}