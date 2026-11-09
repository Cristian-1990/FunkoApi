using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FunkoAppi01.Entity.Mongo;

/// <summary>
/// Documento de Categoria en MongoDB. Lleva su propio <see cref="Id"/> numérico
/// (no el ObjectId nativo de Mongo) para que el resto de la aplicación — DTOs,
/// controlador, rutas con <c>{id:long}</c> — no tenga que saber qué motor hay
/// detrás del repositorio.
/// </summary>
public class CategoriaMongoDocument
{
    [BsonId]
    public ObjectId MongoId { get; set; }

    [BsonElement("id")]
    public long Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
