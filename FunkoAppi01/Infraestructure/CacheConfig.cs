using FunkoAppi01.Cache;
using FunkoAppi01.Config;

namespace FunkoAppi01.Infraestructure;

public static class CacheConfig
{
    public static IServiceCollection AddCache(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        if (environment.IsDevelopment())
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
                options.Configuration = configuration["Redis:ConnectionString"]);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        return services;
    }
}
