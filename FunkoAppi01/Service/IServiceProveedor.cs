using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;

namespace FunkoAppi01.Service;

public interface IServiceProveedor
{
    Task<IEnumerable<ProveedorDto>> GetAllAsync();
    Task<Result<ProveedorDto, DomainError>> GetByIdAsync(long id);
    Task<Result<ProveedorDto, DomainError>> AddAsync(CreateProveedorDto request);
    Task<Result<ProveedorDto, DomainError>> UpdateAsync(long id, UpdateProveedorDto request);
    Task<Result<bool, DomainError>> DeleteAsync(long id);
    Task<IEnumerable<ProveedorDto>> GetByNameAsync(string termino);
}
