using FunkoAppi01.Service;

namespace FunkoAppi01.Infraestructure;

public static class ServiceConfig
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        {
            services.AddScoped<IServiceFunko, ServiceFunko>();
            services.AddScoped<IServiceCategoria, ServiceCategoria>();
            services.AddScoped<IServiceProveedor, ServiceProveedor>();
            return services;
        }
    }
}


