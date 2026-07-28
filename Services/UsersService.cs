using AutoMapper;
using Microsoft.AspNetCore.Identity;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Security.Users;
using TechStoreApi.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;
using techstore_api.Services.Interfaces;

namespace techstore_api.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly IMapper _mapper;

        public UsersService(UserManager<UserEntity> userManager, RoleManager<RoleEntity> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<UserDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var consulta = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                consulta = consulta.Where(u => u.Email!.Contains(searchTerm) || 
                                         u.FirstName.Contains(searchTerm) || 
                                         u.LastName.Contains(searchTerm));
            }

            var usuarios = await consulta.ToListAsync();
            var usuariosDto = new List<UserDto>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                var usuarioDto = _mapper.Map<UserDto>(usuario);
                usuarioDto.Roles = roles.ToList();
                usuariosDto.Add(usuarioDto);
            }

            return new ResponseDto<List<UserDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = usuariosDto
            };
        }

        public async Task<ResponseDto<UserDto>> GetOneByIdAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return new ResponseDto<UserDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Usuario no encontrado."
                };
            }

            var usuarioDto = _mapper.Map<UserDto>(usuario);
            usuarioDto.Roles = (await _userManager.GetRolesAsync(usuario)).ToList();

            return new ResponseDto<UserDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = usuarioDto
            };
        }

        public async Task<ResponseDto<object>> CreateAsync(UserCreateDto dto)
        {
            var usuarioExiste = await _userManager.FindByEmailAsync(dto.Email);
            if (usuarioExiste != null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Ya existe un usuario con este correo electrónico."
                };
            }

            var usuario = _mapper.Map<UserEntity>(dto);
            usuario.UserName = dto.Email;

            var resultado = await _userManager.CreateAsync(usuario, dto.Password);

            if (!resultado.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al crear el usuario.",
                    Errors = resultado.Errors.Select(e => e.Description).ToList()
                };
            }

            if (dto.Roles != null && dto.Roles.Any())
            {
                foreach (var nombreRol in dto.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(nombreRol))
                    {
                        await _userManager.AddToRoleAsync(usuario, nombreRol);
                    }
                }
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Usuario creado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> EditAsync(UserEditDto dto, string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Usuario no encontrado."
                };
            }

            // Actualizar propiedades del usuario
            usuario.Email = dto.Email;
            usuario.UserName = dto.Email;
            usuario.FirstName = dto.FirstName;
            usuario.LastName = dto.LastName;

            var resultadoActualizacion = await _userManager.UpdateAsync(usuario);

            if (!resultadoActualizacion.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al actualizar el usuario.",
                    Errors = resultadoActualizacion.Errors.Select(e => e.Description).ToList()
                };
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var resultadoCambioContrasena = await _userManager.ResetPasswordAsync(usuario, token, dto.NewPassword);
                if (!resultadoCambioContrasena.Succeeded)
                {
                    return new ResponseDto<object>
                    {
                        Status = false,
                        StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                        Message = "Error al cambiar la contraseña.",
                        Errors = resultadoCambioContrasena.Errors.Select(e => e.Description).ToList()
                    };
                }
            }

            // Actualizar roles
            var rolesExistentes = await _userManager.GetRolesAsync(usuario);
            var rolesARemover = rolesExistentes.Except(dto.Roles).ToList();
            var rolesAAgregar = dto.Roles.Except(rolesExistentes).ToList();

            if (rolesARemover.Any())
            {
                await _userManager.RemoveFromRolesAsync(usuario, rolesARemover);
            }

            if (rolesAAgregar.Any())
            {
                foreach (var nombreRol in rolesAAgregar)
                {
                    if (await _roleManager.RoleExistsAsync(nombreRol))
                    {
                        await _userManager.AddToRoleAsync(usuario, nombreRol);
                    }
                }
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Usuario actualizado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Usuario no encontrado."
                };
            }

            var resultado = await _userManager.DeleteAsync(usuario);
            if (!resultado.Succeeded)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.ERROR_INTERNO_DEL_SERVIDOR,
                    Message = "Error al eliminar el usuario.",
                    Errors = resultado.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.SIN_CONTENIDO,
                Message = "Usuario eliminado exitosamente."
            };
        }
    }
}