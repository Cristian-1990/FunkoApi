using FluentAssertions;
using FunkoAppi01.Entity;
using FunkoAppi01.Models;
using FunkoAppi01.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FunkoApp.Test.Repository;

/// <summary>
/// Tests de RepositoryProveedor contra una base SQLite real en memoria (no mockeada).
/// </summary>
[TestFixture]
public class RepositoryProveedorTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private RepositoryProveedor _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new RepositoryProveedor(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static Proveedor CrearProveedor(string nombre = "Funko Inc") =>
        new() { Nombre = nombre, Telefono = "600111222" };

    [TestFixture]
    public class CasosPositivos : RepositoryProveedorTests
    {
        [Test]
        public async Task AddAsync_DeberiaAsignarIdYFechaDeCreacion()
        {
            // Act
            var creado = await _repository.AddAsync(CrearProveedor());

            // Assert
            creado.Id.Should().BeGreaterThan(0);
            creado.CreatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task GetAllAsync_SoloDeberiaDevolverLosNoBorrados()
        {
            // Arrange
            var activo = await _repository.AddAsync(CrearProveedor("Funko Inc"));
            var borrado = await _repository.AddAsync(CrearProveedor("Mattel"));
            await _repository.SoftDeleteAsync(borrado.Id);

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            resultado.Should().ContainSingle(p => p.Id == activo.Id);
        }

        [Test]
        public async Task UpdateAsync_DeberiaCambiarNombreYTelefono()
        {
            // Arrange
            var creado = await _repository.AddAsync(CrearProveedor());

            // Act
            var actualizado = await _repository.UpdateAsync(
                creado.Id, new Proveedor { Nombre = "Funko Inc.", Telefono = "600999888" });

            // Assert
            actualizado!.Nombre.Should().Be("Funko Inc.");
            actualizado.Telefono.Should().Be("600999888");
        }

        [Test]
        public async Task GetPagedAsync_DeberiaRespetarElTamanoDePagina()
        {
            // Arrange
            await _repository.AddAsync(CrearProveedor("Funko Inc"));
            await _repository.AddAsync(CrearProveedor("Mattel"));
            await _repository.AddAsync(CrearProveedor("Hasbro"));

            // Act
            var (items, total) = await _repository.GetPagedAsync(1, 2);

            // Assert
            items.Should().HaveCount(2);
            total.Should().Be(3);
        }
    }

    [TestFixture]
    public class CasosNegativos : RepositoryProveedorTests
    {
        [Test]
        public async Task GetByIdAsync_ProveedorBorrado_DeberiaDevolverNull()
        {
            // Arrange
            var creado = await _repository.AddAsync(CrearProveedor());
            await _repository.SoftDeleteAsync(creado.Id);

            // Act
            var resultado = await _repository.GetByIdAsync(creado.Id);

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task SoftDeleteAsync_IdInexistente_DeberiaDevolverFalse()
        {
            // Act
            var resultado = await _repository.SoftDeleteAsync(999);

            // Assert
            resultado.Should().BeFalse();
        }

        [Test]
        public async Task UpdateAsync_IdInexistente_DeberiaDevolverNull()
        {
            // Act
            var resultado = await _repository.UpdateAsync(999, CrearProveedor());

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ExistsByNameAsync_OtroProveedorActivoConEseNombre_DeberiaDevolverTrue()
        {
            // Arrange
            await _repository.AddAsync(CrearProveedor("Funko Inc"));

            // Act
            var existe = await _repository.ExistsByNameAsync("Funko Inc");

            // Assert
            existe.Should().BeTrue();
        }
    }
}
