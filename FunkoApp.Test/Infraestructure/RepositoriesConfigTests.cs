using FluentAssertions;
using FunkoAppi01.Infraestructure;
using FunkoAppi01.Repository;
using FunkoAppi01.Repository.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FunkoApp.Test.Infraestructure;

/// <summary>
/// Tests de humo de RepositoriesConfig: comprueba que cada rama (EFCore/Mongo)
/// registra la implementación de IRepositoryCategoria que le corresponde.
/// Se inspeccionan los ServiceDescriptor directamente, sin resolver el
/// proveedor completo: AppDbContext lo registra PersistenceConfig aparte, no
/// hace falta levantarlo aquí para comprobar qué se ha registrado.
/// </summary>
[TestFixture]
public class RepositoriesConfigTests
{
    private static IConfiguration ConfiguracionConCategoriaProvider(string proveedor)
    {
        var datos = new Dictionary<string, string?>
        {
            ["DatabaseSettings:CategoriaProvider"] = proveedor
        };
        return new ConfigurationBuilder().AddInMemoryCollection(datos).Build();
    }

    [TestFixture]
    public class CasosPositivos : RepositoriesConfigTests
    {
        [Test]
        public void AddRepositories_PorDefecto_DeberiaRegistrarRepositoryCategoriaEfCore()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddRepositories(ConfiguracionConCategoriaProvider("EFCore"));

            // Assert
            services.Should().Contain(d =>
                d.ServiceType == typeof(IRepositoryCategoria) && d.ImplementationType == typeof(RepositoryCategoria));
            services.Should().Contain(d =>
                d.ServiceType == typeof(IRepositoryFunko) && d.ImplementationType == typeof(RepositoryFunko));
            services.Should().Contain(d =>
                d.ServiceType == typeof(IRepositoryProveedor) && d.ImplementationType == typeof(RepositoryProveedor));
        }

        [Test]
        public void AddRepositories_ConCategoriaProviderMongo_DeberiaRegistrarRepositoryCategoriaMongo()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddRepositories(ConfiguracionConCategoriaProvider("Mongo"));

            // Assert
            services.Should().Contain(d =>
                d.ServiceType == typeof(IRepositoryCategoria) && d.ImplementationType == typeof(RepositoryCategoriaMongo));
        }
    }
}
