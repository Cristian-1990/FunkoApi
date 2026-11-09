using System.Collections.Concurrent;
using FunkoAppi01.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FunkoAppi01.Cache;

/// <summary>
/// Caché en memoria del propio proceso. Se usa en Development, donde no hace
/// falta levantar Redis para trabajar.
/// </summary>
public class MemoryCacheService(IMemoryCache memoryCache, IOptions<CacheOptions> options) : ICacheService
{
    // Un semáforo por clave: evita que, si dos peticiones piden lo mismo a la vez
    // y hay un MISS, las dos ejecuten la factory (cache stampede).
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) =>
        Task.FromResult(memoryCache.TryGetValue(key, out T? value) ? value : default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        memoryCache.Set(key, value, ResolverExpiracion(expiration));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(memoryCache.TryGetValue(key, out _));

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        if (memoryCache.TryGetValue(key, out T? cached))
            return cached;

        var semaforo = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaforo.WaitAsync(ct);
        try
        {
            if (memoryCache.TryGetValue(key, out cached))
                return cached;

            var valor = await factory();
            memoryCache.Set(key, valor, ResolverExpiracion(expiration));
            return valor;
        }
        finally
        {
            semaforo.Release();
        }
    }

    private TimeSpan ResolverExpiracion(TimeSpan? expiration) =>
        expiration ?? TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes);
}
