using System.Text.Json;
using FluentAssertions;
using FunkoAppi01.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunkoApp.Test.Middleware;

/// <summary>
/// Tests de ExceptionHandlerMiddleware. Se invoca directamente con un
/// HttpContext real (DefaultHttpContext) y un "next" que lanza la excepción
/// que se quiera probar, sin necesidad de levantar el servidor entero.
/// </summary>
[TestFixture]
public class ExceptionHandlerMiddlewareTests
{
    private static HttpContext CrearContexto()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> LeerCuerpoAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    [TestFixture]
    public class CasosPositivos : ExceptionHandlerMiddlewareTests
    {
        [Test]
        public async Task InvokeAsync_SinExcepcion_DeberiaDejarPasarLaPeticion()
        {
            // Arrange
            var context = CrearContexto();
            var middleware = new ExceptionHandlerMiddleware(
                _ => Task.CompletedTask, new Mock<ILogger<ExceptionHandlerMiddleware>>().Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task InvokeAsync_ArgumentException_DeberiaDevolver400ConProblemDetails()
        {
            // Arrange
            var context = CrearContexto();
            var middleware = new ExceptionHandlerMiddleware(
                _ => throw new ArgumentException("dato invalido"),
                new Mock<ILogger<ExceptionHandlerMiddleware>>().Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(400);
            context.Response.ContentType.Should().Be("application/problem+json");
            var cuerpo = await LeerCuerpoAsync(context);
            var problema = JsonSerializer.Deserialize<JsonElement>(cuerpo);
            problema.GetProperty("detail").GetString().Should().Be("dato invalido");
        }

        [Test]
        public async Task InvokeAsync_ExcepcionGenerica_DeberiaDevolver500SinFiltrarElMensajeReal()
        {
            // Arrange
            var context = CrearContexto();
            var middleware = new ExceptionHandlerMiddleware(
                _ => throw new InvalidOperationException("detalle interno sensible"),
                new Mock<ILogger<ExceptionHandlerMiddleware>>().Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);
            var cuerpo = await LeerCuerpoAsync(context);
            cuerpo.Should().NotContain("detalle interno sensible",
                "un error 500 nunca debe filtrar el mensaje real de la excepción al cliente");
        }
    }
}
