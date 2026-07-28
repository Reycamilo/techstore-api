using AutoMapper;
using Microsoft.AspNetCore.Identity;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Security.Roles;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;
using Microsoft.EntityFrameworkCore;

namespace techstore_api.Services
{
    public class RolesService : IRolesService
    {
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly IMapper _mapper;

        public RolesService(RoleManager<RoleEntity> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<RoleDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var consulta = _roleManager.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                consulta = consulta.Where(r => r.Name!.Contains(searchTerm) || r.Description.Contains(searchTerm));
            }

            var roles = await consulta.ToListAsync();
            var rolesDto = _mapper.Map<List<RoleDto>>(roles);

            return new ResponseDto<List<RoleDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = rolesDto
            };
        }

        public async Task<ResponseDto<RoleDto>> GetOneByIdAsync(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
            {
                return new ResponseDto<RoleDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Rol no encontrado."
                };
            }

            var rolDto = _mapper.Map<RoleDto>(rol);

            return new ResponseDto<RoleDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = rolDto
            };
        }

        public async Task<ResponseDto<object>> CreateAsync(RoleCreateDto dto)
        {
            var rolExiste = await _roleManager.RoleExistsAsync(dto.Name);
            if (rolExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Ya existe un rol con este nombre."
                };
            }

            var rol = _mapper.Map<RoleEntity>(dto);
            var resultado = await _roleManager.CreateAsync(rol);

            if (!resultado.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al crear el rol.",
                    Errors = resultado.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Rol creado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> EditAsync(RoleEditDto dto, string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Rol no encontrado."
                };
            }

            // Verificar si el nombre del rol está siendo cambiado a un nombre de rol existente
            if (rol.Name != dto.Name && await _roleManager.RoleExistsAsync(dto.Name))
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Ya existe otro rol con este nombre."
                };
            }

            rol.Name = dto.Name;
            rol.NormalizedName = _roleManager.NormalizeKey(dto.Name);
            rol.Description = dto.Description!;

            var resultado = await _roleManager.UpdateAsync(rol);

            if (!resultado.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al actualizar el rol.",
                    Errors = resultado.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Rol actualizado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Rol no encontrado."
                };
            }

            var resultado = await _roleManager.DeleteAsync(rol);
            if (!resultado.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al eliminar el rol.",
                    Errors = resultado.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.SIN_CONTENIDO,
                Message = "Rol eliminado exitosamente."
            };
        }
    }
}