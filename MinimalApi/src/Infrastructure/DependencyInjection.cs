using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                x => 
                {
                    x.UseNetTopologySuite();
                    x.MigrationsAssembly("Infrastructure");
                }));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ITerrainRepository, TerrainRepository>();
        services.AddScoped<ITopoExport3DModelingRepository, TopoExport3DModelingRepository>();
        services.AddScoped<IDXFLayerRepository, DXFLayerRepository>();
        services.AddScoped<IDXFFeatureRepository, DXFFeatureRepository>();
        services.AddScoped<IGpkgFileRepository, GpkgFileRepository>();
        services.AddScoped<IGpkgLayerRepository, GpkgLayerRepository>();
        services.AddScoped<IGpkgFeatureRepository, GpkgFeatureRepository>();

        return services;
    }
}
