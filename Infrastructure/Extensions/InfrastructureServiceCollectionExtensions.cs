using Application.Contracts.FileService.Interfaces;
using Infrastructure.FileStorageService.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFileService(this IServiceCollection services)
    {
        services.AddScoped<IFileService, DatabaseStorageService>();

        return services;
    }

}