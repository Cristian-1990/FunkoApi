using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Models;
using FunkoAppi01.Repository;
using FunkoAppi01.Service;
using FunkoAppi01.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Services;

/// <summary>
/// Tests unitarios de ServiceCategoria. El repositorio y el validador van simulados con Moq.
/// </summary>
[TestFixture]
public class ServiceCategoriaTests
{
    private Mock<IRepositoryCategoria> _mockRepository = null!;
    private Mock<ICategoriaValidator> _mockValidator = null!;
    private ServiceCategoria _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepositoryCategoria>();
        _mockValidator = new Mock<ICategoriaValidator>();
        _service = new ServiceCategoria(
            _mockRepository.Object,
            _mockValidator.Object,
            new Mock<ILogger<ServiceCategoria>>().Object);
    }

    private static Categoria CrearCategoria(long id = 1, string nombre = "Anime") => new()
    {
        Id = id,
        Nombre = nombre,
        CreatedAt = new DateTime(2026, 1, 1)
    };

    private static CreateCategoriaDto CrearCreateDto(string nombre = "Anime") => new(nombre);

    private static UpdateCategoriaDto CrearUpdateDto(string nombre = "Anime") => new(nombre);

    [TestFixture]
    public class CasosPositivos : ServiceCategoriaTests
    {
        [Test]
        public async Task GetAllAsync_DeberiaMapearTodasLasCategorias()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync([CrearCategoria(1, "Anime"), CrearCategoria(2, "Videojuegos")]);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByIdAsync_ExisteLaCategoria_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CrearCategoria());

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Nombre.Should().Be("Anime");
        }

        [Test]
        public async Task AddAsync_DatosValidos_DeberiaLlamarAlRepositorioYRetornarSuccess()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateCategoriaDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Categoria>())).ReturnsAsync(CrearCategoria());

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Categoria>()), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ExisteLaCategoria_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateCategoriaDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 1)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Categoria>())).ReturnsAsync(CrearCategoria());

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }

        [Test]
        public async Task DeleteAsync_ExisteLaCategoria_DeberiaRetornarSuccess()
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
    public class CasosNegativos : ServiceCategoriaTests
    {
        [Test]
        public async Task GetByIdAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);

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
                .Returns(Result.Failure<CreateCategoriaDto, DomainError>(
                    CategoriaErrors.Validation(["El nombre no puede estar vacío"])));

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Test]
        public async Task AddAsync_NombreDuplicado_NoDeberiaLlamarAlRepositorioAdd()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateCategoriaDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(true);

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.AlreadyExists>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateCategoriaDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 99)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(99, It.IsAny<Categoria>())).ReturnsAsync((Categoria?)null);

            // Act
            var resultado = await _service.UpdateAsync(99, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
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
