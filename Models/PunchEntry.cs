namespace PunchBotCore2.Models
{
    public enum Kind { In, Out }

    public class PunchEntry
    {
        public const string TableName = "punch";

        public int Id { get; set; }
        public DateTime Time { get; set; }
        public Kind Kind { get; set; }

        public string ToSqlRow()
        {
            return $"    ('{Time:u}','{Kind}')";
        }

        public override string ToString()
        {
            return $"Id\t{Time}\t{Kind}";
        }
    }
}