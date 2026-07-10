using PunchBotCore2.Data;

namespace PunchBotCore2.Util;

public interface IDataAggregatorFactory
{
    DataAggregator Create(PunchContext context);
}