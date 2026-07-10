using PunchBotCore2.Data;
using PunchBotCore2.Util;

namespace PunchBotCore2.Tests.Mocks;

public class TestDataAggregatorFactory(IDateTimeService dateTimeService) : IDataAggregatorFactory
{
    public DataAggregator Create(PunchContext context)
    {
        return new(context, dateTimeService);
    }
}