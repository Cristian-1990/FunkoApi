using System.Security.Claims;
using FluentAssertions;
using FunkoAppi01.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Hubs;

/// <summary>
/// Tests de FunkosHub. Context/Groups/Clients son propiedades públicas de Hub
/// (normalmente las rellena SignalR en producción) — aquí se sustituyen por
/// mocks, igual que se hace con cualquier otra dependencia.
/// </summary>
[TestFixture]
public class FunkosHubTests
{
    private Mock<HubCallerContext> _mockContext = null!;
    private Mock<IGroupManager> _mockGroups = null!;
    private Mock<IHubCallerClients> _mockClients = null!;
    private Mock<ISingleClientProxy> _mockCallerProxy = null!;
    private FunkosHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        _mockContext = new Mock<HubCallerContext>();
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-1");

        _mockGroups = new Mock<IGroupManager>();
        _mockCallerProxy = new Mock<ISingleClientProxy>();
        _mockCallerProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockClients = new Mock<IHubCallerClients>();
        _mockClients.Setup(c => c.Caller).Returns(_mockCallerProxy.Object);

        _hub = new FunkosHub
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };
    }

    [TearDown]
    public void TearDown() => _hub.Dispose();

    private static ClaimsPrincipal CrearUsuario(bool esAdmin)
    {
        var claims = new List<Claim> { new("username", "test") };
        if (esAdmin)
            claims.Add(new Claim("role", "ADMIN"));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test", "username", "role"));
    }

    [TestFixture]
    public class CasosPositivos : FunkosHubTests
    {
        [Test]
        public async Task OnConnectedAsync_UsuarioAdmin_DeberiaUnirseAlGrupoAdmins()
        {
            // Arrange
            _mockContext.Setup(c => c.User).Returns(CrearUsuario(esAdmin: true));

            // Act
            await _hub.OnConnectedAsync();

            // Assert
            _mockGroups.Verify(g => g.AddToGroupAsync("conn-1", "admins", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task JoinCategoriaGroup_DeberiaUnirseAlGrupoDeEsaCategoria()
        {
            // Act
            await _hub.JoinCategoriaGroup(5);

            // Assert
            _mockGroups.Verify(g => g.AddToGroupAsync("conn-1", "categoria-5", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task LeaveCategoriaGroup_DeberiaSalirDelGrupoDeEsaCategoria()
        {
            // Act
            await _hub.LeaveCategoriaGroup(5);

            // Assert
            _mockGroups.Verify(g => g.RemoveFromGroupAsync("conn-1", "categoria-5", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Ping_DeberiaResponderPongAlCaller()
        {
            // Act
            await _hub.Ping();

            // Assert
            _mockCallerProxy.Verify(p => p.SendCoreAsync(
                "Pong", It.Is<object[]>(args => args.Length == 1 && args[0] is DateTime), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : FunkosHubTests
    {
        [Test]
        public async Task OnConnectedAsync_UsuarioNormal_NoDeberiaUnirseAlGrupoAdmins()
        {
            // Arrange
            _mockContext.Setup(c => c.User).Returns(CrearUsuario(esAdmin: false));

            // Act
            await _hub.OnConnectedAsync();

            // Assert
            _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), "admins", It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
