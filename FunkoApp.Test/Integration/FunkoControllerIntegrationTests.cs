using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FunkoAppi01.Dto;
using NUnit.Framework;

namespace FunkoApp.Test.Integration;

/// <summary>
/// Tests de integración de FunkoController: servidor en memoria con
/// WebApplicationFactory, sin mocks, peticiones HTTP reales.
/// </summary>
[TestFixture]
public class FunkoControllerIntegrationTests
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
    public class CasosPositivos : FunkoControllerIntegrationTests
    {
        [Test]
        public async Task GetAll_SinToken_DeberiaDevolverOkConLista()
        {
            // Act
            var response = await _client.GetAsync("/api/Funko");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var funkos = await response.Content.ReadFromJsonAsync<List<FunkoDto>>();
            funkos.Should().NotBeNull();
        }

        [Test]
        public async Task Create_ConTokenDeUsuarioNormal_DeberiaDevolverCreated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());
            var request = new CreateFunkoDto("Pikachu", 15.5m, 10, "Pokemon");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Funko", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]
        public async Task Delete_ConTokenDeAdmin_DeberiaDevolverNoContent()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());
            var creado = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("Charmander", 12m, 5, "Pokemon"));
            var funko = await creado.Content.ReadFromJsonAsync<FunkoDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/Funko/{funko!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task Update_Existente_DeberiaDevolverOkConLosNuevosDatos()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());
            var creado = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("Eevee", 10m, 2, "Pokemon"));
            var funko = await creado.Content.ReadFromJsonAsync<FunkoDto>();

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Funko/{funko!.Id}",
                new UpdateFunkoDto("Vaporeon", 18m, 1, "Pokemon"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<FunkoDto>();
            actualizado!.Nombre.Should().Be("Vaporeon");
        }

        [Test]
        public async Task UpdatePrice_Existente_DeberiaDevolverOkConElNuevoPrecio()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());
            var creado = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("Jolteon", 10m, 2, "Pokemon"));
            var funko = await creado.Content.ReadFromJsonAsync<FunkoDto>();

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/Funko/{funko!.Id}", 25m);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<FunkoDto>();
            actualizado!.Precio.Should().Be(25m);
        }

        [Test]
        public async Task Search_DeberiaDevolverSoloLosQueCoincidenConElTexto()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());
            await _client.PostAsJsonAsync("/api/Funko", new CreateFunkoDto("Articuno", 30m, 1, "Pokemon"));
            await _client.PostAsJsonAsync("/api/Funko", new CreateFunkoDto("Zapdos", 30m, 1, "Pokemon"));

            // Act
            var response = await _client.GetAsync("/api/Funko/search?nombre=Arti");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var funkos = await response.Content.ReadFromJsonAsync<List<FunkoDto>>();
            funkos.Should().ContainSingle(f => f.Nombre == "Articuno");
        }

        [Test]
        public async Task GetPaged_DeberiaDevolverLosMetadatosDePaginacion()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());
            await _client.PostAsJsonAsync("/api/Funko", new CreateFunkoDto("Moltres", 30m, 1, "Pokemon"));

            // Act
            var response = await _client.GetAsync("/api/Funko/paged?page=1&pageSize=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<FunkoDto>>();
            pagina!.Pagination.Page.Should().Be(1);
            pagina.Pagination.PageSize.Should().Be(1);
            pagina.Data.Should().HaveCount(1);
        }
    }

    [TestFixture]
    public class CasosNegativos : FunkoControllerIntegrationTests
    {
        [Test]
        public async Task Create_SinToken_DeberiaDevolver401()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("Pikachu", 15.5m, 10, "Pokemon"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Delete_ConTokenDeUsuarioNormal_DeberiaDevolver403()
        {
            // Arrange: crea el Funko como admin, intenta borrarlo como user
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());
            var creado = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("Squirtle", 11m, 3, "Pokemon"));
            var funko = await creado.Content.ReadFromJsonAsync<FunkoDto>();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());

            // Act
            var response = await _client.DeleteAsync($"/api/Funko/{funko!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Create_CuerpoInvalido_Devuelve400ConProblemDetailsPorCampo()
        {
            // Arrange: nombre vacío y precio negativo, varios campos mal a la vez
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Funko",
                new CreateFunkoDto("", -5m, 1, "Pokemon"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("status").GetInt32().Should().Be(400);
            body.TryGetProperty("errors", out var errors).Should().BeTrue(
                "la validación de [ApiController] agrupa los errores por campo");
            errors.EnumerateObject().Should().NotBeEmpty();
        }

        [Test]
        public async Task GetById_NoExiste_Devuelve404ConShapeDeDominio()
        {
            // Act
            var response = await _client.GetAsync("/api/Funko/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        }
    }
}
