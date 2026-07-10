using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Data;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers;

public class HomeController(
    IDbContextFactory<PunchContext> contextFactory,
    IDataAggregatorFactory aggregatorFactory,
    IDateTimeService dateTimeService) : Controller
{
    private static readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
    private static readonly TimeSpan MinBreakDuration = TimeSpan.FromMinutes(30);

    public async Task<ActionResult> Index()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DataAggregator aggregator = aggregatorFactory.Create(context);
        return View(await aggregator.GetIndexData());
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
        DateTime today = dateTimeService.Today;
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
        DataAggregator aggregator = aggregatorFactory.Create(context);
        Week week = new() { TimeSpans = await aggregator.GetWeeklyTimeSpans(dateTimeService.Now) };
        return View(week);
    }

    public async Task<ActionResult> ListAll()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        return View(context.PunchEntries.OrderByDescending(x => x.Time).ToList());
    }

    public async Task<ActionResult> Edit(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        PunchEntry? entry = context.PunchEntries.Find(id);
        if (entry is not null)
        {
            return View(entry);
        }
        return NotFound();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(PunchEntry entry)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        int updatedRows = await context.PunchEntries
            .Where(e => e.Id == entry.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Time, _ => entry.Time));
        return updatedRows > 0 ? RedirectToAction("ListAll") : NotFound();
    }

    public async Task<ActionResult> Delete(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries.Where(e => e.Id == id).ExecuteDeleteAsync();
        return RedirectToAction("ListAll");
    }

    public async Task<IActionResult> Error()
    {
        return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
