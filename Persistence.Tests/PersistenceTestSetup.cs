using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;
using Testcontainers.MsSql;

namespace Persistence.Tests;

public class PersistenceTestSetup : IAsyncLifetime
{
    public UnitOfWork UnitOfWork { get; private set; } = null!;
    private AccountingDbContext _dbContext = null!;

    private MsSqlContainer? _sqlContainer;

    private bool _useDocker = true;

    public async Task InitializeAsync()
    {
        try
        {
            // Build container here (not in constructor!)
            _sqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                .WithPassword("your_strong_password_123")
                .Build();

            // Try to start SQL Server in Docker
            await _sqlContainer.StartAsync();

            var dbOptionBuilder = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseSqlServer(_sqlContainer.GetConnectionString());

            _dbContext = new AccountingDbContext(dbOptionBuilder.Options);

            await _dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Docker not available, falling back to LocalDB. Error: {ex.Message}");
            _useDocker = false;

            // Use SQL Server LocalDB instead
            var dbOptionBuilder = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AccountingTestDb;Trusted_Connection=True;");

            _dbContext = new AccountingDbContext(dbOptionBuilder.Options);

            // Clean & migrate so you always start fresh
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
        }

        UnitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task DisposeAsync()
    {
        if (_useDocker && _sqlContainer is not null)
        {
            await _sqlContainer.StopAsync();
        }

        await _dbContext.DisposeAsync();
    }
}