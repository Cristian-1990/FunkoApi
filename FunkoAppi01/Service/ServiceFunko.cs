using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Mappers;
using FunkoAppi01.Repository;
using FunkoAppi01.Validators;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace FunkoAppi01.Service;

/// <summary>
/// Orquesta las reglas de negocio de Funko entre el validador y el repositorio.
/// </summary>
public class ServiceFunko(
    IRepositoryFunko repository,
    IFunkoValidator validator,
    ILogger<ServiceFunko> logger,
    IOutputCacheStore outputCacheStore) : IServiceFunko
{
    private const string CacheTag = "funkos";

    public async Task<IEnumerable<FunkoDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(f => f.ToDto());

    public async Task<Result<FunkoDto, DomainError>> GetByIdAsync(long id)
    {
        if (await repository.GetByIdAsync(id) is not { } funko)
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(id));

        return Result.Success<FunkoDto, DomainError>(funko.ToDto());
    }

    public async Task<Result<FunkoDto, DomainError>> AddAsync(CreateFunkoDto request)
    {
        var validacion = validator.ValidateCreate(request);
        if (validacion.IsFailure)
            return Result.Failure<FunkoDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre))
        {
            logger.LogWarning("Intento de crear un Funko con nombre duplicado: {Nombre}", request.Nombre);
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.AlreadyExists(request.Nombre));
        }

        var creado = await repository.AddAsync(request.ToFunko());
        logger.LogInformation("Funko creado: {Id} - {Nombre}", creado.Id, creado.Nombre);
        await outputCacheStore.EvictByTagAsync(CacheTag, CancellationToken.None);
        return Result.Success<FunkoDto, DomainError>(creado.ToDto());
    }

    public async Task<Result<FunkoDto, DomainError>> UpdateAsync(long id, UpdateFunkoDto request)
    {
        var validacion = validator.ValidateUpdate(request);
        if (validacion.IsFailure)
            return Result.Failure<FunkoDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre, id))
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.AlreadyExists(request.Nombre));

        if (await repository.UpdateAsync(id, request.ToFunko()) is not { } actualizado)
        {
            logger.LogWarning("Intento de actualizar un Funko inexistente: {Id}", id);
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(id));
        }

        logger.LogInformation("Funko actualizado: {Id} - {Nombre}", actualizado.Id, actualizado.Nombre);
        await outputCacheStore.EvictByTagAsync(CacheTag, CancellationToken.None);
        return Result.Success<FunkoDto, DomainError>(actualizado.ToDto());
    }

    public async Task<Result<FunkoDto, DomainError>> UpdatePriceAsync(long id, decimal precio)
    {
        if (precio < 0)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoErrors.Validation(["El precio no puede ser negativo"]));

        if (await repository.UpdatePrecioAsync(id, precio) is not { } actualizado)
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(id));

        await outputCacheStore.EvictByTagAsync(CacheTag, CancellationToken.None);
        return Result.Success<FunkoDto, DomainError>(actualizado.ToDto());
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id)
    {
        if (!await repository.SoftDeleteAsync(id))
        {
            logger.LogWarning("Intento de borrar un Funko inexistente: {Id}", id);
            return Result.Failure<bool, DomainError>(FunkoErrors.NotFound(id));
        }

        logger.LogInformation("Funko borrado (logico): {Id}", id);
        await outputCacheStore.EvictByTagAsync(CacheTag, CancellationToken.None);
        return Result.Success<bool, DomainError>(true);
    }

    public async Task<IEnumerable<FunkoDto>> GetByNameAsync(string termino) =>
        (await repository.GetByNameAsync(termino)).Select(f => f.ToDto());

    public async Task<PagedResponse<FunkoDto>> GetPagedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await repository.GetPagedAsync(page, pageSize);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResponse<FunkoDto>(
            items.Select(f => f.ToDto()),
            new PageMetadata(page, pageSize, totalPages, total));
    }
}
