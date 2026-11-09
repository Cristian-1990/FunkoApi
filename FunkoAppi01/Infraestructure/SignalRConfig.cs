namespace FunkoAppi01.Infraestructure;

public static class SignalRConfig
{
    public static IServiceCollection AddSignalRConfiguration(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        });

        return services;
    }
}
