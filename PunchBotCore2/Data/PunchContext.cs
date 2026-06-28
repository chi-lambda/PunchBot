using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Models;
using PunchBotCore2.Util;

namespace PunchBotCore2.Data;

public class PunchContext(DbContextOptions<PunchContext> options, IDateTimeService dateTimeService) : DbContext(options)
{
    public DbSet<PunchEntry> PunchEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PunchEntry>().ToTable("PunchEntries");
    }
    public List<Activity> GetDailyTimeSpans(DateTime time)
    {
        DateTime startOfDay = time.Date;
        return GetWorkTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time);
    }

    public List<Activity> GetDailyBreakTimeSpans(DateTime time)
    {
        DateTime startOfDay = time.Date;
        return GetBreakTimeSpansForQuery(e => e.Time >= startOfDay && e.Time <= time);
    }

    public List<Activity> GetWeeklyTimeSpans(DateTime time)
    {
        var differenceToMonday = ((int)time.DayOfWeek + 6) % 7;
        DateTime monday = time.AddDays(-differenceToMonday).Date;
        return GetWorkTimeSpansForQuery(e => e.Time >= monday && e.Time <= time);
    }

    public List<Activity> GetMonthlyTimeSpans(DateTime time)
    {
        DateTime firstOfMonth = new(time.Year, time.Month, 1);
        return GetWorkTimeSpansForQuery(e => e.Time >= firstOfMonth && e.Time <= time);
    }

    public List<Activity> GetAllTimeSpans(DateTime until)
    {
        return GetWorkTimeSpansForQuery(e => e.Time <= until);
    }

    private List<Activity> GetWorkTimeSpansForQuery(Expression<Func<PunchEntry, bool>> query)
    {
        IQueryable<PunchEntry> entries = PunchEntries.Where(query).OrderBy(e => e.Time);
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
                    timeSpans.Add(new Activity(lastPunchInTime.Value, punch.Time));
                    lastPunchInTime = null;
                    break;
            }
        }
        if (lastPunchInTime != null)
        {
            timeSpans.Add(new Activity(lastPunchInTime.Value, default));
        }
        return timeSpans;
    }

    private List<Activity> GetBreakTimeSpansForQuery(Expression<Func<PunchEntry, bool>> query)
    {
        IQueryable<PunchEntry> entries = PunchEntries.Where(query).OrderBy(e => e.Time).Skip(1);
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
                    timeSpans.Add(new Activity(lastPunchOutTime.Value, punch.Time));
                    lastPunchOutTime = null;
                    break;
            }
        }
        if (lastPunchOutTime != null)
        {
            timeSpans.Add(new Activity(lastPunchOutTime.Value, dateTimeService.Now));
        }
        return timeSpans;
    }
    public async Task Migrate(LiteDB.LiteDatabase db)
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