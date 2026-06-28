namespace PunchBotCore2.Models;

public enum Kind { In, Out }

public record PunchEntry(int Id, DateTime Time, Kind Kind)
{
    public const string TableName = "punch";

    public string ToSqlRow()
    {
        return $"    ('{Time:u}','{Kind}')";
    }

    public override string ToString()
    {
        return $"{Id}\t{Time}\t{Kind}";
    }
}