using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FunkoAppi01.Dto;
using NUnit.Framework;

namespace FunkoApp.Test.Integration;

/// <summary>
/// Tests de integración de ProveedorController: servidor en memoria con
/// WebApplicationFactory, sin mocks, peticiones HTTP reales.
/// </summary>
[TestFixture]
public class ProveedorControllerIntegrationTests
{
    private FunkoAppWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new FunkoAppWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> ObtenerTokenAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto("admin@funko.com", "admin123"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    private async Task<string> ObtenerTokenUserAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto("user@funko.com", "user123"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    [TestFixture]
    public class CasosPositivos : ProveedorControllerIntegrationTests
    {
        [Test]
        public async Task GetAll_SinToken_DeberiaDevolverOkConLista()
        {
            // Act
            var response = await _client.GetAsync("/api/Proveedor");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var proveedores = await response.Content.ReadFromJsonAsync<List<ProveedorDto>>();
            proveedores.Should().NotBeNull();
        }

        [Test]
        public async Task Create_ConTokenDeAdmin_DeberiaDevolverCreated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Proveedor",
                new CreateProveedorDto("Funko Inc", "600111222"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]
        public async Task Delete_ConTokenDeAdmin_DeberiaDevolverNoContent()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());
            var creado = await _client.PostAsJsonAsync("/api/Proveedor",
                new CreateProveedorDto("Mattel", "600222333"));
            var proveedor = await creado.Content.ReadFromJsonAsync<ProveedorDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/Proveedor/{proveedor!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [TestFixture]
    public class CasosNegativos : ProveedorControllerIntegrationTests
    {
        [Test]
        public async Task Create_SinToken_DeberiaDevolver401()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/Proveedor",
                new CreateProveedorDto("Funko Inc", "600111222"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Create_ConTokenDeUsuarioNormal_DeberiaDevolver403()
        {
            // Arrange: crear Proveedor exige rol Admin, no solo estar autenticado
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Proveedor",
                new CreateProveedorDto("Hasbro", "600333444"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Create_TelefonoVacio_Devuelve400ConProblemDetailsPorCampo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Proveedor",
                new CreateProveedorDto("Funko Inc", ""));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("status").GetInt32().Should().Be(400);
        }

        [Test]
        public async Task GetById_NoExiste_Devuelve404ConShapeDeDominio()
        {
            // Act
            var response = await _client.GetAsync("/api/Proveedor/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        }
    }
}
