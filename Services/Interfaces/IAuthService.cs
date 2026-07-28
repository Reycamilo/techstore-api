
using techstore_api.Dtos.Security.Auth;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);

    }
}