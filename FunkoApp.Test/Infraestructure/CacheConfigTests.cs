using FluentAssertions;
using FunkoAppi01.Cache;
using FunkoAppi01.Infraestructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Infraestructure;

/// <summary>
/// Tests de humo de CacheConfig: Development registra memoria, cualquier otro
/// entorno registra Redis.
/// </summary>
[TestFixture]
public class CacheConfigTests
{
    private static IConfiguration ConfiguracionVacia() => new ConfigurationBuilder().Build();

    private static IWebHostEnvironment CrearEntorno(string nombre)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(nombre);
        return mock.Object;
    }

    [TestFixture]
    public class CasosPositivos : CacheConfigTests
    {
        [Test]
        public void AddCache_EnDevelopment_DeberiaRegistrarMemoryCacheService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCache(ConfiguracionVacia(), CrearEntorno("Development"));

            // Assert
            services.Should().Contain(d =>
                d.ServiceType == typeof(ICacheService) && d.ImplementationType == typeof(MemoryCacheService));
        }

        [Test]
        public void AddCache_EnProduction_DeberiaRegistrarRedisCacheService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCache(ConfiguracionVacia(), CrearEntorno("Production"));

            // Assert
            services.Should().Contain(d =>
                d.ServiceType == typeof(ICacheService) && d.ImplementationType == typeof(RedisCacheService));
        }
    }
}
