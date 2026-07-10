using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Controllers;
using PunchBotCore2.Data;
using PunchBotCore2.Models;
using PunchBotCore2.Tests.Mocks;
using PunchBotCore2.Util;

namespace PunchBotCore2.Tests;

[TestClass]
public sealed class HomeControllerTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Index_WorksWithInitialDatabase()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(23)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
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
        Assert.AreEqual("", model.RemainingTimeSign);
    }

    [TestMethod]
    public async Task Index_OnePunchAdded()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(10)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.FromHours(2), model.DaySum);
        Assert.AreEqual(TimeSpan.FromHours(2), model.WeekSum);
        Assert.AreEqual(TimeSpan.Zero, model.DayBreakSum);
        Assert.AreEqual(new(1, DateTime.Today.AddHours(8), Kind.In), model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(5), model.RemainingTime);
        Assert.AreEqual("", model.RemainingTimeSign);
    }

    [TestMethod]
    public async Task Index_TwoPunchesAdded()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(12)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
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
        Assert.AreEqual(TimeSpan.FromHours(2), model.DayBreakSum);
        Assert.AreEqual(new(2, DateTime.Today.AddHours(10), Kind.Out), model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(5), model.RemainingTime);
        Assert.AreEqual("", model.RemainingTimeSign);
    }

    [TestMethod]
    public async Task Index_ThreePunchesAdded()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(14)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(2, DateTime.Today.AddHours(10), Kind.Out));
            context.PunchEntries.Add(new(3, DateTime.Today.AddHours(12), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.FromHours(4), model.DaySum);
        Assert.AreEqual(TimeSpan.FromHours(4), model.WeekSum);
        Assert.AreEqual(TimeSpan.FromHours(2), model.DayBreakSum);
        Assert.AreEqual(new(3, DateTime.Today.AddHours(12), Kind.In), model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(3), model.RemainingTime);
        Assert.AreEqual("", model.RemainingTimeSign);
    }
    [TestMethod]

    public async Task Index_FourPunchesAdded()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(23)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(2, DateTime.Today.AddHours(12), Kind.Out));
            context.PunchEntries.Add(new(3, DateTime.Today.AddHours(13), Kind.In));
            context.PunchEntries.Add(new(4, DateTime.Today.AddHours(16), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.FromHours(7), model.DaySum);
        Assert.AreEqual(TimeSpan.FromHours(7), model.WeekSum);
        Assert.AreEqual(TimeSpan.FromHours(1), model.DayBreakSum);
        Assert.AreEqual(new(4, DateTime.Today.AddHours(16), Kind.Out), model.LastEntry);
        Assert.AreEqual(TimeSpan.Zero, model.RemainingTime);
        Assert.AreEqual("", model.RemainingTimeSign);
    }

    [TestMethod]
    public async Task Index_FourPunchesAdded_WithOvertime()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(23)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(2, DateTime.Today.AddHours(12), Kind.Out));
            context.PunchEntries.Add(new(3, DateTime.Today.AddHours(13), Kind.In));
            context.PunchEntries.Add(new(4, DateTime.Today.AddHours(17), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(TimeSpan.FromHours(8), model.DaySum);
        Assert.AreEqual(TimeSpan.FromHours(8), model.WeekSum);
        Assert.AreEqual(TimeSpan.FromHours(1), model.DayBreakSum);
        Assert.AreEqual(new(4, DateTime.Today.AddHours(17), Kind.Out), model.LastEntry);
        Assert.AreEqual(TimeSpan.FromHours(-1), model.RemainingTime);
        Assert.AreEqual("-", model.RemainingTimeSign);
    }

    [TestMethod]
    public async Task Delete_RemovesRow()
    {
        IDateTimeService dateTimeService = new TestDateTimeService();
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
            Assert.AreEqual(1, context.PunchEntries.Count());
        }
        await controller.Delete(1);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(0, context.PunchEntries.Count());
        }
    }

    [TestMethod]
    public async Task Edit_UpdatesTime()
    {
        IDateTimeService dateTimeService = new TestDateTimeService();
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
            Assert.AreEqual(1, context.PunchEntries.Count());
        }
        await controller.Edit(new PunchEntry(1, DateTime.Today.AddHours(10), Kind.In));
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(DateTime.Today.AddHours(10), context.PunchEntries.First().Time);
        }
    }

    [TestMethod]
    public async Task Edit_ShowsView()
    {
        IDateTimeService dateTimeService = new TestDateTimeService();
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
            Assert.AreEqual(1, context.PunchEntries.Count());
        }
        ActionResult result = await controller.Edit(1);
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<PunchEntry>(viewResult.Model);
        PunchEntry model = (PunchEntry)viewResult.Model;
        Assert.AreEqual(1, model.Id);
        Assert.AreEqual(DateTime.Today.AddHours(8), model.Time);
        Assert.AreEqual(Kind.In, model.Kind);
    }

    [TestMethod]
    public async Task Edit_Returns404()
    {
        IDateTimeService dateTimeService = new TestDateTimeService();
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
            Assert.AreEqual(1, context.PunchEntries.Count());
        }
        ActionResult result = await controller.Edit(2);
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Edit_PostReturns404()
    {
        IDateTimeService dateTimeService = new DateTimeService();
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(1, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }
        ActionResult result = await controller.Edit(new PunchEntry(2, DateTime.Today.AddHours(10), Kind.In));
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Punch_AddsFirstInEntry()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(8)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

        await controller.Punch();

        using PunchContext context = contextFactory.CreateDbContext();
        Assert.AreEqual(new PunchEntry(1, DateTime.Today.AddHours(8), Kind.In), context.PunchEntries.First());
    }

    [TestMethod]
    public async Task Punch_AddsFirstOutEntry()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(10)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, DateTime.Today.AddHours(8), Kind.In));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        await controller.Punch();

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(new PunchEntry(2, DateTime.Today.AddHours(10), Kind.Out), context.PunchEntries.OrderByDescending(e => e.Time).First());
        }
    }

    [TestMethod]
    public async Task Punch_AddsSecondInEntry()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today.AddHours(12)]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, DateTime.Today.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(default, DateTime.Today.AddHours(10), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        await controller.Punch();

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(new PunchEntry(3, DateTime.Today.AddHours(12), Kind.In), context.PunchEntries.OrderByDescending(e => e.Time).First());
        }
    }

    [TestMethod]
    public async Task Holiday_AddsTwoPunches()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([DateTime.Today]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

        await controller.Holiday();
        using (PunchContext context = contextFactory.CreateDbContext())
        {
            Assert.AreEqual(2, context.PunchEntries.Count());
            DateTime foo = DateTime.Today.AddHours(8);
            List<PunchEntry> entries = context.PunchEntries.OrderBy(e => e.Time).ToList();
            Assert.AreEqual(new PunchEntry(1, DateTime.Today.AddHours(8), Kind.In), entries[0]);
            Assert.AreEqual(new PunchEntry(2, DateTime.Today.AddHours(15), Kind.Out), entries[1]);
        }

    }

    [TestMethod]
    public async Task Week_ReturnsOneDay()
    {
        DateTime monday = new(2026, 07, 06);
        DateTime fridayEOD = monday.AddDays(4).AddHours(23);
        IDateTimeService dateTimeService = new TestDateTimeService([fridayEOD]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

        using (PunchContext context = contextFactory.CreateDbContext())
        {
            context.PunchEntries.Add(new(default, monday.AddHours(8), Kind.In));
            context.PunchEntries.Add(new(default, monday.AddHours(11), Kind.Out));
            context.PunchEntries.Add(new(default, monday.AddHours(12), Kind.In));
            context.PunchEntries.Add(new(default, monday.AddHours(16), Kind.Out));
            await context.SaveChangesAsync(TestContext.CancellationToken);
        }

        ActionResult result = await controller.Week();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<Week>(viewResult.Model);
        Week model = (Week)viewResult.Model;
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
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

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
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<Week>(viewResult.Model);
        Week model = (Week)viewResult.Model;
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
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);

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
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<List<PunchEntry>>(viewResult.Model);
        List<PunchEntry> model = (List<PunchEntry>)viewResult.Model;
        Assert.HasCount(4 * 5, model);
    }

    [TestMethod]
    public async Task Error_ReturnsErrorViewModel()
    {
        IDateTimeService dateTimeService = new TestDateTimeService([]);
        TestPunchContextFactory contextFactory = new();
        TestDataAggregatorFactory aggregatorFactory = new(dateTimeService);
        HomeController controller = new(contextFactory, aggregatorFactory, dateTimeService);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        IActionResult result = await controller.Error();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ErrorViewModel>(viewResult.Model);
    }
}
