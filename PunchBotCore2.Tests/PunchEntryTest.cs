using PunchBotCore2.Models;

namespace PunchBotCore2.Tests;

[TestClass]
public class PunchEntryTest
{
    [TestMethod]
    public void ToStringTest()
    {
        PunchEntry entry = new(1, new DateTime(2026, 07, 06, 10, 0, 0), Kind.In);
        Assert.AreEqual("1\t2026-07-06T10:00:00.0000000\tIn", entry.ToString());
    }
}