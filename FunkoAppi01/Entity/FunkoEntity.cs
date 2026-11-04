namespace FunkoAppi01.Entity;

/// <summary>
/// Entidad de persistencia para Funko. Relación Uno a Muchos con CategoriaEntity.
/// La configuración (tabla, columnas, relaciones) está en AppDbContext con Fluent API.
/// </summary>
public class FunkoEntity
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public long CategoriaId { get; set; }
    public CategoriaEntity Categoria { get; set; } = null!;
    public long? ProveedorId { get; set; }
    public ProveedorEntity? Proveedor { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
