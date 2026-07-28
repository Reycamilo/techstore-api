using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreApi.Constants;
using TechStoreApi.Dtos.Common;
using techstore_api.Dtos.Security.Roles;
using techstore_api.Services.Interfaces;


namespace techstore_api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles.
    /// Requiere autenticación con token Bearer.
    /// </summary>
    [Route("api/roles")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]


    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        /// <summary>
        /// Obtiene una lista paginada de roles.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 0, sin paginación).</param>
        /// <returns>Lista paginada de roles.</returns>
        [HttpGet]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<List<RoleDto>>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        public async Task<ActionResult<ResponseDto<List<RoleDto>>>> GetList(
            [FromQuery] string searchTerm = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 0)
        {
            var response = await _rolesService.GetListAsync(searchTerm, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Obtiene un rol por su ID.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del rol.</param>
        /// <returns>Detalles del rol.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<RoleDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 404)]
        public async Task<ActionResult<ResponseDto<RoleDto>>> GetOneById(string id)
        {
            var response = await _rolesService.GetOneByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<object>), 201)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 500)]
        public async Task<ActionResult<ResponseDto<object>>> Create([FromBody] RoleCreateDto dto)
        {
            var response = await _rolesService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Edita un rol existente.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del rol a editar.</param>
        /// <param name="dto">Objeto de transferencia de datos para editar rol.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = NombresDeRoles.ADMINISTRADOR)]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ResponseDto<object>), 500)]
        public async Task<ActionResult<ResponseDto<object>>> Edit(string id, [FromBody] RoleEditDto dto)
        {
            var response = await _rolesService.EditAsync(dto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Elimina un rol.
        /// Requiere el rol de ADMINISTRADOR.
        /// </summary>
        /// <param name="id">ID del rol a eliminar.</param>
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
            var response = await _rolesService.DeleteAsync(id);

            if (response.StatusCode == 204)
            {
                return NoContent();
            }

            return StatusCode(response.StatusCode, response);
        }
    }
}