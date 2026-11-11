using FluentAssertions;
using FunkoAppi01.Service.Email;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Services;

/// <summary>
/// Tests de MemoryEmailService. No envía nada de verdad: solo se comprueba que
/// no lanza excepción al "enviar"/"encolar" un mensaje.
/// </summary>
[TestFixture]
public class MemoryEmailServiceTests
{
    private MemoryEmailService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new MemoryEmailService(new Mock<ILogger<MemoryEmailService>>().Object);
    }

    [TestFixture]
    public class CasosPositivos : MemoryEmailServiceTests
    {
        [Test]
        public async Task SendEmailAsync_NoDeberiaLanzarExcepcion()
        {
            // Act
            var accion = async () => await _service.SendEmailAsync(new EmailMessage
            {
                To = "destino@funko.com",
                Subject = "Asunto",
                Body = "<p>Cuerpo</p>",
                IsHtml = true
            });

            // Assert
            await accion.Should().NotThrowAsync();
        }

        [Test]
        public async Task EnqueueEmailAsync_NoDeberiaLanzarExcepcion()
        {
            // Act
            var accion = async () => await _service.EnqueueEmailAsync(new EmailMessage
            {
                To = "destino@funko.com",
                Subject = "Asunto",
                Body = "Texto plano",
                IsHtml = false
            });

            // Assert
            await accion.Should().NotThrowAsync();
        }
    }
}
