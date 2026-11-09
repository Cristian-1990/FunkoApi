using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Service;

public interface IServiceCategoria
{
    Task<IEnumerable<CategoriaDto>> GetAllAsync();
    Task<Result<CategoriaDto, DomainError>> GetByIdAsync(long id);
    Task<Result<CategoriaDto, DomainError>> AddAsync(CreateCategoriaDto request);
    Task<Result<CategoriaDto, DomainError>> UpdateAsync(long id, UpdateCategoriaDto request);
    Task<Result<bool, DomainError>> DeleteAsync(long id);
    Task<IEnumerable<CategoriaDto>> GetByNameAsync(string termino);
    Task<PagedResponse<CategoriaDto>> GetPagedAsync(int page, int pageSize);
}
