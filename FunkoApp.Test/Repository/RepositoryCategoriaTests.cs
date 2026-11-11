using FluentAssertions;
using FunkoAppi01.Entity;
using FunkoAppi01.Models;
using FunkoAppi01.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FunkoApp.Test.Repository;

/// <summary>
/// Tests de RepositoryCategoria contra una base SQLite real en memoria (no mockeada).
/// </summary>
[TestFixture]
public class RepositoryCategoriaTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private RepositoryCategoria _repository = null!;

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

        _repository = new RepositoryCategoria(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static Categoria CrearCategoria(string nombre = "Anime") => new() { Nombre = nombre };

    [TestFixture]
    public class CasosPositivos : RepositoryCategoriaTests
    {
        [Test]
        public async Task AddAsync_DeberiaAsignarIdYFechaDeCreacion()
        {
            // Act
            var creada = await _repository.AddAsync(CrearCategoria());

            // Assert
            creada.Id.Should().BeGreaterThan(0);
            creada.CreatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task GetAllAsync_SoloDeberiaDevolverLasNoBorradas()
        {
            // Arrange
            var activa = await _repository.AddAsync(CrearCategoria("Anime"));
            var borrada = await _repository.AddAsync(CrearCategoria("Videojuegos"));
            await _repository.SoftDeleteAsync(borrada.Id);

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            resultado.Should().ContainSingle(c => c.Id == activa.Id);
        }

        [Test]
        public async Task UpdateAsync_DeberiaCambiarElNombreYMarcarActualizado()
        {
            // Arrange
            var creada = await _repository.AddAsync(CrearCategoria("Anime"));

            // Act
            var actualizada = await _repository.UpdateAsync(creada.Id, CrearCategoria("Anime y Manga"));

            // Assert
            actualizada!.Nombre.Should().Be("Anime y Manga");
            actualizada.UpdatedAt.Should().NotBeNull();
        }

        [Test]
        public async Task GetPagedAsync_DeberiaRespetarElTamanoDePagina()
        {
            // Arrange
            await _repository.AddAsync(CrearCategoria("Anime"));
            await _repository.AddAsync(CrearCategoria("Videojuegos"));
            await _repository.AddAsync(CrearCategoria("Cine"));

            // Act
            var (items, total) = await _repository.GetPagedAsync(1, 2);

            // Assert
            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        [Test]
        public async Task ExistsByNameAsync_ExcluyendoSuPropioId_DeberiaIgnorarseAsiMismo()
        {
            // Arrange
            var categoria = await _repository.AddAsync(CrearCategoria("Anime"));

            // Act
            var existe = await _repository.ExistsByNameAsync("Anime", excludeId: categoria.Id);

            // Assert
            existe.Should().BeFalse();
        }
    }

    [TestFixture]
    public class CasosNegativos : RepositoryCategoriaTests
    {
        [Test]
        public async Task GetByIdAsync_CategoriaBorrada_DeberiaDevolverNull()
        {
            // Arrange
            var creada = await _repository.AddAsync(CrearCategoria());
            await _repository.SoftDeleteAsync(creada.Id);

            // Act
            var resultado = await _repository.GetByIdAsync(creada.Id);

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
            var resultado = await _repository.UpdateAsync(999, CrearCategoria());

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ExistsByNameAsync_OtraCategoriaActivaConEseNombre_DeberiaDevolverTrue()
        {
            // Arrange
            await _repository.AddAsync(CrearCategoria("Anime"));

            // Act
            var existe = await _repository.ExistsByNameAsync("Anime");

            // Assert
            existe.Should().BeTrue();
        }
    }
}
