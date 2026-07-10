using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Data;
using PunchBotCore2.Models;

namespace PunchBotCore2.Util;

public class DataAggregator(PunchContext context, IDateTimeService dateTimeService)
{
    internal static readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
    internal static readonly TimeSpan MinBreakDuration = TimeSpan.FromMinutes(30);

    internal async Task<IndexData> GetIndexData()
    {
        DateTime now = dateTimeService.Now;
        DbSet<PunchEntry> punchEntries = context.PunchEntries;
        PunchEntry? lastEntry = await punchEntries.OrderByDescending(e => e.Time).FirstOrDefaultAsync();
        TimeSpan totalSum = (await GetAllTimeSpans(now)).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);

        var numDays = punchEntries.GroupBy(x => x.Time.Date).Count();
        TimeSpan remainingTime = numDays * DailyWorkTime - totalSum;
        if (remainingTime <= TimeSpan.Zero && !HasWorkedToday(now))
        {
            remainingTime = DailyWorkTime + remainingTime;
        }
        TimeSpan daySum = (await GetDailyTimeSpans(now)).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        TimeSpan dayBreakSum = (await GetDailyBreakTimeSpans(now)).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        DateTime estimatedEnd = dayBreakSum >= MinBreakDuration ? now + remainingTime : now + remainingTime + MinBreakDuration - dayBreakSum;
        IndexData indexData = new(
            WeekSum: (await GetWeeklyTimeSpans(now)).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
            DaySum: daySum,
            LastEntry: lastEntry,
            RemainingTime: remainingTime,
            DayBreakSum: dayBreakSum,
            EstimatedEnd: estimatedEnd
        );

        return indexData;
    }

    public async Task<List<Activity>> GetDailyTimeSpans(DateTime time)
    {
        DateTime startOfDay = time.Date;
        return await GetWorkTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time, time);
    }

    public async Task<List<Activity>> GetDailyBreakTimeSpans(DateTime time)
    {
        DateTime startOfDay = time.Date;
        return await GetBreakTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time, time);
    }

    public async Task<List<Activity>> GetWeeklyTimeSpans(DateTime time)
    {
        var differenceToMonday = ((int)time.DayOfWeek + 6) % 7;
        DateTime monday = time.AddDays(-differenceToMonday).Date;
        return await GetWorkTimeSpansForQuery(e => e.Time >= monday && e.Time <= time, time);
    }

    public async Task<List<Activity>> GetAllTimeSpans(DateTime time)
    {
        return await GetWorkTimeSpansForQuery(e => e.Time <= time, time);
    }

    public bool HasWorkedToday(DateTime time)
    {
        return context.PunchEntries.Any(e => e.Time >= time.Date);
    }

    private async Task<List<Activity>> GetWorkTimeSpansForQuery(Expression<Func<PunchEntry, bool>> query, DateTime time)
    {
        IEnumerable<PunchEntry> entries = await context.PunchEntries
            .Where(query)
            .OrderBy(e => e.Time)
            .ToListAsync();
        if (!entries.Any())
        {
            return [];
        }

        PunchEntry lastPunch = entries.First();
        List<Activity> timeSpans = [];
        foreach (PunchEntry punch in entries)
        {
            if (punch.Kind == Kind.Out)
            {
                timeSpans.Add(new Activity(lastPunch.Time, punch.Time));
            }

            lastPunch = punch;
        }

        if (lastPunch.Kind == Kind.In)
        {
            timeSpans.Add(new Activity(lastPunch.Time, time));
        }
        return timeSpans;
    }

    private async Task<List<Activity>> GetBreakTimeSpansForQuery(Expression<Func<PunchEntry, bool>> query, DateTime time)
    {
        IEnumerable<PunchEntry> entries = await context.PunchEntries
            .Where(query)
            .OrderBy(e => e.Time)
            .Skip(1)
            .ToListAsync();
        if (!entries.Any())
        {
            return [];
        }

        PunchEntry lastPunch = entries.First();
        List<Activity> timeSpans = [];
        foreach (PunchEntry punch in entries)
        {
            if (punch.Kind == Kind.In)
            {
                timeSpans.Add(new Activity(lastPunch.Time, punch.Time));
            }

            lastPunch = punch;
        }

        TimeSpan breakSum = timeSpans.Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
        if (lastPunch.Kind == Kind.Out && breakSum.TotalMinutes < 30)
        {
            timeSpans.Add(new Activity(lastPunch.Time, time));
        }

        return timeSpans;
    }
}