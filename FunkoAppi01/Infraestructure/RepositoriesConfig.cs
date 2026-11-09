using FunkoAppi01.Config;
using FunkoAppi01.Repository;
using FunkoAppi01.Repository.Mongo;
using MongoDB.Driver;

namespace FunkoAppi01.Infraestructure;

public static class RepositoriesConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConfig = new DatabaseConfig();
        configuration.GetSection(DatabaseConfig.SectionName).Bind(databaseConfig);

        // Scoped: comparte el mismo AppDbContext (tambien Scoped) durante toda la peticion.
        services.AddScoped<IRepositoryFunko, RepositoryFunko>();
        services.AddScoped<IRepositoryProveedor, RepositoryProveedor>();

        if (databaseConfig.CategoriaProvider.Equals("Mongo", StringComparison.OrdinalIgnoreCase))
        {
            var mongoConfig = new MongoConfig();
            configuration.GetSection(MongoConfig.SectionName).Bind(mongoConfig);

            services.AddSingleton<IMongoClient>(new MongoClient(mongoConfig.ConnectionString));
            services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoConfig.Database));
            services.AddScoped<IRepositoryCategoria, RepositoryCategoriaMongo>();
        }
        else
        {
            services.AddScoped<IRepositoryCategoria, RepositoryCategoria>();
        }

        return services;
    }
}
