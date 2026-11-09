using FunkoAppi01.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FunkoAppi01.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real de Funkos. Los clientes se
/// agrupan por rol (admins) y, opcionalmente, por categoría (quien solo quiere
/// enterarse de los cambios de "Pokemon", por ejemplo).
/// </summary>
[Authorize]
public class FunkosHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var isAdmin = Context.User?.IsInRole(UserRoles.Admin) ?? false;

        if (isAdmin)
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }

    /// <summary>Se suscribe a las notificaciones de una categoría concreta.</summary>
    public async Task JoinCategoriaGroup(long categoriaId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"categoria-{categoriaId}");

    /// <summary>Cancela la suscripción a una categoría.</summary>
    public async Task LeaveCategoriaGroup(long categoriaId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"categoria-{categoriaId}");

    /// <summary>Comprobación de vida de la conexión.</summary>
    public async Task Ping() => await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
}
