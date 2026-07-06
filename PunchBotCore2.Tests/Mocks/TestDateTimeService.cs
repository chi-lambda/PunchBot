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
                if (_dates.MoveNext())
                {
                    return _dates.Current;
                }
                else
                {
                    throw new InvalidOperationException("Date was requested from TestDateTimeService, but none were provided");
                }
            }
        }
    }

    public DateTime Today
    {
        get
        {
            if (_dates.MoveNext())
            {
                return _dates.Current.Date;
            }
            else
            {
                _dates.Reset();
                if (_dates.MoveNext())
                {
                    return _dates.Current.Date;
                }
                else
                {
                    throw new InvalidOperationException("Date was requested from TestDateTimeService, but none were provided");
                }
            }
        }
    }
}