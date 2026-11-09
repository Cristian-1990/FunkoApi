using FunkoAppi01.Entity.Mongo;
using FunkoAppi01.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FunkoAppi01.Repository.Mongo;

/// <summary>
/// Implementación alternativa de <see cref="IRepositoryCategoria"/> sobre MongoDB,
/// en vez de EF Core. Misma interfaz, mismo contrato — el servicio y el
/// controlador no saben (ni les importa) qué motor hay detrás.
/// </summary>
public class RepositoryCategoriaMongo : IRepositoryCategoria
{
    private readonly IMongoCollection<CategoriaMongoDocument> _categorias;
    private readonly IMongoCollection<BsonDocument> _contadores;

    public RepositoryCategoriaMongo(IMongoDatabase database)
    {
        _categorias = database.GetCollection<CategoriaMongoDocument>("categorias");
        _contadores = database.GetCollection<BsonDocument>("contadores");

        _categorias.Indexes.CreateOne(new CreateIndexModel<CategoriaMongoDocument>(
            Builders<CategoriaMongoDocument>.IndexKeys.Ascending(c => c.Id), new CreateIndexOptions { Unique = true }));
    }

    public async Task<IReadOnlyList<Categoria>> GetAllAsync()
    {
        var documentos = await _categorias.Find(c => !c.IsDeleted).SortBy(c => c.Id).ToListAsync();
        return documentos.Select(ToCategoria).ToList();
    }

    public async Task<Categoria?> GetByIdAsync(long id)
    {
        var documento = await _categorias.Find(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync();
        return documento is null ? null : ToCategoria(documento);
    }

    public async Task<Categoria> AddAsync(Categoria categoria)
    {
        var documento = new CategoriaMongoDocument
        {
            Id = await SiguienteIdAsync(),
            Nombre = categoria.Nombre,
            CreatedAt = DateTime.UtcNow
        };

        await _categorias.InsertOneAsync(documento);
        return ToCategoria(documento);
    }

    public async Task<Categoria?> UpdateAsync(long id, Categoria categoria)
    {
        var update = Builders<CategoriaMongoDocument>.Update
            .Set(c => c.Nombre, categoria.Nombre)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var resultado = await _categorias.FindOneAndUpdateAsync(
            c => c.Id == id && !c.IsDeleted, update,
            new FindOneAndUpdateOptions<CategoriaMongoDocument> { ReturnDocument = ReturnDocument.After });

        return resultado is null ? null : ToCategoria(resultado);
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var update = Builders<CategoriaMongoDocument>.Update
            .Set(c => c.IsDeleted, true)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var resultado = await _categorias.UpdateOneAsync(c => c.Id == id && !c.IsDeleted, update);
        return resultado.ModifiedCount > 0;
    }

    public async Task<IReadOnlyList<Categoria>> GetByNameAsync(string? nombre)
    {
        var texto = nombre ?? string.Empty;
        var filtro = Builders<CategoriaMongoDocument>.Filter.Regex(
            c => c.Nombre, new BsonRegularExpression(texto, "i"));

        var documentos = await _categorias.Find(filtro & Builders<CategoriaMongoDocument>.Filter.Eq(c => c.IsDeleted, false))
            .SortBy(c => c.Nombre).ToListAsync();

        return documentos.Select(ToCategoria).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null)
    {
        var filtro = Builders<CategoriaMongoDocument>.Filter.Eq(c => c.IsDeleted, false) &
                     Builders<CategoriaMongoDocument>.Filter.Regex(
                         c => c.Nombre, new BsonRegularExpression($"^{nombre}$", "i"));

        if (excludeId is not null)
            filtro &= Builders<CategoriaMongoDocument>.Filter.Ne(c => c.Id, excludeId.Value);

        return await _categorias.Find(filtro).AnyAsync();
    }

    public async Task<(IReadOnlyList<Categoria> Items, int Total)> GetPagedAsync(int page, int pageSize)
    {
        var filtro = Builders<CategoriaMongoDocument>.Filter.Eq(c => c.IsDeleted, false);

        var total = (int)await _categorias.CountDocumentsAsync(filtro);
        var documentos = await _categorias.Find(filtro).SortBy(c => c.Id)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return (documentos.Select(ToCategoria).ToList(), total);
    }

    /// <summary>
    /// MongoDB no tiene autoincremento nativo: un contador propio con
    /// <c>FindOneAndUpdate $inc</c> da el siguiente id de forma atómica.
    /// </summary>
    private async Task<long> SiguienteIdAsync()
    {
        var filtro = Builders<BsonDocument>.Filter.Eq("_id", "categorias");
        var update = Builders<BsonDocument>.Update.Inc("seq", 1L);
        var opciones = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var documento = await _contadores.FindOneAndUpdateAsync(filtro, update, opciones);
        return documento["seq"].AsInt64;
    }

    private static Categoria ToCategoria(CategoriaMongoDocument documento) => new()
    {
        Id = documento.Id,
        Nombre = documento.Nombre,
        IsDeleted = documento.IsDeleted,
        CreatedAt = documento.CreatedAt,
        UpdatedAt = documento.UpdatedAt
    };
}
