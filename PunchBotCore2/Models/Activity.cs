namespace PunchBotCore2.Models;

public record Activity(DateTime Start, DateTime End)
{
    public string Description { get; set; } = string.Empty;
    public TimeSpan Duration => End - Start;
}