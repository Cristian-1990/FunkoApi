using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Validators;
using NUnit.Framework;

namespace FunkoApp.Test.Validators;

/// <summary>
/// Tests de CategoriaValidator.
/// </summary>
[TestFixture]
public class CategoriaValidatorTests
{
    private CategoriaValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CategoriaValidator();
    }

    private static CreateCategoriaDto PeticionCrear(string nombre = "Anime") => new(nombre);

    private static UpdateCategoriaDto PeticionActualizar(string nombre = "Anime") => new(nombre);

    [TestFixture]
    public class CasosPositivos : CategoriaValidatorTests
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
    public class CasosNegativos : CategoriaValidatorTests
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
