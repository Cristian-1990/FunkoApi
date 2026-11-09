namespace FunkoAppi01.Cache;

/// <summary>
/// Abstracción de caché de aplicación. El servicio de negocio nunca habla
/// directamente con <c>IMemoryCache</c>/<c>IDistributedCache</c>, siempre con esto
/// — así la implementación (memoria o Redis) es intercambiable sin tocar el servicio.
/// </summary>
public interface ICacheService
{
    /// <summary>Lee un valor de la caché. Null si no existe o ha expirado.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Guarda un valor en la caché con la expiración indicada (o la de por defecto).</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>Elimina una entrada de la caché.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Comprueba si una clave existe en la caché.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>Lee de la caché; si no existe, ejecuta <paramref name="factory"/>, guarda el resultado y lo devuelve.</summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
}
