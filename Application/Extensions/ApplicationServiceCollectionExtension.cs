using Application.Features.Accounts.Commands.GroupAccount;
using Application.Services.Accounts;
using Application.Services.FinancialPeriod;
using Application.Services.Journal;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Handler
        var handlerTypes = typeof(CreateGroupAccountCommandHandler).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Handler"));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

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