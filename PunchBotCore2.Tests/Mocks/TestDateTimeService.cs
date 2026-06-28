using PunchBotCore2.Util;

namespace PunchBotCore2.Tests.Mocks;

public class TestDateTimeService : IDateTimeService
{
    private readonly IEnumerator<DateTime> _dates;
    public TestDateTimeService()
    {
        IEnumerable<DateTime> dates = [DateTime.Now];
        _dates = dates.GetEnumerator();
    }

    public TestDateTimeService(IEnumerable<DateTime> dates)
    {
        _dates = dates.GetEnumerator();
    }

    public DateTime Now
    {
        get
        {
            if (_dates.MoveNext())
            {
                return _dates.Current;
            }
            else
            {
                _dates.Reset();
                _dates.MoveNext();
                return _dates.Current;
            }
        }
    }
}