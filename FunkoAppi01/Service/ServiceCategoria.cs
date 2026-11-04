using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Mappers;
using FunkoAppi01.Repository;
using FunkoAppi01.Validators;

namespace FunkoAppi01.Service;

/// <summary>
/// Orquesta las reglas de negocio de Categoria entre el validador y el repositorio.
/// </summary>
public class ServiceCategoria(
    IRepositoryCategoria repository,
    ICategoriaValidator validator,
    ILogger<ServiceCategoria> logger) : IServiceCategoria
{
    public async Task<IEnumerable<CategoriaDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(c => c.ToDto());

    public async Task<Result<CategoriaDto, DomainError>> GetByIdAsync(long id)
    {
        if (await repository.GetByIdAsync(id) is not { } categoria)
            return Result.Failure<CategoriaDto, DomainError>(CategoriaErrors.NotFound(id));

        return Result.Success<CategoriaDto, DomainError>(categoria.ToDto());
    }

    public async Task<Result<CategoriaDto, DomainError>> AddAsync(CreateCategoriaDto request)
    {
        var validacion = validator.ValidateCreate(request);
        if (validacion.IsFailure)
            return Result.Failure<CategoriaDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre))
        {
            logger.LogWarning("Intento de crear una Categoria con nombre duplicado: {Nombre}", request.Nombre);
            return Result.Failure<CategoriaDto, DomainError>(CategoriaErrors.AlreadyExists(request.Nombre));
        }

        var creada = await repository.AddAsync(request.ToCategoria());
        logger.LogInformation("Categoria creada: {Id} - {Nombre}", creada.Id, creada.Nombre);
        return Result.Success<CategoriaDto, DomainError>(creada.ToDto());
    }

    public async Task<Result<CategoriaDto, DomainError>> UpdateAsync(long id, UpdateCategoriaDto request)
    {
        var validacion = validator.ValidateUpdate(request);
        if (validacion.IsFailure)
            return Result.Failure<CategoriaDto, DomainError>(validacion.Error);

        if (await repository.ExistsByNameAsync(request.Nombre, id))
            return Result.Failure<CategoriaDto, DomainError>(CategoriaErrors.AlreadyExists(request.Nombre));

        if (await repository.UpdateAsync(id, request.ToCategoria()) is not { } actualizada)
        {
            logger.LogWarning("Intento de actualizar una Categoria inexistente: {Id}", id);
            return Result.Failure<CategoriaDto, DomainError>(CategoriaErrors.NotFound(id));
        }

        logger.LogInformation("Categoria actualizada: {Id} - {Nombre}", actualizada.Id, actualizada.Nombre);
        return Result.Success<CategoriaDto, DomainError>(actualizada.ToDto());
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id)
    {
        if (!await repository.SoftDeleteAsync(id))
        {
            logger.LogWarning("Intento de borrar una Categoria inexistente: {Id}", id);
            return Result.Failure<bool, DomainError>(CategoriaErrors.NotFound(id));
        }

        logger.LogInformation("Categoria borrada (logico): {Id}", id);
        return Result.Success<bool, DomainError>(true);
    }

    public async Task<IEnumerable<CategoriaDto>> GetByNameAsync(string termino) =>
        (await repository.GetByNameAsync(termino)).Select(c => c.ToDto());
}
