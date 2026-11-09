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
/// Tests unitarios de ServiceProveedor. El repositorio y el validador van simulados con Moq.
/// </summary>
[TestFixture]
public class ServiceProveedorTests
{
    private Mock<IRepositoryProveedor> _mockRepository = null!;
    private Mock<IProveedorValidator> _mockValidator = null!;
    private Mock<IOutputCacheStore> _mockOutputCacheStore = null!;
    private ServiceProveedor _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepositoryProveedor>();
        _mockValidator = new Mock<IProveedorValidator>();
        _mockOutputCacheStore = new Mock<IOutputCacheStore>();
        _service = new ServiceProveedor(
            _mockRepository.Object,
            _mockValidator.Object,
            new Mock<ILogger<ServiceProveedor>>().Object,
            _mockOutputCacheStore.Object);
    }

    private static Proveedor CrearProveedor(long id = 1, string nombre = "Funko Inc") => new()
    {
        Id = id,
        Nombre = nombre,
        Telefono = "600111222",
        CreatedAt = new DateTime(2026, 1, 1)
    };

    private static CreateProveedorDto CrearCreateDto(string nombre = "Funko Inc", string telefono = "600111222") =>
        new(nombre, telefono);

    private static UpdateProveedorDto CrearUpdateDto(string nombre = "Funko Inc", string telefono = "600111222") =>
        new(nombre, telefono);

    [TestFixture]
    public class CasosPositivos : ServiceProveedorTests
    {
        [Test]
        public async Task GetAllAsync_DeberiaMapearTodosLosProveedores()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync([CrearProveedor(1, "Funko Inc"), CrearProveedor(2, "Mattel")]);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByIdAsync_ExisteElProveedor_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CrearProveedor());

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Nombre.Should().Be("Funko Inc");
        }

        [Test]
        public async Task AddAsync_DatosValidos_DeberiaLlamarAlRepositorioYRetornarSuccess()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateProveedorDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Proveedor>())).ReturnsAsync(CrearProveedor());

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Proveedor>()), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ExisteElProveedor_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateProveedorDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 1)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Proveedor>())).ReturnsAsync(CrearProveedor());

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }

        [Test]
        public async Task DeleteAsync_ExisteElProveedor_DeberiaRetornarSuccess()
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
    public class CasosNegativos : ServiceProveedorTests
    {
        [Test]
        public async Task GetByIdAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Proveedor?)null);

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
                .Returns(Result.Failure<CreateProveedorDto, DomainError>(
                    ProveedorErrors.Validation(["El nombre no puede estar vacío"])));

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Proveedor>()), Times.Never);
        }

        [Test]
        public async Task AddAsync_NombreDuplicado_NoDeberiaLlamarAlRepositorioAdd()
        {
            // Arrange
            var request = CrearCreateDto();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateProveedorDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, null)).ReturnsAsync(true);

            // Act
            var resultado = await _service.AddAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.AlreadyExists>();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Proveedor>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_NoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            var request = CrearUpdateDto();
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateProveedorDto, DomainError>(request));
            _mockRepository.Setup(r => r.ExistsByNameAsync(request.Nombre, 99)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(99, It.IsAny<Proveedor>())).ReturnsAsync((Proveedor?)null);

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
