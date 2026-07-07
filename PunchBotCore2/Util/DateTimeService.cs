using System.Diagnostics.CodeAnalysis;

namespace PunchBotCore2.Util;

[ExcludeFromCodeCoverage]
public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
}
