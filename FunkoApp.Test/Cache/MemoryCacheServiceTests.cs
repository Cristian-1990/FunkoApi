using FluentAssertions;
using FunkoAppi01.Cache;
using FunkoAppi01.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace FunkoApp.Test.Cache;

/// <summary>
/// Tests de MemoryCacheService contra un IMemoryCache real (no mockeado):
/// es el propio objeto de .NET, no hay nada que simular.
/// </summary>
[TestFixture]
public class MemoryCacheServiceTests
{
    private MemoryCacheService _cache = null!;

    [SetUp]
    public void SetUp()
    {
        var options = Options.Create(new CacheOptions { DefaultExpirationMinutes = 30 });
        _cache = new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), options);
    }

    [TestFixture]
    public class CasosPositivos : MemoryCacheServiceTests
    {
        [Test]
        public async Task SetAsync_LuegoGetAsync_DeberiaDevolverElMismoValor()
        {
            // Act
            await _cache.SetAsync("clave", "valor");
            var resultado = await _cache.GetAsync<string>("clave");

            // Assert
            resultado.Should().Be("valor");
        }

        [Test]
        public async Task ExistsAsync_ClaveGuardada_DeberiaDevolverTrue()
        {
            // Arrange
            await _cache.SetAsync("clave", 42);

            // Act
            var existe = await _cache.ExistsAsync("clave");

            // Assert
            existe.Should().BeTrue();
        }

        [Test]
        public async Task RemoveAsync_DeberiaEliminarLaEntrada()
        {
            // Arrange
            await _cache.SetAsync("clave", "valor");

            // Act
            await _cache.RemoveAsync("clave");

            // Assert
            (await _cache.ExistsAsync("clave")).Should().BeFalse();
        }

        [Test]
        public async Task GetOrSetAsync_ConMiss_DeberiaEjecutarLaFactoryYGuardarElResultado()
        {
            // Act
            var resultado = await _cache.GetOrSetAsync("clave", () => Task.FromResult("calculado"));

            // Assert
            resultado.Should().Be("calculado");
            (await _cache.GetAsync<string>("clave")).Should().Be("calculado");
        }

        [Test]
        public async Task GetOrSetAsync_ConHit_NoDeberiaEjecutarLaFactory()
        {
            // Arrange
            await _cache.SetAsync("clave", "original");
            var factoryLlamada = false;

            // Act
            var resultado = await _cache.GetOrSetAsync("clave", () =>
            {
                factoryLlamada = true;
                return Task.FromResult("nuevo");
            });

            // Assert
            resultado.Should().Be("original");
            factoryLlamada.Should().BeFalse();
        }
    }

    [TestFixture]
    public class CasosNegativos : MemoryCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveInexistente_DeberiaDevolverDefault()
        {
            // Act
            var resultado = await _cache.GetAsync<string>("no-existe");

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ExistsAsync_ClaveInexistente_DeberiaDevolverFalse()
        {
            // Act
            var existe = await _cache.ExistsAsync("no-existe");

            // Assert
            existe.Should().BeFalse();
        }
    }
}
