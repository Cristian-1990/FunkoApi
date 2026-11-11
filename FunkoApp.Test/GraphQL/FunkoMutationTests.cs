using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.GraphQL;
using FunkoAppi01.Service;
using HotChocolate.Subscriptions;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.GraphQL;

/// <summary>
/// Tests de FunkoMutation. Sin constructor en la clase real (ver su XMLDoc), así
/// que aquí también se le pasan las dependencias por parámetro, igual que hace
/// HotChocolate en producción.
/// </summary>
[TestFixture]
public class FunkoMutationTests
{
    private FunkoMutation _mutation = null!;
    private Mock<IServiceFunko> _mockService = null!;
    private Mock<ITopicEventSender> _mockSender = null!;

    [SetUp]
    public void SetUp()
    {
        _mutation = new FunkoMutation();
        _mockService = new Mock<IServiceFunko>();
        _mockSender = new Mock<ITopicEventSender>();
    }

    private static FunkoDto CrearDto(long id = 1, string nombre = "Pikachu") =>
        new(id, nombre, 15.5m, 10, "Pokemon", null, DateTime.UtcNow, null);

    [TestFixture]
    public class CasosPositivos : FunkoMutationTests
    {
        [Test]
        public async Task CrearFunko_Exito_DeberiaPublicarElEventoYDevolverElDto()
        {
            // Arrange
            var dto = CrearDto();
            _mockService.Setup(s => s.AddAsync(It.IsAny<CreateFunkoDto>()))
                .ReturnsAsync(Result.Success<FunkoDto, DomainError>(dto));

            // Act
            var resultado = await _mutation.CrearFunko(
                new CreateFunkoDto("Pikachu", 15.5m, 10, "Pokemon"), _mockService.Object, _mockSender.Object);

            // Assert
            resultado.Should().BeEquivalentTo(dto);
            _mockSender.Verify(s => s.SendAsync("FunkoCreado", dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ActualizarFunko_Exito_DeberiaPublicarElEventoYDevolverElDto()
        {
            // Arrange
            var dto = CrearDto(nombre: "Raichu");
            _mockService.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateFunkoDto>()))
                .ReturnsAsync(Result.Success<FunkoDto, DomainError>(dto));

            // Act
            var resultado = await _mutation.ActualizarFunko(
                1, new UpdateFunkoDto("Raichu", 20m, 5, "Pokemon"), _mockService.Object, _mockSender.Object);

            // Assert
            resultado.Should().BeEquivalentTo(dto);
            _mockSender.Verify(s => s.SendAsync("FunkoActualizado", dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task EliminarFunko_Exito_DeberiaPublicarElEventoYDevolverTrue()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(Result.Success<bool, DomainError>(true));

            // Act
            var resultado = await _mutation.EliminarFunko(1, _mockService.Object, _mockSender.Object);

            // Assert
            resultado.Should().BeTrue();
            _mockSender.Verify(s => s.SendAsync("FunkoEliminado", 1L, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : FunkoMutationTests
    {
        [Test]
        public async Task CrearFunko_Falla_NoDeberiaPublicarEventoYDevolverNull()
        {
            // Arrange
            _mockService.Setup(s => s.AddAsync(It.IsAny<CreateFunkoDto>()))
                .ReturnsAsync(Result.Failure<FunkoDto, DomainError>(FunkoErrors.AlreadyExists("Pikachu")));

            // Act
            var resultado = await _mutation.CrearFunko(
                new CreateFunkoDto("Pikachu", 15.5m, 10, "Pokemon"), _mockService.Object, _mockSender.Object);

            // Assert
            resultado.Should().BeNull();
            _mockSender.Invocations.Should().BeEmpty("un fallo de validación no debe notificar nada");
        }

        [Test]
        public async Task EliminarFunko_NoExiste_NoDeberiaPublicarEventoYDevolverFalse()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(99))
                .ReturnsAsync(Result.Failure<bool, DomainError>(FunkoErrors.NotFound(99)));

            // Act
            var resultado = await _mutation.EliminarFunko(99, _mockService.Object, _mockSender.Object);

            // Assert
            resultado.Should().BeFalse();
            _mockSender.Invocations.Should().BeEmpty("un Funko inexistente no debe notificar nada");
        }
    }
}
