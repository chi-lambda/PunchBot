using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Models;

namespace PunchBotCore2.Data;

public class PunchContext(DbContextOptions<PunchContext> options) : DbContext(options)
{
    public DbSet<PunchEntry> PunchEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PunchEntry>().ToTable("PunchEntries");
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
        return PunchEntries.Any(e => e.Time >= time.Date);
    }

    private async Task<List<Activity>> GetWorkTimeSpansForQuery(Expression<Func<PunchEntry, bool>> query, DateTime time)
    {
        IEnumerable<PunchEntry> entries = await PunchEntries
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
        IEnumerable<PunchEntry> entries = await PunchEntries
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
            if (punch.Kind== Kind.In)
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
    public async Task Migrate(LiteDB.ILiteDatabase db)
    {
        if (await PunchEntries.AnyAsync())
        {
            return; // already migrated
        }

        Console.WriteLine("Migrating DB");

        IEnumerable<PunchEntry> punchEntries = db.GetCollection<PunchEntry>(PunchEntry.TableName).FindAll();
        Console.WriteLine($"Migrating {punchEntries.Count()} entries");
        await PunchEntries.AddRangeAsync(punchEntries);
        await SaveChangesAsync();
    }

}