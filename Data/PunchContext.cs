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
}