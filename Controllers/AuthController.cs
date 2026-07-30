
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techstore_api.Dtos.Security.Auth;
using techstore_api.Dtos.Security.Users;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Controllers
{
    /// <summary>
    /// Controlador para la autenticación de usuarios.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersService _usersService;

        public AuthController(IAuthService authService, IUsersService usersService)
        {
            _authService = authService;
            _usersService = usersService;
        }

        /// <summary>
        /// Inicia sesión de un usuario y devuelve un token JWT.
        /// </summary>
        /// <param name="dto">Objeto de transferencia de datos de inicio de sesión.</param>
        /// <returns>Respuesta con el token JWT si la autenticación es exitosa.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Registra un nuevo cliente (público)
        /// </summary>
        /// <param name="dto">Datos del cliente a registrar</param>
        /// <returns>Respuesta de éxito o error</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseDto<object>), 201)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        public async Task<ActionResult<ResponseDto<object>>> Register([FromBody] UserCreateDto dto)
        {
            // Forzar el rol CLIENTE, ignorando cualquier otro rol enviado
            dto.Roles = new List<string> { "CLIENTE" };
            var response = await _usersService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}