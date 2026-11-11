using FluentAssertions;
using FunkoAppi01.Cache;
using FunkoAppi01.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Cache;

/// <summary>
/// Tests de RedisCacheService con IDistributedCache mockeado (no hace falta Redis
/// real para probar la lógica de serialización/expiración; eso ya se verificó en
/// vivo con Redis real en Docker, ver DECISIONES.md).
/// </summary>
[TestFixture]
public class RedisCacheServiceTests
{
    private Mock<IDistributedCache> _mockDistributedCache = null!;
    private RedisCacheService _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        var options = Options.Create(new CacheOptions { DefaultExpirationMinutes = 30, SlidingExpirationMinutes = 10 });
        _cache = new RedisCacheService(_mockDistributedCache.Object, options);
    }

    [TestFixture]
    public class CasosPositivos : RedisCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveExistente_DeberiaDeserializarElValor()
        {
            // Arrange
            _mockDistributedCache
                .Setup(c => c.GetAsync("clave", It.IsAny<CancellationToken>()))
                .ReturnsAsync("\"valor\""u8.ToArray());

            // Act
            var resultado = await _cache.GetAsync<string>("clave");

            // Assert
            resultado.Should().Be("valor");
        }

        [Test]
        public async Task SetAsync_DeberiaSerializarYGuardarConExpiracion()
        {
            // Act
            await _cache.SetAsync("clave", "valor", TimeSpan.FromMinutes(5));

            // Assert
            _mockDistributedCache.Verify(c => c.SetAsync(
                "clave",
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RemoveAsync_DeberiaLlamarAlRemoveDelDistributedCache()
        {
            // Act
            await _cache.RemoveAsync("clave");

            // Assert
            _mockDistributedCache.Verify(c => c.RemoveAsync("clave", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ExistsAsync_ClaveExistente_DeberiaDevolverTrue()
        {
            // Arrange
            _mockDistributedCache
                .Setup(c => c.GetAsync("clave", It.IsAny<CancellationToken>()))
                .ReturnsAsync("\"valor\""u8.ToArray());

            // Act
            var existe = await _cache.ExistsAsync("clave");

            // Assert
            existe.Should().BeTrue();
        }

        [Test]
        public async Task GetOrSetAsync_ConMiss_DeberiaEjecutarLaFactoryYGuardar()
        {
            // Arrange: MISS en las dos comprobaciones (antes y dentro del lock)
            _mockDistributedCache
                .Setup(c => c.GetAsync("clave", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var resultado = await _cache.GetOrSetAsync("clave", () => Task.FromResult("calculado"));

            // Assert
            resultado.Should().Be("calculado");
            _mockDistributedCache.Verify(c => c.SetAsync(
                "clave", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : RedisCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveInexistente_DeberiaDevolverDefault()
        {
            // Arrange
            _mockDistributedCache
                .Setup(c => c.GetAsync("no-existe", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var resultado = await _cache.GetAsync<string>("no-existe");

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ExistsAsync_ClaveInexistente_DeberiaDevolverFalse()
        {
            // Arrange
            _mockDistributedCache
                .Setup(c => c.GetAsync("no-existe", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var existe = await _cache.ExistsAsync("no-existe");

            // Assert
            existe.Should().BeFalse();
        }
    }
}
