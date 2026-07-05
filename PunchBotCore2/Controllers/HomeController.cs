using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Data;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers;

public class HomeController(IDbContextFactory<PunchContext> contextFactory, IDateTimeService dateTimeService) : Controller
{
    private readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
    private readonly TimeSpan minBreakDuration = TimeSpan.FromMinutes(30);

    public async Task<ActionResult> Index()
    {
        return View(await GetIndexData());
    }

    private async Task<IndexData> GetIndexData()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DbSet<PunchEntry> punchEntries = context.PunchEntries;
        PunchEntry? lastEntry = await punchEntries.OrderByDescending(e => e.Time).FirstOrDefaultAsync();
        DateTime now = dateTimeService.Now;
        TimeSpan totalSum = context.GetAllTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);

        var numDays = punchEntries.GroupBy(x => x.Time.Date).Count();
        TimeSpan remainingTime = numDays * DailyWorkTime - totalSum;
        if (remainingTime <= TimeSpan.Zero && !context.HasWorkedToday(now))
        {
            remainingTime = DailyWorkTime + remainingTime;
        }
        TimeSpan daySum = context.GetDailyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        TimeSpan dayBreakSum = context.GetDailyBreakTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        DateTime estimatedEnd = dayBreakSum >= minBreakDuration ? now + remainingTime : now + remainingTime + minBreakDuration - dayBreakSum;
        IndexData indexData = new(
            WeekSum: context.GetWeeklyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
            DaySum: daySum,
            LastEntry: lastEntry,
            RemainingTime: remainingTime,
            DayBreakSum: dayBreakSum,
            EstimatedEnd: estimatedEnd
        );

        return indexData;
    }

    [HttpPost]
    public async Task<ActionResult> Punch()
    {
        DateTime now = dateTimeService.Now;

        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DbSet<PunchEntry> punchEntries = context.PunchEntries;

        PunchEntry? lastEntry = await punchEntries.OrderByDescending(e => e.Time).FirstOrDefaultAsync();
        Kind lastKind = lastEntry?.Kind ?? Kind.Out;

        await punchEntries.AddAsync(new PunchEntry(default, now, lastKind == Kind.In ? Kind.Out : Kind.In));
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<ActionResult> Holiday()
    {
        DateTime today = DateTime.Today;
        DateTime startTime = today.AddHours(8);
        DateTime endTime = startTime + DailyWorkTime;

        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DbSet<PunchEntry> punchEntries = context.PunchEntries;

        punchEntries.Add(new PunchEntry(default, startTime, Kind.In));
        punchEntries.Add(new PunchEntry(default, endTime, Kind.Out));
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Week()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        Week week = new() { TimeSpans = context.GetWeeklyTimeSpans(dateTimeService.Now) };
        return View(week);
    }

    // public ActionResult Clear()
    // {
    //     using var _db = new LiteDatabase(dbFilename);
    //     _db.DropCollection(PunchEntry.TableName);
    //     return Redirect("Index");
    // }

    public async Task<ActionResult> ListAll()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        return View(context.PunchEntries.OrderByDescending(x => x.Time).ToList());
    }

    public async Task<ActionResult> Edit(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        return View(context.PunchEntries.Find(id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(PunchEntry entry)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries
            .Where(e => e.Id == entry.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Time, _ => entry.Time));
        return RedirectToAction("ListAll");
    }

    public async Task<ActionResult> Delete(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries.Where(e => e.Id == id).ExecuteDeleteAsync();
        return RedirectToAction("ListAll");
    }

    // public FileResult DownloadDatabase()
    // {
    //     var content = System.IO.File.ReadAllBytes(dbFilename);
    //     return File(content, "application/octet-stream", "times.db");
    // }

    public async Task<ContentResult> Export()
    {
        const string header = "insert into\n    punch_entries(id, time, kind)\nvalues\n";
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        IEnumerable<string> result = context.PunchEntries
            .OrderBy(x => x.Time)
            .Select(x => x.ToSqlRow());

        return new ContentResult
        {
            Content = header + string.Join(",\n", result) + ";",
            ContentType = "application/sql",
            StatusCode = 200
        };
    }

    public IActionResult Error()
    {
        return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
