using Application.Repositories.Common;
using Application.Services.Accounts;
using Application.Services.FinancialPeriod;
using Application.Services.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.Common;

namespace Persistence.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        // DataBase
        services.AddDbContext<AccountingDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("AccountingSystem"), builder =>
            {
                builder.EnableRetryOnFailure();
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

        // Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Accounts
        services.AddScoped<GroupAccountService>();
        services.AddScoped<GeneralLedgerAccountService>();
        services.AddScoped<SubLedgerAccountService>();
        services.AddScoped<DetailedAccountService>();

        // Financial period
        services.AddScoped<FinancialPeriodService>();

        // Journal
        services.AddScoped<JournalVoucherService>();
        services.AddScoped<JournalEntryService>();

        return services;
    }
}