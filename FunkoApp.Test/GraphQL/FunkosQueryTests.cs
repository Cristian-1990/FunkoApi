using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.GraphQL;
using FunkoAppi01.Service;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.GraphQL;

/// <summary>
/// Tests de FunkosQuery. HotChocolate inyecta los servicios por parámetro ([Service]),
/// así que se puede llamar al método directamente, sin levantar el servidor GraphQL.
/// </summary>
[TestFixture]
public class FunkosQueryTests
{
    private FunkosQuery _query = null!;

    [SetUp]
    public void SetUp()
    {
        _query = new FunkosQuery();
    }

    [TestFixture]
    public class CasosPositivos : FunkosQueryTests
    {
        [Test]
        public async Task GetFunkos_DeberiaDevolverLoQueDevuelveElServicio()
        {
            // Arrange
            var mockService = new Mock<IServiceFunko>();
            mockService.Setup(s => s.GetAllAsync())
                .ReturnsAsync([new FunkoDto(1, "Pikachu", 15.5m, 10, "Pokemon", null, DateTime.UtcNow, null)]);

            // Act
            var resultado = await _query.GetFunkos(mockService.Object);

            // Assert
            resultado.Should().ContainSingle(f => f.Nombre == "Pikachu");
        }

        [Test]
        public async Task GetFunko_Existente_DeberiaDevolverElDto()
        {
            // Arrange
            var dto = new FunkoDto(1, "Pikachu", 15.5m, 10, "Pokemon", null, DateTime.UtcNow, null);
            var mockService = new Mock<IServiceFunko>();
            mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Result.Success<FunkoDto, DomainError>(dto));

            // Act
            var resultado = await _query.GetFunko(1, mockService.Object);

            // Assert
            resultado.Should().BeEquivalentTo(dto);
        }

        [Test]
        public async Task GetCategorias_DeberiaDevolverLoQueDevuelveElServicio()
        {
            // Arrange
            var mockService = new Mock<IServiceCategoria>();
            mockService.Setup(s => s.GetAllAsync())
                .ReturnsAsync([new CategoriaDto(1, "Anime", DateTime.UtcNow, null)]);

            // Act
            var resultado = await _query.GetCategorias(mockService.Object);

            // Assert
            resultado.Should().ContainSingle(c => c.Nombre == "Anime");
        }
    }

    [TestFixture]
    public class CasosNegativos : FunkosQueryTests
    {
        [Test]
        public async Task GetFunko_NoExiste_DeberiaDevolverNull()
        {
            // Arrange
            var mockService = new Mock<IServiceFunko>();
            mockService.Setup(s => s.GetByIdAsync(99))
                .ReturnsAsync(Result.Failure<FunkoDto, DomainError>(FunkoErrors.NotFound(99)));

            // Act
            var resultado = await _query.GetFunko(99, mockService.Object);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
