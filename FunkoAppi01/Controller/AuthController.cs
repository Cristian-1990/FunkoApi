using CSharpFunctionalExtensions;
using FunkoAppi01.Dto;
using FunkoAppi01.Errors;
using FunkoAppi01.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FunkoAppi01.Controller;

/// <summary>
/// Autenticación: registro, login y perfil del usuario autenticado.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Registra un usuario nuevo.</summary>
    /// <param name="request">Datos de registro.</param>
    /// <response code="201">Usuario creado, con su token de acceso.</response>
    /// <response code="409">Ya existe un usuario con ese email.</response>
    [HttpPost("signup")]
    [SwaggerOperation(Summary = "Registra un usuario nuevo", OperationId = "Signup", Tags = ["Auth"])]
    [SwaggerResponse(201, "Usuario creado", typeof(AuthResponseDto))]
    [SwaggerResponse(409, "Email ya registrado")]
    public async Task<ActionResult<AuthResponseDto>> Signup(SignupDto request)
    {
        var resultado = await authService.SignupAsync(request);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentUser), resultado.Value)
            : ToActionResult(resultado);
    }

    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    /// <param name="request">Credenciales.</param>
    /// <response code="200">Login correcto, con el token de acceso.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Inicia sesión", OperationId = "Login", Tags = ["Auth"])]
    [SwaggerResponse(200, "Login correcto", typeof(AuthResponseDto))]
    [SwaggerResponse(401, "Credenciales inválidas")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request) =>
        ToActionResult(await authService.LoginAsync(request));

    /// <summary>Devuelve los datos del usuario autenticado, leídos del propio token.</summary>
    /// <response code="200">Datos del usuario autenticado.</response>
    /// <response code="401">Sin token o token inválido.</response>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Perfil del usuario autenticado", OperationId = "Me", Tags = ["Auth"])]
    [SwaggerResponse(200, "Datos del usuario")]
    [SwaggerResponse(401, "No autenticado")]
    public ActionResult GetCurrentUser() => Ok(new
    {
        Id = User.FindFirst("sub")?.Value,
        Username = User.FindFirst("username")?.Value,
        Email = User.FindFirst("email")?.Value,
        Role = User.FindFirst("role")?.Value
    });

    private ActionResult<T> ToActionResult<T>(Result<T, DomainError> resultado)
    {
        if (resultado.IsSuccess)
            return Ok(resultado.Value);

        return resultado.Error switch
        {
            DomainError.Unauthorized unauthorized => Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "No autorizado",
                Detail = unauthorized.Message
            }),
            DomainError.Conflict conflicto => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = conflicto.Message
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
