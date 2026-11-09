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
/// CRUD de Proveedores. La lectura es pública; crear, actualizar y borrar exige rol Admin
/// (según el esquema acordado en clase para Categoria y Proveedor).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedorController(IServiceProveedor service) : ControllerBase
{
    /// <summary>Lista todos los Proveedores activos.</summary>
    /// <response code="200">Listado devuelto correctamente.</response>
    [AllowAnonymous]
    [HttpGet]
    [OutputCache(Duration = 60, Tags = ["proveedores"])]
    [SwaggerOperation(Summary = "Lista todos los Proveedores", OperationId = "GetAllProveedores", Tags = ["Proveedor"])]
    [SwaggerResponse(200, "Listado de Proveedores", typeof(IEnumerable<ProveedorDto>))]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll() => Ok(await service.GetAllAsync());

    /// <summary>Lista los Proveedores de forma paginada.</summary>
    /// <param name="page">Página solicitada (empieza en 1).</param>
    /// <param name="pageSize">Elementos por página (máximo 100).</param>
    /// <response code="200">Página devuelta correctamente.</response>
    [AllowAnonymous]
    [HttpGet("paged")]
    [OutputCache(Duration = 60, Tags = ["proveedores"])]
    [SwaggerOperation(Summary = "Lista los Proveedores paginados", OperationId = "GetPagedProveedores", Tags = ["Proveedor"])]
    [SwaggerResponse(200, "Página de Proveedores", typeof(PagedResponse<ProveedorDto>))]
    public async Task<ActionResult<PagedResponse<ProveedorDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await service.GetPagedAsync(page, pageSize));

    /// <summary>Consulta un Proveedor por su identificador.</summary>
    /// <param name="id">Identificador del Proveedor.</param>
    /// <response code="200">Proveedor encontrado.</response>
    /// <response code="404">No existe ningún Proveedor con ese id.</response>
    [AllowAnonymous]
    [HttpGet("{id:long}")]
    [OutputCache(Duration = 60, Tags = ["proveedores"])]
    [SwaggerOperation(Summary = "Consulta un Proveedor por id", OperationId = "GetProveedorById", Tags = ["Proveedor"])]
    [SwaggerResponse(200, "Proveedor encontrado", typeof(ProveedorDto))]
    [SwaggerResponse(404, "No encontrado")]
    public async Task<ActionResult<ProveedorDto>> GetById(long id) => ToActionResult(await service.GetByIdAsync(id));

    /// <summary>Busca Proveedores cuyo nombre contenga el texto indicado.</summary>
    /// <param name="nombre">Texto a buscar en el nombre.</param>
    /// <response code="200">Resultado de la búsqueda (puede ser vacío).</response>
    [AllowAnonymous]
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Busca Proveedores por nombre", OperationId = "SearchProveedores", Tags = ["Proveedor"])]
    [SwaggerResponse(200, "Proveedores encontrados", typeof(IEnumerable<ProveedorDto>))]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> Search([FromQuery] string? nombre) =>
        Ok(await service.GetByNameAsync(nombre ?? string.Empty));

    /// <summary>Crea un Proveedor nuevo. Requiere rol Admin.</summary>
    /// <param name="request">Datos del Proveedor a crear.</param>
    /// <response code="201">Proveedor creado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Ya existe un Proveedor con ese nombre.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    [SwaggerOperation(Summary = "Crea un Proveedor (solo Admin)", OperationId = "CreateProveedor", Tags = ["Proveedor"])]
    [SwaggerResponse(201, "Proveedor creado", typeof(ProveedorDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(409, "Nombre duplicado")]
    [SwaggerResponse(403, "Sin permisos")]
    public async Task<ActionResult<ProveedorDto>> Create(CreateProveedorDto request)
    {
        var resultado = await service.AddAsync(request);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : ToActionResult(resultado);
    }

    /// <summary>Actualiza por completo un Proveedor existente. Requiere rol Admin.</summary>
    /// <param name="id">Identificador del Proveedor.</param>
    /// <param name="request">Nuevos datos del Proveedor.</param>
    /// <response code="200">Proveedor actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe ningún Proveedor con ese id.</response>
    /// <response code="409">Ya existe otro Proveedor con ese nombre.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:long}")]
    [SwaggerOperation(Summary = "Actualiza un Proveedor (solo Admin)", OperationId = "UpdateProveedor", Tags = ["Proveedor"])]
    [SwaggerResponse(200, "Proveedor actualizado", typeof(ProveedorDto))]
    [SwaggerResponse(400, "Datos inválidos")]
    [SwaggerResponse(404, "No encontrado")]
    [SwaggerResponse(409, "Nombre duplicado")]
    [SwaggerResponse(403, "Sin permisos")]
    public async Task<ActionResult<ProveedorDto>> Update(long id, UpdateProveedorDto request) =>
        ToActionResult(await service.UpdateAsync(id, request));

    /// <summary>Borra (lógicamente) un Proveedor. Requiere rol Admin.</summary>
    /// <param name="id">Identificador del Proveedor.</param>
    /// <response code="204">Proveedor borrado.</response>
    /// <response code="404">No existe ningún Proveedor con ese id.</response>
    /// <response code="403">El usuario autenticado no tiene rol Admin.</response>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id:long}")]
    [SwaggerOperation(Summary = "Borra un Proveedor (solo Admin)", OperationId = "DeleteProveedor", Tags = ["Proveedor"])]
    [SwaggerResponse(204, "Proveedor borrado")]
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
