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