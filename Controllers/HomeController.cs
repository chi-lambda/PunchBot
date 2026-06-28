using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Util;
using PunchBotCore2.Data;
using Microsoft.EntityFrameworkCore;

namespace PunchBotCore2.Controllers;

public class HomeController(PunchContext context) : Controller
{
    private readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
    private readonly TimeSpan minBreakDuration = TimeSpan.FromMinutes(30);
    private IQueryable<PunchEntry> PunchEntries { get; } = context.PunchEntries;

    public ActionResult Index()
    {
        return View(GetIndexData());
    }

    private IndexData GetIndexData()
    {
        DbSet<PunchEntry> punchEntries = context.PunchEntries;
        PunchEntry lastEntry = punchEntries.OrderByDescending(e => e.Time).First();
        DateTime now = DateTime.Now;
        TimeSpan totalSum = context.GetAllTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);

        var numDays = punchEntries.GroupBy(x => x.Time.Date).Count();
        TimeSpan remainingTime = numDays * DailyWorkTime - totalSum;
        if (remainingTime <= TimeSpan.Zero)
        {
            remainingTime = DailyWorkTime + remainingTime;
        }
        TimeSpan daySum = context.GetDailyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        TimeSpan dayBreakSum = context.GetDailyBreakTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        DateTime estimatedEnd = dayBreakSum >= minBreakDuration ? DateTime.Now + remainingTime : DateTime.Now + remainingTime + minBreakDuration - dayBreakSum;
        IndexData indexData = new(
            weekSum: context.GetWeeklyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
            daySum: daySum,
            lastEntry: lastEntry,
            remainingTime: remainingTime,
            dayBreakSum: dayBreakSum,
            estimatedEnd: estimatedEnd
        );

        return indexData;
    }

    [HttpPost]
    public async Task<ActionResult> Punch()
    {
        DateTime now = DateTime.Now;

        DbSet<PunchEntry> punchEntries = context.PunchEntries;

        PunchEntry lastEntry = punchEntries.OrderByDescending(e => e.Time).First();
        Kind lastKind = lastEntry?.Kind ?? Kind.Out;

        await punchEntries.AddAsync(new PunchEntry { Kind = lastKind == Kind.In ? Kind.Out : Kind.In, Time = now });
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<ActionResult> Holiday()
    {
        DateTime today = DateTime.Today;
        DateTime startTime = today.AddHours(8);
        DateTime endTime = startTime + DailyWorkTime;

        DbSet<PunchEntry> punchEntries = context.PunchEntries;

        punchEntries.Add(new PunchEntry { Kind = Kind.In, Time = startTime });
        punchEntries.Add(new PunchEntry { Kind = Kind.Out, Time = endTime });
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public ActionResult Week()
    {
        Week week = new() { TimeSpans = context.GetWeeklyTimeSpans(DateTime.Now) };
        return View(week);
    }

    // public ActionResult Clear()
    // {
    //     using var _db = new LiteDatabase(dbFilename);
    //     _db.DropCollection(PunchEntry.TableName);
    //     return Redirect("Index");
    // }

    public ActionResult ListAll()
    {
        return View(context.PunchEntries.OrderByDescending(x => x.Time).ToList());
    }

    public ActionResult Edit(int id)
    {
        return View(context.PunchEntries.Find(id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Edit(PunchEntry entry)
    {
        context.PunchEntries.Update(entry);
        return RedirectToAction("ListAll");
    }

    public async Task<ActionResult> Delete(int id)
    {
        PunchEntry? entryToDelete = context.PunchEntries.Find(id);
        Console.WriteLine("Found {0}", entryToDelete);
        if (entryToDelete is not null)
        {
            context.PunchEntries.Remove(entryToDelete);
            await context.SaveChangesAsync();
        }
        return RedirectToAction("ListAll");
    }

    // public FileResult DownloadDatabase()
    // {
    //     var content = System.IO.File.ReadAllBytes(dbFilename);
    //     return File(content, "application/octet-stream", "times.db");
    // }

    public ContentResult Export()
    {
        const string header = "insert into\n    punch_entries(id, time, kind)\nvalues\n";
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
