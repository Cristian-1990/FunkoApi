using FunkoAppi01.Entity;
using FunkoAppi01.Mappers;
using FunkoAppi01.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoAppi01.Repository;

/// <summary>
/// Repositorio de Funko con Entity Framework Core.
/// Funciona igual con SQLite (perfil Dev) y con PostgreSQL (perfil Prod):
/// el proveedor se elige al registrar el AppDbContext.
/// </summary>
public class RepositoryFunko(AppDbContext context) : IRepositoryFunko
{
    public async Task<IReadOnlyList<Funko>> GetAllAsync()
    {
        var entidades = await context.Funkos
            .Include(f => f.Categoria)
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .ToListAsync();

        return entidades.Select(f => f.ToFunko()).ToList();
    }

    public async Task<Funko?> GetByIdAsync(long id)
    {
        var entidad = await BuscarPorId(id);
        return entidad?.ToFunko();
    }

    public async Task<Funko> AddAsync(Funko funko)
    {
        var categoria = await ObtenerOCrearCategoria(funko.Categoria);

        var entidad = new FunkoEntity
        {
            Nombre = funko.Nombre,
            Precio = funko.Precio,
            Stock = funko.Stock,
            CategoriaId = categoria.Id,
            Categoria = categoria,
            CreatedAt = DateTime.UtcNow
        };

        context.Funkos.Add(entidad);
        await context.SaveChangesAsync();

        return entidad.ToFunko();
    }

    public async Task<Funko?> UpdateAsync(long id, Funko funko)
    {
        var entidad = await BuscarPorId(id);
        if (entidad is null)
            return null;

        var categoria = await ObtenerOCrearCategoria(funko.Categoria);

        entidad.Nombre = funko.Nombre;
        entidad.Precio = funko.Precio;
        entidad.Stock = funko.Stock;
        entidad.CategoriaId = categoria.Id;
        entidad.Categoria = categoria;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return entidad.ToFunko();
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        var entidad = await BuscarPorId(id);
        if (entidad is null)
            return false;

        entidad.IsDeleted = true;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Funko?> UpdatePrecioAsync(long id, decimal precio)
    {
        var entidad = await BuscarPorId(id);
        if (entidad is null)
            return null;

        entidad.Precio = precio;
        entidad.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return entidad.ToFunko();
    }

    public async Task<IReadOnlyList<Funko>> GetByNameAsync(string? nombre)
    {
        var texto = nombre ?? string.Empty;

        var entidades = await context.Funkos
            .Include(f => f.Categoria)
            .AsNoTracking()
            .Where(f => f.Nombre.Contains(texto))
            .OrderBy(f => f.Nombre)
            .ToListAsync();

        return entidades.Select(f => f.ToFunko()).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null) =>
        await context.Funkos.AnyAsync(f => f.Id != excludeId && f.Nombre.ToLower() == nombre.ToLower());

    public async Task<(IReadOnlyList<Funko> Items, int Total)> GetPagedAsync(int page, int pageSize)
    {
        var query = context.Funkos.Include(f => f.Categoria).AsNoTracking().OrderBy(f => f.Id);

        var total = await query.CountAsync();
        var entidades = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (entidades.Select(f => f.ToFunko()).ToList(), total);
    }

    private async Task<FunkoEntity?> BuscarPorId(long id) =>
        await context.Funkos.Include(f => f.Categoria).FirstOrDefaultAsync(f => f.Id == id);

    private async Task<CategoriaEntity> ObtenerOCrearCategoria(string nombre)
    {
        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());

        if (categoria is not null)
            return categoria;

        categoria = new CategoriaEntity { Nombre = nombre };
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        return categoria;
    }
}
