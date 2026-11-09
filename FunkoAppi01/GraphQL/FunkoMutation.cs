using FunkoAppi01.Dto;
using FunkoAppi01.Entity;
using FunkoAppi01.Service;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;

namespace FunkoAppi01.GraphQL;

/// <summary>
/// Raíz de las mutaciones GraphQL. Sin constructor a propósito: HotChocolate
/// registra este tipo como singleton, así que una dependencia scoped (como
/// IServiceFunko) inyectada por constructor quedaría "cautiva" y rompería en la
/// segunda petición. Las dependencias se inyectan por parámetro con [Service].
/// </summary>
public class FunkoMutation
{
    /// <summary>Crea un Funko. Requiere estar autenticado.</summary>
    [Authorize]
    public async Task<FunkoDto?> CrearFunko(
        CreateFunkoDto input, [Service] IServiceFunko service, [Service] ITopicEventSender sender)
    {
        var resultado = await service.AddAsync(input);
        if (resultado.IsFailure)
            return null;

        await sender.SendAsync("FunkoCreado", resultado.Value);
        return resultado.Value;
    }

    /// <summary>Actualiza un Funko existente. Requiere estar autenticado.</summary>
    [Authorize]
    public async Task<FunkoDto?> ActualizarFunko(
        long id, UpdateFunkoDto input, [Service] IServiceFunko service, [Service] ITopicEventSender sender)
    {
        var resultado = await service.UpdateAsync(id, input);
        if (resultado.IsFailure)
            return null;

        await sender.SendAsync("FunkoActualizado", resultado.Value);
        return resultado.Value;
    }

    /// <summary>Borra (lógicamente) un Funko. Requiere rol Admin.</summary>
    [Authorize(Roles = [UserRoles.Admin])]
    public async Task<bool> EliminarFunko(long id, [Service] IServiceFunko service, [Service] ITopicEventSender sender)
    {
        var resultado = await service.DeleteAsync(id);
        if (resultado.IsFailure)
            return false;

        await sender.SendAsync("FunkoEliminado", id);
        return true;
    }
}
