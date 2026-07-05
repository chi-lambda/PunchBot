using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Data;
using PunchBotCore2.Util;

namespace PunchBotCore2.Tests.Mocks;

public class TestPunchContextFactory : IDbContextFactory<PunchContext>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PunchContext> _contextOptions;

    public TestPunchContextFactory()
    {
        // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
        // at the end of the test (see Dispose below).
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        // These options will be used by the context instances in this test suite, including the connection opened above.
        _contextOptions = new DbContextOptionsBuilder<PunchContext>()
            .UseSqlite(_connection)
            .Options;
        using PunchContext context = new(_contextOptions);
        context.Database.EnsureCreated();
    }
    
    public PunchContext CreateDbContext()
    {
        return new PunchContext(_contextOptions);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}