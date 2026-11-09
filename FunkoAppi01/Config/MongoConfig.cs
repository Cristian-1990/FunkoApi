namespace FunkoAppi01.Config;

/// <summary>
/// Configuración de la conexión a MongoDB (solo se usa si
/// <c>DatabaseSettings:CategoriaProvider</c> vale "Mongo").
/// </summary>
public class MongoConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "funkoapp";
}
