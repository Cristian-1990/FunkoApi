using FunkoAppi01.Entity;
using FunkoAppi01.Mappers;
using FunkoAppi01.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoAppi01.Repository;

/// <summary>
/// Repositorio de Categoria con Entity Framework Core.
/// </summary>
public class RepositoryCategoria(AppDbContext context) : IRepositoryCategoria
{
    public async Task<IReadOnlyList<Categoria>> GetAllAsync()
    {
        var entidades = await context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();

        return entidades.Select(c => c.ToCategoria()).ToList();
    }

    public async Task<Categoria?> GetByIdAsync(long id)
    {
        var entidad = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        return entidad?.ToCategoria();
    }

    public async Task<Categoria> AddAsync(Categoria categoria)
    {
        var entidad = new CategoriaEntity
        {
            Nombre = categoria.Nombre,
            CreatedAt = DateTime.UtcNow
        };

        context.Categorias.Add(entidad);
        await context.SaveChangesAsync();

        return entidad.ToCategoria();
    }

    public async Task<Categoria?> UpdateAsync(long id, Categoria categoria)
    {
        var entidad = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (entidad is null)
            return null;

        entidad.Nombre = categoria.Nombre;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return entidad.ToCategoria();
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var entidad = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (entidad is null)
            return false;

        entidad.IsDeleted = true;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Categoria>> GetByNameAsync(string? nombre)
    {
        var texto = nombre ?? string.Empty;

        var entidades = await context.Categorias
            .AsNoTracking()
            .Where(c => c.Nombre.Contains(texto))
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return entidades.Select(c => c.ToCategoria()).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null) =>
        await context.Categorias.AnyAsync(c => c.Id != excludeId && c.Nombre.ToLower() == nombre.ToLower());

    public async Task<(IReadOnlyList<Categoria> Items, int Total)> GetPagedAsync(int page, int pageSize)
    {
        var query = context.Categorias.AsNoTracking().OrderBy(c => c.Id);

        var total = await query.CountAsync();
        var entidades = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (entidades.Select(c => c.ToCategoria()).ToList(), total);
    }
}
