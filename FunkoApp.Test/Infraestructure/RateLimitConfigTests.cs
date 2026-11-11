using FluentAssertions;
using FunkoAppi01.Infraestructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace FunkoApp.Test.Infraestructure;

/// <summary>
/// Test de humo de RateLimitConfig: registra el limitador y lo configura con el
/// código de rechazo esperado. El comportamiento en vivo (429 al superar el
/// límite) ya se verificó con una ráfaga real, ver DECISIONES.md.
/// </summary>
[TestFixture]
public class RateLimitConfigTests
{
    [TestFixture]
    public class CasosPositivos : RateLimitConfigTests
    {
        [Test]
        public void AddRateLimitConfig_DeberiaConfigurarElCodigoDeRechazo429()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddRateLimitConfig();
            var provider = services.BuildServiceProvider();
            var opciones = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

            // Assert
            opciones.RejectionStatusCode.Should().Be(429);
            opciones.GlobalLimiter.Should().NotBeNull();
        }
    }
}
