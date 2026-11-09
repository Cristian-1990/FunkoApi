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
/// CRUD de Funkos. La lectura es pública; escribir exige estar autenticado y
/// borrar exige además el rol Admin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FunkoController(IServiceFunko service) : ControllerBase
{
    /// <summary>Lista todos los Funkos activos.</summary>
    /// <returns>Colección de Funkos.</returns>
    /// <response code="200">Listado devuelto correctamente.</response>
    [AllowAnonymous]
    [HttpGet]
    [OutputCache(Duration = 60, Tags = ["funkos"])]
    [SwaggerOperation(Summary = "Lista todos los Funkos", OperationId = "GetAllFunkos", Tags = ["Funko"])]
    [SwaggerResponse(200, "Listado de Funkos", typeof(IEnumerable<FunkoDto>))]
    public async Task<ActionResult<IEnumerable<FunkoDto>>> GetAll() => Ok(await service.GetAllAsync());

    /// <summary>Lista los Funkos de forma paginada.</summary>
    /// <param name="page">Página solicitada (empieza en 1).</param>
    /// <param name="pageSize">Elementos por página (máximo 100).</param>
    /// <returns>Página de Funkos con sus metadatos.</returns>
    /// <response code="200">Página devuelta correctamente.</response>
    [AllowAnonymous]
    [HttpGet("paged")]
    [OutputCache(Duration = 60, Tags = ["funkos"])]
    [SwaggerOperation(Summary = "Lista los Funkos paginados", OperationId = "GetPagedFunkos", Tags = ["Funko"])]
    [SwaggerResponse(200, "Página de Funkos", typeof(PagedResponse<FunkoDto>))]
    public async Task<ActionResult<PagedResponse<FunkoDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await service.GetPagedAsync(page, pageSize));

    /// <summary>Consulta un Funko por su identificador.</summary>
    /// <param name="id">Identificador del Funko.</param>
    /// <response code="200">Funko encontrado.</response>
    /// <response code="404">No existe ningún Funko con ese id.</response>
    [AllowAnonymous]
    [HttpGet("{id:long}")]
    [OutputCache(Duration = 60, Tags = ["funkos"])]
    [SwaggerOperation(Summary = "Consulta un Funko por id", OperationId = "GetFunkoById", Tags = ["Funko"])]
    [SwaggerResponse(200, "Funko encontrado", typeof(FunkoDto))]
    [SwaggerResponse(404, "No encontrado")]
    public async Task<ActionResult<FunkoDto>> GetById(long id) => ToActionResult(await service.GetByIdAsync(id));

    /// <summary>Busca Funkos cuyo nombre contenga el texto indicado.</summary>
    /// <param name="nombre">Texto a buscar en el nombre.</param>
    /// <response code="200">Resultado de la búsqueda (puede ser vacío).</response>
    [AllowAnonymous]
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Busca Funkos por nombre", OperationId = "SearchFunkos", Tags = ["Funko"])]
    [SwaggerResponse(200, "Funkos encontrados", typeof(IEnumerable<FunkoDto>))]
    public async Task<ActionResult<IEnumerable<FunkoDto>>> Search([FromQuery] string? nombre) =>
        Ok(await service.GetByNameAsync(nombre ?? string.Empty));

    /// <summary>Crea un Funko nuevo.</summary>
    /// <param name="request">Datos del Funko a crear.</param>
    /// <response code="201">Funko creado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Ya existe un Funko con ese nombre.</response>
    [HttpPost]
    [SwaggerOperation(Summary = "Crea un Funko", OperationId = "CreateFunko", Tags = ["Funko"])]
    [SwaggerResponse(201, "Funko creado", typeof(FunkoDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(409, "Nombre duplicado")]
    public async Task<ActionResult<FunkoDto>> Create(CreateFunkoDto request)
    {
        var resultado = await service.AddAsync(request);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : ToActionResult(resultado);
    }

    /// <summary>Actualiza por completo un Funko existente.</summary>
    /// <param name="id">Identificador del Funko.</param>
    /// <param name="request">Nuevos datos del Funko.</param>
    /// <response code="200">Funko actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe ningún Funko con ese id.</response>
    /// <response code="409">Ya existe otro Funko con ese nombre.</response>
    [HttpPut("{id:long}")]
    [SwaggerOperation(Summary = "Actualiza un Funko", OperationId = "UpdateFunko", Tags = ["Funko"])]
    [SwaggerResponse(200, "Funko actualizado", typeof(FunkoDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(404, "No encontrado")]
    [SwaggerResponse(409, "Nombre duplicado")]
    public async Task<ActionResult<FunkoDto>> Update(long id, UpdateFunkoDto request) =>
        ToActionResult(await service.UpdateAsync(id, request));

    /// <summary>Actualiza solo el precio de un Funko.</summary>
    /// <param name="id">Identificador del Funko.</param>
    /// <param name="precio">Nuevo precio.</param>
    /// <response code="200">Precio actualizado.</response>
    /// <response code="400">Precio inválido (negativo).</response>
    /// <response code="404">No existe ningún Funko con ese id.</response>
    [HttpPatch("{id:long}")]
    [SwaggerOperation(Summary = "Actualiza solo el precio de un Funko", OperationId = "UpdateFunkoPrice", Tags = ["Funko"])]
    [SwaggerResponse(200, "Precio actualizado", typeof(FunkoDto))]
    [SwaggerResponse(400, "Precio inválido")]
    [SwaggerResponse(404, "No encontrado")]
    public async Task<ActionResult<FunkoDto>> UpdatePrice(long id, [FromBody] decimal precio) =>
        ToActionResult(await service.UpdatePriceAsync(id, precio));

    /// <summary>Borra (lógicamente) un Funko. Requiere rol Admin.</summary>
    /// <param name="id">Identificador del Funko.</param>
    /// <response code="204">Funko borrado.</response>
    /// <response code="404">No existe ningún Funko con ese id.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id:long}")]
    [SwaggerOperation(Summary = "Borra un Funko (solo Admin)", OperationId = "DeleteFunko", Tags = ["Funko"])]
    [SwaggerResponse(204, "Funko borrado")]
    [SwaggerResponse(404, "No encontrado")]
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
