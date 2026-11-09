using FunkoAppi01.Dto;
using FunkoAppi01.Service;
using HotChocolate.Authorization;

namespace FunkoAppi01.GraphQL;

/// <summary>
/// Raíz de las consultas GraphQL. Reutiliza los servicios de negocio existentes
/// (los mismos que usa la API REST) en vez de hablar directamente con el
/// repositorio: así una regla de negocio solo vive en un sitio.
/// </summary>
public class FunkosQuery
{
    /// <summary>Lista todos los Funkos activos.</summary>
    [AllowAnonymous]
    public async Task<IEnumerable<FunkoDto>> GetFunkos([Service] IServiceFunko service) =>
        await service.GetAllAsync();

    /// <summary>Consulta un Funko por su identificador. Null si no existe.</summary>
    [AllowAnonymous]
    public async Task<FunkoDto?> GetFunko(long id, [Service] IServiceFunko service)
    {
        var resultado = await service.GetByIdAsync(id);
        return resultado.IsSuccess ? resultado.Value : null;
    }

    /// <summary>Lista todas las Categorias activas.</summary>
    [AllowAnonymous]
    public async Task<IEnumerable<CategoriaDto>> GetCategorias([Service] IServiceCategoria service) =>
        await service.GetAllAsync();
}
