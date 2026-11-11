using FluentAssertions;
using FunkoAppi01.Entity;
using FunkoAppi01.Models;
using FunkoAppi01.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FunkoApp.Test.Repository;

/// <summary>
/// Tests de RepositoryFunko contra una base SQLite real en memoria (no mockeada).
/// TestContainers queda para cuando se añada PostgreSQL; SQLite no necesita Docker.
/// </summary>
[TestFixture]
public class RepositoryFunkoTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private RepositoryFunko _repository = null!;

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

        _repository = new RepositoryFunko(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static Funko CrearFunko(string nombre = "Pikachu") => new()
    {
        Nombre = nombre,
        Precio = 15.5m,
        Stock = 10,
        Categoria = "Pokemon"
    };

    [TestFixture]
    public class CasosPositivos : RepositoryFunkoTests
    {
        [Test]
        public async Task AddAsync_DeberiaAsignarIdYFechaDeCreacion()
        {
            // Act
            var creado = await _repository.AddAsync(CrearFunko());

            // Assert
            creado.Id.Should().BeGreaterThan(0);
            creado.CreatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task AddAsync_DosFunkosConLaMismaCategoria_DeberianCompartirLaFilaDeCategoria()
        {
            // Act
            await _repository.AddAsync(CrearFunko("Pikachu"));
            await _repository.AddAsync(CrearFunko("Charmander"));

            // Assert
            (await _context.Categorias.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task GetAllAsync_SoloDeberiaDevolverLosNoBorrados()
        {
            // Arrange
            var activo = await _repository.AddAsync(CrearFunko("Pikachu"));
            var borrado = await _repository.AddAsync(CrearFunko("Charmander"));
            await _repository.SoftDeleteAsync(borrado.Id);

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            resultado.Should().ContainSingle(f => f.Id == activo.Id);
        }

        [Test]
        public async Task ExistsByNameAsync_ExcluyendoSuPropioId_DeberiaIgnorarseAsiMismo()
        {
            // Arrange
            var funko = await _repository.AddAsync(CrearFunko("Pikachu"));

            // Act
            var existe = await _repository.ExistsByNameAsync("Pikachu", excludeId: funko.Id);

            // Assert
            existe.Should().BeFalse();
        }

        [Test]
        public async Task AddAsync_ConProveedor_DeberiaCrearloYAsignarlo()
        {
            // Arrange
            var funko = CrearFunko("Pikachu");
            funko.Proveedor = "Funko Inc";

            // Act
            var creado = await _repository.AddAsync(funko);

            // Assert
            creado.Proveedor.Should().Be("Funko Inc");
            (await _context.Proveedores.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task AddAsync_DosFunkosConElMismoProveedor_DeberianCompartirLaFilaDeProveedor()
        {
            // Arrange
            var uno = CrearFunko("Pikachu");
            uno.Proveedor = "Funko Inc";
            var dos = CrearFunko("Charmander");
            dos.Proveedor = "Funko Inc";

            // Act
            await _repository.AddAsync(uno);
            await _repository.AddAsync(dos);

            // Assert
            (await _context.Proveedores.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task UpdatePrecioAsync_DeberiaActualizarSoloElPrecio()
        {
            // Arrange
            var creado = await _repository.AddAsync(CrearFunko("Pikachu"));

            // Act
            var actualizado = await _repository.UpdatePrecioAsync(creado.Id, 99.9m);

            // Assert
            actualizado!.Precio.Should().Be(99.9m);
            actualizado.UpdatedAt.Should().NotBeNull();
        }

        [Test]
        public async Task GetByNameAsync_DeberiaDevolverSoloLosQueContienenElTexto()
        {
            // Arrange
            await _repository.AddAsync(CrearFunko("Pikachu"));
            await _repository.AddAsync(CrearFunko("Charmander"));

            // Act
            var resultado = await _repository.GetByNameAsync("Pika");

            // Assert
            resultado.Should().ContainSingle(f => f.Nombre == "Pikachu");
        }

        [Test]
        public async Task GetPagedAsync_DeberiaRespetarElTamanoDePagina()
        {
            // Arrange
            await _repository.AddAsync(CrearFunko("Pikachu"));
            await _repository.AddAsync(CrearFunko("Charmander"));
            await _repository.AddAsync(CrearFunko("Squirtle"));

            // Act
            var (items, total) = await _repository.GetPagedAsync(1, 2);

            // Assert
            items.Should().HaveCount(2);
            total.Should().Be(3);
        }
    }

    [TestFixture]
    public class CasosNegativos : RepositoryFunkoTests
    {
        [Test]
        public async Task GetByIdAsync_FunkoBorrado_DeberiaDevolverNull()
        {
            // Arrange
            var creado = await _repository.AddAsync(CrearFunko());
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
            var resultado = await _repository.UpdateAsync(999, CrearFunko());

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ExistsByNameAsync_OtroFunkoActivoConEseNombre_DeberiaDevolverTrue()
        {
            // Arrange
            await _repository.AddAsync(CrearFunko("Pikachu"));

            // Act
            var existe = await _repository.ExistsByNameAsync("Pikachu");

            // Assert
            existe.Should().BeTrue();
        }
    }
}
