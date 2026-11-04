namespace FunkoAppi01.Entity;

/// <summary>
/// Entidad de persistencia para las categorías de Funko.
/// La configuración (tabla, columnas, índices) está en AppDbContext con Fluent API.
/// </summary>
public class CategoriaEntity
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FunkoEntity> Funkos { get; set; } = [];
}
