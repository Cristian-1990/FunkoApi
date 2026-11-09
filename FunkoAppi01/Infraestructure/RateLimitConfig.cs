using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FunkoAppi01.Infraestructure;

/// <summary>
/// Limitación de peticiones por IP con la API nativa de .NET
/// (<see cref="System.Threading.RateLimiting"/>), no con paquetes de terceros.
/// </summary>
public static class RateLimitConfig
{
    public static IServiceCollection AddRateLimitConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ip = ResolverIp(context);

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "Rate limit superado para {Ip} en {Path}",
                    ResolverIp(context.HttpContext), context.HttpContext.Request.Path);

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    status = 429,
                    title = "Demasiadas peticiones",
                    detail = "Has superado el límite de peticiones permitidas. Inténtalo de nuevo en unos segundos."
                }, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>Resuelve la IP real del cliente, incluso detrás de un proxy/reverse proxy.</summary>
    private static string ResolverIp(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) &&
            !string.IsNullOrWhiteSpace(forwardedFor))
            return forwardedFor.ToString().Split(',')[0].Trim();

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp) &&
            !string.IsNullOrWhiteSpace(realIp))
            return realIp.ToString();

        return context.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
    }
}
