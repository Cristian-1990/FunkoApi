namespace FunkoAppi01.Infraestructure;

/// <summary>
/// Caché de respuestas HTTP (OutputCache nativo de ASP.NET Core) para los endpoints
/// de lectura pública. Se invalida por tag desde el servicio tras cada escritura.
/// </summary>
public static class OutputCacheConfig
{
    public static IServiceCollection AddOutputCacheConfig(this IServiceCollection services)
    {
        services.AddOutputCache();
        return services;
    }

    public static WebApplication UseOutputCacheConfig(this WebApplication app)
    {
        app.UseOutputCache();
        return app;
    }
}
