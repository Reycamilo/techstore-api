
using techstore_api.Dtos.Security.Roles;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface IRolesService
    {
        Task<ResponseDto<List<RoleDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0);
        Task<ResponseDto<RoleDto>> GetOneByIdAsync(string id);
        Task<ResponseDto<object>> CreateAsync(RoleCreateDto dto);
        Task<ResponseDto<object>> EditAsync(RoleEditDto dto, string id);
        Task<ResponseDto<object>> DeleteAsync(string id);
    }
}