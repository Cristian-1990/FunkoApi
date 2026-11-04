using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Entity;
using FunkoAppi01.Errors;
using FunkoAppi01.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll() => Ok(await service.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoriaDto>> GetById(long id) => ToActionResult(await service.GetByIdAsync(id));

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> Search([FromQuery] string? nombre) =>
        Ok(await service.GetByNameAsync(nombre ?? string.Empty));

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CreateCategoriaDto request)
    {
        var resultado = await service.AddAsync(request);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : ToActionResult(resultado);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoriaDto>> Update(long id, UpdateCategoriaDto request) =>
        ToActionResult(await service.UpdateAsync(id, request));

    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id:long}")]
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
