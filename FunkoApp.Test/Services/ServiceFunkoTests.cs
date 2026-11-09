using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Models;
using FunkoAppi01.Repository;
using FunkoAppi01.Service;
using FunkoAppi01.Validators;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Services;

/// <summary>
/// Tests unitarios de ServiceFunko. El repositorio y el validador van simulados con Moq.
/// </summary>
[TestFixture]
public class ServiceFunkoTests
{
    private Mock<IRepositoryFunko> _mockRepository = null!;
    private Mock<IFunkoValidator> _mockValidator = null!;
    private Mock<IOutputCacheStore> _mockOutputCacheStore = null!;
    private ServiceFunko _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepositoryFunko>();
        _mockValidator = new Mock<IFunkoValidator>();
        _mockOutputCacheStore = new Mock<IOutputCacheStore>();
        _service = new ServiceFunko(
            _mockRepository.Object,
            _mockValidator.Object,
            new Mock<ILogger<ServiceFunko>>().Object,
            _mockOutputCacheStore.Object);
    }

    private static Funko CrearFunko(long id = 1, string nombre = "Pikachu") => new()
    {
        Id = id,
        Nombre = nombre,
        Precio = 15.5m,
        Stock = 10,
        Categoria = "Pokemon",
        CreatedAt = new DateTime(2026, 1, 1)
    };

    private static CreateFunkoDto CrearCreateDto(string nombre = "Pikachu") =>
        new(nombre, 15.5m, 10, "Pokemon");

    private static UpdateFunkoDto CrearUpdateDto(string nombre = "Pikachu") =>
        new(nombre, 20m, 5, "Pokemon");

    [TestFixture]
    public class CasosPositivos : ServiceFunkoTests
    {
        [Test]
        public async Task GetAllAsync_DeberiaMapearTodosLosFunkos()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync([CrearFunko(1, "Pikachu"), CrearFunko(2, "Charmander")]);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByIdAsync_ExisteElFunko_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CrearFunko());

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Nombre.Should().Be("Pikachu");
        }

        [Test]
        public async Task AddAsync_DatosValidos_DeberiaLlamarAlRepositorioYRetornarSuccess()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateFunkoDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Funko>())).ReturnsAsync(CrearFunko());

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Funko>()), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ExisteElFunko_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateFunkoDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 1)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Funko>())).ReturnsAsync(CrearFunko());

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }

        [Test]
        public async Task UpdatePriceAsync_ExisteElFunko_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockRepository.Setup(r => r.UpdatePrecioAsync(1, 25m)).ReturnsAsync(CrearFunko());

            // Act
            var resultado = await _service.UpdatePriceAsync(1, 25m);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }

        [Test]
        public async Task DeleteAsync_ExisteElFunko_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockRepository.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().BeTrue();
        }
    }

    [TestFixture]
    public class CasosNegativos : ServiceFunkoTests
    {
        [Test]
        public async Task GetByIdAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Funko?)null);

            // Act
            var resultado = await _service.GetByIdAsync(99);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }

        [Test]
        public async Task AddAsync_ValidacionFalla_NoDeberiaLlamarAlRepositorioAdd()
        {
            // Arrange
            var request = CrearCreateDto(nombre: "");
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Failure<CreateFunkoDto, DomainError>(
                    FunkoErrors.Validation(["El nombre no puede estar vacío"])));

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Funko>()), Times.Never);
        }

        [Test]
        public async Task AddAsync_NombreDuplicado_NoDeberiaLlamarAlRepositorioAdd()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateFunkoDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(true);

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.AlreadyExists>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Funko>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateFunkoDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 99)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(99, It.IsAny<Funko>())).ReturnsAsync((Funko?)null);

            // Act
            var resultado = await _service.UpdateAsync(99, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }

        [Test]
        public async Task UpdatePriceAsync_PrecioNegativo_NoDeberiaLlamarAlRepositorio()
        {
            // Act
            var resultado = await _service.UpdatePriceAsync(1, -5m);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRepository.Verify(r => r.UpdatePrecioAsync(It.IsAny<long>(), It.IsAny<decimal>()), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.SoftDeleteAsync(99)).ReturnsAsync(false);

            // Act
            var resultado = await _service.DeleteAsync(99);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }
    }
}
