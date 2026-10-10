using Application.Contracts.JournalEntryService;
using Application.Contracts.JournalVoucherService;
using Application.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.Common;
using Persistence.Services.JournalEntryService;
using Persistence.Services.JournalVoucherService;

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
                //builder.EnableRetryOnFailure();
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

        services.AddScoped<IJournalVoucherNumberGenerator, JournalVoucherNumberGenerator>();
        services.AddScoped<IJournalEntryNumberGenerator, JournalEntryNumberGenerator>();

        // Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}