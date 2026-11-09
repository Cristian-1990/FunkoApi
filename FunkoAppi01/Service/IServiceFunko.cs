using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Service;

public interface IServiceFunko
{
    Task<IEnumerable<FunkoDto>> GetAllAsync();
    Task<Result<FunkoDto, DomainError>> GetByIdAsync(long id);
    Task<Result<FunkoDto, DomainError>> AddAsync(CreateFunkoDto request);
    Task<Result<FunkoDto, DomainError>> UpdateAsync(long id, UpdateFunkoDto request);
    Task<Result<FunkoDto, DomainError>> UpdatePriceAsync(long id, decimal precio);
    Task<Result<bool, DomainError>> DeleteAsync(long id);
    Task<IEnumerable<FunkoDto>> GetByNameAsync(string termino);
    Task<PagedResponse<FunkoDto>> GetPagedAsync(int page, int pageSize);
}
