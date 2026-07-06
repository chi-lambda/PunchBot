using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Controllers;
using PunchBotCore2.Data;
using PunchBotCore2.Models;
using PunchBotCore2.Tests.Mocks;
using PunchBotCore2.Util;

namespace PunchBotCore2.Tests;

[TestClass]
public sealed class JsonControllerTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Get_ReturnsPunchEntry()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        IActionResult result = await controller.Get(1);
        Assert.IsInstanceOfType<JsonResult>(result);
        JsonResult jsonResult = (JsonResult)result;
        Assert.IsInstanceOfType<PunchEntry>(jsonResult.Value);
        Assert.AreEqual(new PunchEntry(1, DateTime.Today.AddHours(8), Kind.In), jsonResult.Value);
    }

    [TestMethod]
    public async Task Get_ReturnsNotFound()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        IActionResult result = await controller.Get(2);
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Patch_UpdatesTime()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Patch(new PunchEntry(1, DateTime.Today.AddHours(10), Kind.In));
        Assert.IsInstanceOfType<OkResult>(result);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(new PunchEntry(1, DateTime.Today.AddHours(10), Kind.In), context.PunchEntries.Single());
        }
    }

    [TestMethod]
    public async Task Patch_ReturnsNotFound()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Patch(new PunchEntry(2, DateTime.Today.AddHours(10), Kind.In));
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Delete_RemovedEntity()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
            Assert.AreEqual(1, context.PunchEntries.Count());
        }
        ActionResult result = await controller.Delete(1);
        Assert.IsInstanceOfType<NoContentResult>(result);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(0, context.PunchEntries.Count());
        }
    }


    [TestMethod]
    public async Task Week_ReturnsOneDay()
    {
        DateTime monday = new(2026, 07, 06);
        DateTime fridayEOD = monday.AddDays(4).AddHours(23);
        IDateTimeService dateTimeService = new TestDateTimeService([fridayEOD]);
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, monday.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(default, monday.AddHours(11), Kind.Out));
            context.PunchEntries.Add(new(default, monday.AddHours(12), Kind.In));
            context.PunchEntries.Add(new(default, monday.AddHours(16), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        ActionResult result = await controller.Week();
        Assert.IsInstanceOfType<JsonResult>(result);
        JsonResult viewResult = (JsonResult)result;
        Assert.IsInstanceOfType<Week>(viewResult.Value);
        Week model = (Week)viewResult.Value;
        Assert.HasCount(2, model.TimeSpans);
        Assert.AreEqual(TimeSpan.FromHours(7), model.Sum);
    }

    [TestMethod]
    public async Task Week_ReturnsWholeWeek()
    {
        DateTime monday = new(2026, 07, 06);
        DateTime fridayEOD = monday.AddDays(4).AddHours(23);
        IDateTimeService dateTimeService = new TestDateTimeService([fridayEOD]);
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            for (int i = 0; i < 5; i++)
            {
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(8), Kind.In));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(11), Kind.Out));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(12), Kind.In));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(16), Kind.Out));
            }
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        ActionResult result = await controller.Week();
        Assert.IsInstanceOfType<JsonResult>(result);
        JsonResult viewResult = (JsonResult)result;
        Assert.IsInstanceOfType<Week>(viewResult.Value);
        Week model = (Week)viewResult.Value;
        Assert.HasCount(2 * 5, model.TimeSpans);
        Assert.AreEqual(TimeSpan.FromHours(35), model.Sum);
    }

    [TestMethod]
    public async Task ListAll_ReturnsEverything()
    {
        DateTime monday = new(2026, 07, 06);
        DateTime fridayEOD = monday.AddDays(4).AddHours(23);
        IDateTimeService dateTimeService = new TestDateTimeService([]);
        TestPunchContextFactory contextFactory = new();
        JsonController controller = new(contextFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            for (int i = 0; i < 5; i++)
            {
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(8), Kind.In));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(11), Kind.Out));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(12), Kind.In));
                context.PunchEntries.Add(new(default, monday.AddDays(i).AddHours(16), Kind.Out));
            }
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        ActionResult result = await controller.ListAll();
        Assert.IsInstanceOfType<JsonResult>(result);
        JsonResult viewResult = (JsonResult)result;
        Assert.IsInstanceOfType<List<PunchEntry>>(viewResult.Value);
        List<PunchEntry> model = (List<PunchEntry>)viewResult.Value;
        Assert.HasCount(4 * 5, model);
    }
}
