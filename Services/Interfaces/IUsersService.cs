using techstore_api.Dtos.Security.Users;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface IUsersService
    {
        Task<ResponseDto<List<UserDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0);
        Task<ResponseDto<UserDto>> GetOneByIdAsync(string id);
        Task<ResponseDto<object>> CreateAsync(UserCreateDto dto);
        Task<ResponseDto<object>> EditAsync(UserEditDto dto, string id);
        Task<ResponseDto<object>> DeleteAsync(string id);
    }
}