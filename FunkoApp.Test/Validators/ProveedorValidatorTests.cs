using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Validators;
using NUnit.Framework;

namespace FunkoApp.Test.Validators;

/// <summary>
/// Tests de ProveedorValidator.
/// </summary>
[TestFixture]
public class ProveedorValidatorTests
{
    private ProveedorValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ProveedorValidator();
    }

    private static CreateProveedorDto PeticionCrear(string nombre = "Funko Inc", string telefono = "600111222") =>
        new(nombre, telefono);

    private static UpdateProveedorDto PeticionActualizar(string nombre = "Funko Inc", string telefono = "600111222") =>
        new(nombre, telefono);

    [TestFixture]
    public class CasosPositivos : ProveedorValidatorTests
    {
        [Test]
        public void ValidateCreate_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = PeticionCrear();

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Be(request);
        }

        [Test]
        public void ValidateUpdate_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = PeticionActualizar();

            // Act
            var resultado = _validator.ValidateUpdate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Be(request);
        }
    }

    [TestFixture]
    public class CasosNegativos : ProveedorValidatorTests
    {
        [Test]
        public void ValidateCreate_NombreSoloEspacios_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(nombre: "   ");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            resultado.Error.Message.Should().Contain("nombre no puede estar vacío");
        }

        [Test]
        public void ValidateCreate_TelefonoSoloEspacios_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(telefono: "   ");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("teléfono no puede estar vacío");
        }

        [Test]
        public void ValidateUpdate_NombreSoloEspacios_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionActualizar(nombre: "   ");

            // Act
            var resultado = _validator.ValidateUpdate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("nombre no puede estar vacío");
        }
    }
}
