namespace FunkoAppi01.Models;

public record Funko
{
    public long      Id          { get; set; }
    public string    Nombre      { get; set; } = string.Empty;
    public decimal   Precio      { get; set; }
    public int       Stock       { get; set; }
    public string    Categoria   { get; set; } = string.Empty;
    public string?   Proveedor   { get; set; }
    public bool      IsDeleted   { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
}