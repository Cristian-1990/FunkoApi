namespace FunkoAppi01.Config;

/// <summary>
/// Configuración de la caché de aplicación (en memoria en Development, Redis en Production).
/// </summary>
public class CacheOptions
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "Cache";

    /// <summary>Minutos tras los que expira una entrada si no se refresca.</summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>Minutos de expiración deslizante (se renueva con cada acceso).</summary>
    public int SlidingExpirationMinutes { get; set; } = 10;
}
