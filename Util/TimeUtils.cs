using System.Linq.Expressions;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Data;
using PunchBotCore2.Models;

namespace PunchBotCore2.Util;

public static class TimeUtils
{
    public static List<Activity> GetDailyTimeSpans(this PunchContext context, DateTime time)
    {
        DateTime startOfDay = time.Date;
        return context.GetWorkTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time);
    }

    public static List<Activity> GetDailyBreakTimeSpans(this PunchContext context, DateTime time)
    {
        DateTime startOfDay = time.Date;
        return context.GetBreakTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time);
    }

    public static List<Activity> GetWeeklyTimeSpans(this PunchContext context, DateTime time)
    {
        var differenceToMonday = ((int)time.DayOfWeek + 6) % 7;
        DateTime monday = time.AddDays(-differenceToMonday).Date;
        return context.GetWorkTimeSpansForQuery(e => e.Time >= monday && e.Time <= time);
    }

    public static List<Activity> GetMonthlyTimeSpans(this PunchContext context, DateTime time)
    {
        DateTime firstOfMonth = new(time.Year, time.Month, 1);
        return context.GetWorkTimeSpansForQuery(e => e.Time >= firstOfMonth && e.Time<=time);
    }

    public static List<Activity> GetAllTimeSpans(this PunchContext context, DateTime until)
    {
        return context.GetWorkTimeSpansForQuery(e => e.Time <= until);
    }

    private static List<Activity> GetWorkTimeSpansForQuery(this PunchContext context, Expression<Func<PunchEntry, bool>> query)
    {
        IQueryable<PunchEntry> entries = context.PunchEntries.Where(query).OrderBy(e => e.Time);
        DateTime? lastPunchInTime = null;
        List<Activity> timeSpans = [];
        foreach (PunchEntry punch in entries)
        {
            switch (punch.Kind)
            {
                case Kind.In:
                    lastPunchInTime = punch.Time;
                    break;
                case Kind.Out:
                    if (lastPunchInTime == null) { continue; }
                    timeSpans.Add(new Activity { Start = lastPunchInTime.Value, End = punch.Time });
                    lastPunchInTime = null;
                    break;
            }
        }
        if (lastPunchInTime != null)
        {
            timeSpans.Add(new Activity { Start = lastPunchInTime.Value });
        }
        return timeSpans;
    }

    private static List<Activity> GetBreakTimeSpansForQuery(this PunchContext context, Expression<Func<PunchEntry, bool>> query)
    {
        IQueryable<PunchEntry> entries = context.PunchEntries.Where(query).OrderBy(e => e.Time).Skip(1);
        DateTime? lastPunchOutTime = null;
        List<Activity> timeSpans = [];
        foreach (PunchEntry punch in entries)
        {
            switch (punch.Kind)
            {
                case Kind.Out:
                    lastPunchOutTime = punch.Time;
                    break;
                case Kind.In:
                    if (lastPunchOutTime == null) { continue; }
                    timeSpans.Add(new Activity { Start = lastPunchOutTime.Value, End = punch.Time });
                    lastPunchOutTime = null;
                    break;
            }
        }
        if (lastPunchOutTime != null)
        {
            timeSpans.Add(new Activity { Start = lastPunchOutTime.Value });
        }
        return timeSpans;
    }

    public static async Task Migrate(this LiteDatabase db, PunchContext context)
    {
        if (await context.PunchEntries.AnyAsync())
        {
            return; // already migrated
        }

        Console.WriteLine("Migrating DB");

        IEnumerable<PunchEntry> punchEntries = db.GetCollection<PunchEntry>(PunchEntry.TableName).FindAll();
        Console.WriteLine($"Migrating {punchEntries.Count()} entries");
        await context.PunchEntries.AddRangeAsync(punchEntries);
        await context.SaveChangesAsync();
    }
}

