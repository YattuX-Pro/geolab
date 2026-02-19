using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITerrainService, TerrainService>();
        services.AddScoped<ITopoExport3DModelingService, TopoExport3DModelingService>();
        return services;
    }
}
