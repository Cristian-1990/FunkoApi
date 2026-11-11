using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FunkoAppi01.Dto;
using NUnit.Framework;

namespace FunkoApp.Test.Integration;

/// <summary>
/// Tests de integración de CategoriaController: servidor en memoria con
/// WebApplicationFactory, sin mocks, peticiones HTTP reales.
/// </summary>
[TestFixture]
public class CategoriaControllerIntegrationTests
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
    public class CasosPositivos : CategoriaControllerIntegrationTests
    {
        [Test]
        public async Task GetAll_SinToken_DeberiaDevolverOkConLista()
        {
            // Act
            var response = await _client.GetAsync("/api/Categoria");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaDto>>();
            categorias.Should().NotBeNull();
        }

        [Test]
        public async Task Create_ConTokenDeAdmin_DeberiaDevolverCreated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Categoria", new CreateCategoriaDto("Anime"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]
        public async Task Delete_ConTokenDeAdmin_DeberiaDevolverNoContent()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());
            var creada = await _client.PostAsJsonAsync("/api/Categoria", new CreateCategoriaDto("Videojuegos"));
            var categoria = await creada.Content.ReadFromJsonAsync<CategoriaDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/Categoria/{categoria!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [TestFixture]
    public class CasosNegativos : CategoriaControllerIntegrationTests
    {
        [Test]
        public async Task Create_SinToken_DeberiaDevolver401()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/Categoria", new CreateCategoriaDto("Anime"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Create_ConTokenDeUsuarioNormal_DeberiaDevolver403()
        {
            // Arrange: crear Categoria exige rol Admin, no solo estar autenticado
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenUserAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Categoria", new CreateCategoriaDto("Cine"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Create_NombreVacio_Devuelve400ConProblemDetailsPorCampo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await ObtenerTokenAdminAsync());

            // Act
            var response = await _client.PostAsJsonAsync("/api/Categoria", new CreateCategoriaDto(""));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("status").GetInt32().Should().Be(400);
        }

        [Test]
        public async Task GetById_NoExiste_Devuelve404ConShapeDeDominio()
        {
            // Act
            var response = await _client.GetAsync("/api/Categoria/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        }
    }
}
