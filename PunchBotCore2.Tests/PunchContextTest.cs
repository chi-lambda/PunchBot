using PunchBotCore2.Data;
using PunchBotCore2.Models;
using PunchBotCore2.Tests.Mocks;

namespace PunchBotCore2.Tests;

[TestClass]
public class PunchContextTest
{
    [TestMethod]
    public async Task MigrateTest()
    {
        var liteDb = new LiteDB.LiteDatabase(new MemoryStream(10240));
        var col = liteDb.GetCollection<PunchEntry>(PunchEntry.TableName);
        col.InsertBulk([
            new(1, DateTime.Today.AddHours(8), Kind.In),
            new(2, DateTime.Today.AddHours(11), Kind.In),
            new(3, DateTime.Today.AddHours(12), Kind.In),
            new(4, DateTime.Today.AddHours(16), Kind.In),
        ]);
        TestPunchContextFactory contextFactory = new();
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            await context.Migrate(liteDb);
            Assert.AreEqual(4, context.PunchEntries.Count());
            await context.Migrate(liteDb);
            Assert.AreEqual(4, context.PunchEntries.Count());
        }
    }
}