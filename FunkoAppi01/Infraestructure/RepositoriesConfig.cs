using FunkoAppi01.Repository;

namespace FunkoAppi01.Infraestructure;

public static class RepositoriesConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Scoped: comparte el mismo AppDbContext (tambien Scoped) durante toda la peticion.
        services.AddScoped<IRepositoryFunko, RepositoryFunko>();
        services.AddScoped<IRepositoryCategoria, RepositoryCategoria>();
        services.AddScoped<IRepositoryProveedor, RepositoryProveedor>();
        return services;
    }
}