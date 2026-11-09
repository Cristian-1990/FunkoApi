using System.Collections.Concurrent;
using System.Text.Json;
using FunkoAppi01.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace FunkoAppi01.Cache;

/// <summary>
/// Caché distribuida con Redis. Se usa en Production, para que la caché sobreviva
/// a reinicios del contenedor y se pueda compartir entre varias réplicas de la API.
/// </summary>
public class RedisCacheService(IDistributedCache distributedCache, IOptions<CacheOptions> options) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var json = await distributedCache.GetStringAsync(key, ct);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ResolverExpiracion(expiration),
            SlidingExpiration = TimeSpan.FromMinutes(options.Value.SlidingExpirationMinutes)
        }, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default) =>
        await distributedCache.RemoveAsync(key, ct);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        await distributedCache.GetStringAsync(key, ct) is not null;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null)
            return cached;

        var semaforo = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaforo.WaitAsync(ct);
        try
        {
            cached = await GetAsync<T>(key, ct);
            if (cached is not null)
                return cached;

            var valor = await factory();
            await SetAsync(key, valor, expiration, ct);
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
