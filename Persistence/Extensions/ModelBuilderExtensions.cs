using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Extensions;

internal static class ModelBuilderExtensions
{
    public static void RegisterEntities<TEntityType>(this ModelBuilder modelBuilder, params Assembly[] assemblies)
    {
        var entityTypes = assemblies.SelectMany(x => x.ExportedTypes)
            .Where(x => x is { IsClass: true, IsPublic: true, IsAbstract: false } && typeof(TEntityType).IsAssignableFrom(x));

        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType);
        }
    }

    public static void ApplyRestrictDeleteBehaviour(this ModelBuilder modelBuilder)
    {
        var cascadeForeignKeys =
            modelBuilder.Model.GetEntityTypes()
                .SelectMany(c => c.GetForeignKeys())
                .Where(c => c is { IsOwnership: false, DeleteBehavior: DeleteBehavior.Cascade });

        foreach (var cascadeForeignKey in cascadeForeignKeys)
        {
            cascadeForeignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}