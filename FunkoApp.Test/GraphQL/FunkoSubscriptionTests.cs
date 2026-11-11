using FluentAssertions;
using FunkoAppi01.Dto;
using FunkoAppi01.GraphQL;
using NUnit.Framework;

namespace FunkoApp.Test.GraphQL;

/// <summary>
/// Tests de FunkoSubscription. Cada resolver de subscription es una función
/// pura que devuelve tal cual el mensaje publicado — se comprueba justo eso.
/// </summary>
[TestFixture]
public class FunkoSubscriptionTests
{
    private FunkoSubscription _subscription = null!;

    [SetUp]
    public void SetUp()
    {
        _subscription = new FunkoSubscription();
    }

    [TestFixture]
    public class CasosPositivos : FunkoSubscriptionTests
    {
        [Test]
        public void FunkoCreado_DeberiaDevolverElMismoMensaje()
        {
            // Arrange
            var dto = new FunkoDto(1, "Pikachu", 15.5m, 10, "Pokemon", null, DateTime.UtcNow, null);

            // Act
            var resultado = _subscription.FunkoCreado(dto);

            // Assert
            resultado.Should().BeSameAs(dto);
        }

        [Test]
        public void FunkoActualizado_DeberiaDevolverElMismoMensaje()
        {
            // Arrange
            var dto = new FunkoDto(1, "Raichu", 20m, 5, "Pokemon", null, DateTime.UtcNow, DateTime.UtcNow);

            // Act
            var resultado = _subscription.FunkoActualizado(dto);

            // Assert
            resultado.Should().BeSameAs(dto);
        }

        [Test]
        public void FunkoEliminado_DeberiaDevolverElMismoId()
        {
            // Act
            var resultado = _subscription.FunkoEliminado(42);

            // Assert
            resultado.Should().Be(42);
        }
    }
}
