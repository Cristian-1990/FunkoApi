using FunkoAppi01.Entity;
using FunkoAppi01.Mappers;
using FunkoAppi01.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoAppi01.Repository;

/// <summary>
/// Repositorio de Proveedor con Entity Framework Core.
/// </summary>
public class RepositoryProveedor(AppDbContext context) : IRepositoryProveedor
{
    public async Task<IReadOnlyList<Proveedor>> GetAllAsync()
    {
        var entidades = await context.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        return entidades.Select(p => p.ToProveedor()).ToList();
    }

    public async Task<Proveedor?> GetByIdAsync(long id)
    {
        var entidad = await context.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
        return entidad?.ToProveedor();
    }

    public async Task<Proveedor> AddAsync(Proveedor proveedor)
    {
        var entidad = new ProveedorEntity
        {
            Nombre = proveedor.Nombre,
            Telefono = proveedor.Telefono,
            CreatedAt = DateTime.UtcNow
        };

        context.Proveedores.Add(entidad);
        await context.SaveChangesAsync();

        return entidad.ToProveedor();
    }

    public async Task<Proveedor?> UpdateAsync(long id, Proveedor proveedor)
    {
        var entidad = await context.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
        if (entidad is null)
            return null;

        entidad.Nombre = proveedor.Nombre;
        entidad.Telefono = proveedor.Telefono;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return entidad.ToProveedor();
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var entidad = await context.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
        if (entidad is null)
            return false;

        entidad.IsDeleted = true;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Proveedor>> GetByNameAsync(string? nombre)
    {
        var texto = nombre ?? string.Empty;

        var entidades = await context.Proveedores
            .AsNoTracking()
            .Where(p => p.Nombre.Contains(texto))
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return entidades.Select(p => p.ToProveedor()).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null) =>
        await context.Proveedores.AnyAsync(p => p.Id != excludeId && p.Nombre.ToLower() == nombre.ToLower());
}
