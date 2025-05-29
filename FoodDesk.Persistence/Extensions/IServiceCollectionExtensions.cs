using FoodDesk.Application.Interfaces.Repositories;
using FoodDesk.Persistence.Context;
using FoodDesk.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDesk.Persistence.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext(configuration);
        services.AddRepositories();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseNpgsql(connectionString,
               builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services
            .AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork))
            .AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    }
}
