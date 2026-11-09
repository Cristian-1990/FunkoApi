using FunkoAppi01.Models;

namespace FunkoAppi01.Repository;

public interface IRepositoryFunko
{
    Task<IReadOnlyList<Funko>> GetAllAsync();
    Task<Funko?> GetByIdAsync(long id);
    Task<Funko> AddAsync(Funko funko);
    Task<Funko?> UpdateAsync(long id, Funko funko);
    Task<bool> SoftDeleteAsync(long id);
    Task<Funko?> UpdatePrecioAsync(long id, decimal precio);
    Task<IReadOnlyList<Funko>> GetByNameAsync(string? nombre);
    Task<bool> ExistsByNameAsync(string nombre, long? excludeId = null);
    Task<(IReadOnlyList<Funko> Items, int Total)> GetPagedAsync(int page, int pageSize);
}
