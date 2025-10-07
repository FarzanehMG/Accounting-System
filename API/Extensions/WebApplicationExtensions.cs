using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();

        await db.Database.MigrateAsync();
    }
}