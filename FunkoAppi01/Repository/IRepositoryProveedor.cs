using FunkoAppi01.Models;

namespace FunkoAppi01.Repository;

public interface IRepositoryProveedor
{
    Task<IReadOnlyList<Proveedor>> GetAllAsync();
    Task<Proveedor?> GetByIdAsync(long id);
    Task<Proveedor> AddAsync(Proveedor proveedor);
    Task<Proveedor?> UpdateAsync(long id, Proveedor proveedor);
    Task<bool> SoftDeleteAsync(long id);
    Task<IReadOnlyList<Proveedor>> GetByNameAsync(string? nombre);
    Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null);
    Task<(IReadOnlyList<Proveedor> Items, int Total)> GetPagedAsync(int page, int pageSize);
}
