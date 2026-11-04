using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Mappers;
using FunkoAppi01.Repository;
using FunkoAppi01.Validators;

namespace FunkoAppi01.Service;

/// <summary>
/// Orquesta las reglas de negocio de Proveedor entre el validador y el repositorio.
/// </summary>
public class ServiceProveedor(
    IRepositoryProveedor repository,
    IProveedorValidator validator,
    ILogger<ServiceProveedor> logger) : IServiceProveedor
{
    public async Task<IEnumerable<ProveedorDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(p => p.ToDto());

    public async Task<Result<ProveedorDto, DomainError>> GetByIdAsync(long id)
    {
        if (await repository.GetByIdAsync(id) is not { } proveedor)
            return Result.Failure<ProveedorDto, DomainError>(ProveedorErrors.NotFound(id));

        return Result.Success<ProveedorDto, DomainError>(proveedor.ToDto());
    }

    public async Task<Result<ProveedorDto, DomainError>> AddAsync(CreateProveedorDto request)
    {
        var validacion = validator.ValidateCreate(request);
        if (validacion.IsFailure)
            return Result.Failure<ProveedorDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre))
        {
            logger.LogWarning("Intento de crear un Proveedor con nombre duplicado: {Nombre}", request.Nombre);
            return Result.Failure<ProveedorDto, DomainError>(ProveedorErrors.AlreadyExists(request.Nombre));
        }

        var creado = await repository.AddAsync(request.ToProveedor());
        logger.LogInformation("Proveedor creado: {Id} - {Nombre}", creado.Id, creado.Nombre);
        return Result.Success<ProveedorDto, DomainError>(creado.ToDto());
    }

    public async Task<Result<ProveedorDto, DomainError>> UpdateAsync(long id, UpdateProveedorDto request)
    {
        var validacion = validator.ValidateUpdate(request);
        if (validacion.IsFailure)
            return Result.Failure<ProveedorDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre, id))
            return Result.Failure<ProveedorDto, DomainError>(ProveedorErrors.AlreadyExists(request.Nombre));

        if (await repository.UpdateAsync(id, request.ToProveedor()) is not { } actualizado)
        {
            logger.LogWarning("Intento de actualizar un Proveedor inexistente: {Id}", id);
            return Result.Failure<ProveedorDto, DomainError>(ProveedorErrors.NotFound(id));
        }

        logger.LogInformation("Proveedor actualizado: {Id} - {Nombre}", actualizado.Id, actualizado.Nombre);
        return Result.Success<ProveedorDto, DomainError>(actualizado.ToDto());
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id)
    {
        if (!await repository.SoftDeleteAsync(id))
        {
            logger.LogWarning("Intento de borrar un Proveedor inexistente: {Id}", id);
            return Result.Failure<bool, DomainError>(ProveedorErrors.NotFound(id));
        }

        logger.LogInformation("Proveedor borrado (logico): {Id}", id);
        return Result.Success<bool, DomainError>(true);
    }

    public async Task<IEnumerable<ProveedorDto>> GetByNameAsync(string termino) =>
        (await repository.GetByNameAsync(termino)).Select(p => p.ToDto());
}
