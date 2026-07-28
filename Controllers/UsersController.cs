using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techstore_api.DataBase;
using techstore_api.Dtos.Security.Users;
using techstore_api.Services.Interfaces;
using TechStoreApi.Constants;
using TechStoreApi.Dtos.Common;
using Microsoft.EntityFrameworkCore;

namespace techstore_api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios.
    /// Requiere autenticación con token Bearer.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly TiendaDbContext _context;

        public UsersController(IUsersService usersService, TiendaDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 0, sin paginación).</param>
        /// <returns>Lista paginada de usuarios.</returns>
        [HttpGet]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<List<UserDto>>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetList(
            [FromQuery] string searchTerm = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 0)
        {
            var response = await _usersService.GetListAsync(searchTerm, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>Detalles del usuario.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<UserDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 404)]
        public async Task<ActionResult<ResponseDto<UserDto>>> GetOneById(string id)
        {
            var response = await _usersService.GetOneByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<object>), 201)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 500)]
        public async Task<ActionResult<ResponseDto<object>>> Create([FromBody] UserCreateDto dto)
        {
            var response = await _usersService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Edita un usuario existente.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del usuario a editar.</param>
        /// <param name="dto">Objeto de transferencia de datos para editar usuario.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ResponseDto<object>), 500)]
        public async Task<ActionResult<ResponseDto<object>>> Edit(string id, [FromBody] UserEditDto dto)
        {
            var response = await _usersService.EditAsync(dto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Elimina un usuario.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ResponseDto<object>), 500)]
        public async Task<ActionResult> Delete(string id)
        {
            var response = await _usersService.DeleteAsync(id);
            
            if (response.StatusCode == 204)
            {
                return NoContent();
            }
            
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Obtiene todos los vendedores
        /// </summary>
        [HttpGet("sellers")]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
        public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetSellers()
        {
            var sellers = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && 
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "VENDEDOR")))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Roles = new List<string> { "VENDEDOR" }
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<UserDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Vendedores obtenidos exitosamente",
                Data = sellers
            });
        }
    
    }
}