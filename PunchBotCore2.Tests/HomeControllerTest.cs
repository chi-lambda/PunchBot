using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Controllers;
using PunchBotCore2.Data;
using PunchBotCore2.Models;
using PunchBotCore2.Tests.Mocks;

namespace PunchBotCore2.Tests;

[TestClass]
public sealed class HomeControllerTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Index_WorksWithInitialDatabase()
    {
        HomeController controller = new(new TestPunchContextFactory());
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.Zero, model.DaySum);
        Assert.AreEqual(TimeSpan.Zero, model.WeekSum);
        Assert.AreEqual(TimeSpan.Zero, model.DayBreakSum);
        Assert.IsNull(model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(7), model.RemainingTime);
    }

    [TestMethod]
    public async Task Index_TwoPunchesAdded()
    {
        TestPunchContextFactory contextFactory = new();
        HomeController controller = new(contextFactory);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(2, DateTime.Today.AddHours(10), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.FromHours(2), model.DaySum);
        Assert.AreEqual(TimeSpan.FromHours(2), model.WeekSum);
        Assert.AreEqual(new(2, DateTime.Today.AddHours(10), Kind.Out), model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(5), model.RemainingTime);
    }
}
