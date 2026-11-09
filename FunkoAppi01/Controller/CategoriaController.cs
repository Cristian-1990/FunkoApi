using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Entity;
using FunkoAppi01.Errors;
using FunkoAppi01.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Swashbuckle.AspNetCore.Annotations;

namespace FunkoAppi01.Controller;

/// <summary>
/// CRUD de Categorias. La lectura es pública; crear, actualizar y borrar exige rol Admin
/// (según el esquema acordado en clase para Categoria y Proveedor).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriaController(IServiceCategoria service) : ControllerBase
{
    /// <summary>Lista todas las Categorias activas.</summary>
    /// <response code="200">Listado devuelto correctamente.</response>
    [AllowAnonymous]
    [HttpGet]
    [OutputCache(Duration = 60, Tags = ["categorias"])]
    [SwaggerOperation(Summary = "Lista todas las Categorias", OperationId = "GetAllCategorias", Tags = ["Categoria"])]
    [SwaggerResponse(200, "Listado de Categorias", typeof(IEnumerable<CategoriaDto>))]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll() => Ok(await service.GetAllAsync());

    /// <summary>Lista las Categorias de forma paginada.</summary>
    /// <param name="page">Página solicitada (empieza en 1).</param>
    /// <param name="pageSize">Elementos por página (máximo 100).</param>
    /// <response code="200">Página devuelta correctamente.</response>
    [AllowAnonymous]
    [HttpGet("paged")]
    [OutputCache(Duration = 60, Tags = ["categorias"])]
    [SwaggerOperation(Summary = "Lista las Categorias paginadas", OperationId = "GetPagedCategorias", Tags = ["Categoria"])]
    [SwaggerResponse(200, "Página de Categorias", typeof(PagedResponse<CategoriaDto>))]
    public async Task<ActionResult<PagedResponse<CategoriaDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await service.GetPagedAsync(page, pageSize));

    /// <summary>Consulta una Categoria por su identificador.</summary>
    /// <param name="id">Identificador de la Categoria.</param>
    /// <response code="200">Categoria encontrada.</response>
    /// <response code="404">No existe ninguna Categoria con ese id.</response>
    [AllowAnonymous]
    [HttpGet("{id:long}")]
    [OutputCache(Duration = 60, Tags = ["categorias"])]
    [SwaggerOperation(Summary = "Consulta una Categoria por id", OperationId = "GetCategoriaById", Tags = ["Categoria"])]
    [SwaggerResponse(200, "Categoria encontrada", typeof(CategoriaDto))]
    [SwaggerResponse(404, "No encontrada")]
    public async Task<ActionResult<CategoriaDto>> GetById(long id) => ToActionResult(await service.GetByIdAsync(id));

    /// <summary>Busca Categorias cuyo nombre contenga el texto indicado.</summary>
    /// <param name="nombre">Texto a buscar en el nombre.</param>
    /// <response code="200">Resultado de la búsqueda (puede ser vacío).</response>
    [AllowAnonymous]
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Busca Categorias por nombre", OperationId = "SearchCategorias", Tags = ["Categoria"])]
    [SwaggerResponse(200, "Categorias encontradas", typeof(IEnumerable<CategoriaDto>))]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> Search([FromQuery] string? nombre) =>
        Ok(await service.GetByNameAsync(nombre ?? string.Empty));

    /// <summary>Crea una Categoria nueva. Requiere rol Admin.</summary>
    /// <param name="request">Datos de la Categoria a crear.</param>
    /// <response code="201">Categoria creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Ya existe una Categoria con ese nombre.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    [SwaggerOperation(Summary = "Crea una Categoria (solo Admin)", OperationId = "CreateCategoria", Tags = ["Categoria"])]
    [SwaggerResponse(201, "Categoria creada", typeof(CategoriaDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(409, "Nombre duplicado")]
    [SwaggerResponse(403, "Sin permisos")]
    public async Task<ActionResult<CategoriaDto>> Create(CreateCategoriaDto request)
    {
        var resultado = await service.AddAsync(request);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : ToActionResult(resultado);
    }

    /// <summary>Actualiza por completo una Categoria existente. Requiere rol Admin.</summary>
    /// <param name="id">Identificador de la Categoria.</param>
    /// <param name="request">Nuevos datos de la Categoria.</param>
    /// <response code="200">Categoria actualizada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe ninguna Categoria con ese id.</response>
    /// <response code="409">Ya existe otra Categoria con ese nombre.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:long}")]
    [SwaggerOperation(Summary = "Actualiza una Categoria (solo Admin)", OperationId = "UpdateCategoria", Tags = ["Categoria"])]
    [SwaggerResponse(200, "Categoria actualizada", typeof(CategoriaDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(404, "No encontrada")]
    [SwaggerResponse(409, "Nombre duplicado")]
    [SwaggerResponse(403, "Sin permisos")]
    public async Task<ActionResult<CategoriaDto>> Update(long id, UpdateCategoriaDto request) =>
        ToActionResult(await service.UpdateAsync(id, request));

    /// <summary>Borra (lógicamente) una Categoria. Requiere rol Admin.</summary>
    /// <param name="id">Identificador de la Categoria.</param>
    /// <response code="204">Categoria borrada.</response>
    /// <response code="404">No existe ninguna Categoria con ese id.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id:long}")]
    [SwaggerOperation(Summary = "Borra una Categoria (solo Admin)", OperationId = "DeleteCategoria", Tags = ["Categoria"])]
    [SwaggerResponse(204, "Categoria borrada")]
    [SwaggerResponse(404, "No encontrada")]
    [SwaggerResponse(403, "Sin permisos")]
    public async Task<ActionResult> Delete(long id)
    {
        var resultado = await service.DeleteAsync(id);
        return resultado.IsSuccess ? NoContent() : ToActionResult(resultado).Result!;
    }

    private ActionResult<T> ToActionResult<T>(Result<T, DomainError> resultado)
    {
        if (resultado.IsSuccess)
            return Ok(resultado.Value);

        return resultado.Error switch
        {
            DomainError.NotFound notFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No encontrado",
                Detail = notFound.Message
            }),
            DomainError.AlreadyExists duplicado => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = duplicado.Message
            }),
            DomainError.Validation validacion => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Solicitud inválida",
                Detail = validacion.Message
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
