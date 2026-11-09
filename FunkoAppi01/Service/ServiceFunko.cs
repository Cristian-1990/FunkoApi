using CSharpFunctionalExtensions;
using FunkoAppi01.Cache;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Mappers;
using FunkoAppi01.Repository;
using FunkoAppi01.Service.Email;
using FunkoAppi01.Validators;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace FunkoAppi01.Service;

/// <summary>
/// Orquesta las reglas de negocio de Funko entre el validador y el repositorio.
/// Las lecturas pasan por una caché de aplicación (cache-aside); las escrituras
/// la invalidan, igual que invalidan el OutputCache HTTP, y notifican por correo.
/// </summary>
public class ServiceFunko(
    IRepositoryFunko repository,
    IFunkoValidator validator,
    ILogger<ServiceFunko> logger,
    IOutputCacheStore outputCacheStore,
    ICacheService cache,
    IEmailService emailService) : IServiceFunko
{
    private const string CacheTag = "funkos";
    private const string AllCacheKey = "funkos:all";
    private const string NotificacionesTo = "notificaciones@funkoapp.local";

    public async Task<IEnumerable<FunkoDto>> GetAllAsync() =>
        (await cache.GetOrSetAsync(AllCacheKey, async () =>
            (await repository.GetAllAsync()).Select(f => f.ToDto()).ToList()))!;

    public async Task<Result<FunkoDto, DomainError>> GetByIdAsync(long id)
    {
        var funko = await cache.GetOrSetAsync($"funko:{id}", async () =>
            (await repository.GetByIdAsync(id))?.ToDto());

        if (funko is null)
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(id));

        return Result.Success<FunkoDto, DomainError>(funko);
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
        await InvalidarCacheAsync();
        _ = EnviarNotificacionCreadoAsync(creado.Nombre, creado.Precio);
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
        await InvalidarCacheAsync(id);
        return Result.Success<FunkoDto, DomainError>(actualizado.ToDto());
    }

    public async Task<Result<FunkoDto, DomainError>> UpdatePriceAsync(long id, decimal precio)
    {
        if (precio < 0)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoErrors.Validation(["El precio no puede ser negativo"]));

        if (await repository.UpdatePrecioAsync(id, precio) is not { } actualizado)
            return Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(id));

        await InvalidarCacheAsync(id);
        return Result.Success<FunkoDto, DomainError>(actualizado.ToDto());
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id)
    {
        var nombreBorrado = (await repository.GetByIdAsync(id))?.Nombre;

        if (!await repository.SoftDeleteAsync(id))
        {
            logger.LogWarning("Intento de borrar un Funko inexistente: {Id}", id);
            return Result.Failure<bool, DomainError>(FunkoErrors.NotFound(id));
        }

        logger.LogInformation("Funko borrado (logico): {Id}", id);
        await InvalidarCacheAsync(id);
        _ = EnviarNotificacionEliminadoAsync(nombreBorrado ?? $"id {id}");
        return Result.Success<bool, DomainError>(true);
    }

    /// <summary>Invalida tanto el OutputCache HTTP como la caché de aplicación tras una escritura.</summary>
    private async Task InvalidarCacheAsync(long? id = null)
    {
        await outputCacheStore.EvictByTagAsync(CacheTag, CancellationToken.None);
        await cache.RemoveAsync(AllCacheKey);
        if (id is not null)
            await cache.RemoveAsync($"funko:{id}");
    }

    /// <summary>
    /// Notifica por correo que se ha creado un Funko. Fire-and-forget: un fallo de
    /// email nunca debe tumbar la petición que ya se completó con éxito.
    /// </summary>
    private async Task EnviarNotificacionCreadoAsync(string nombre, decimal precio)
    {
        try
        {
            await emailService.SendEmailAsync(new EmailMessage
            {
                To = NotificacionesTo,
                Subject = $"Funko creado: {nombre}",
                Body = EmailTemplates.CreateBase("Nuevo Funko", EmailTemplates.FunkoCreado(nombre, precio)),
                IsHtml = true
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error enviando email de Funko creado: {Nombre}", nombre);
        }
    }

    /// <summary>Notifica por correo que se ha borrado un Funko. Fire-and-forget, igual que al crear.</summary>
    private async Task EnviarNotificacionEliminadoAsync(string nombre)
    {
        try
        {
            await emailService.SendEmailAsync(new EmailMessage
            {
                To = NotificacionesTo,
                Subject = $"Funko eliminado: {nombre}",
                Body = EmailTemplates.CreateBase("Funko eliminado", EmailTemplates.FunkoEliminado(nombre)),
                IsHtml = true
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error enviando email de Funko eliminado: {Nombre}", nombre);
        }
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
