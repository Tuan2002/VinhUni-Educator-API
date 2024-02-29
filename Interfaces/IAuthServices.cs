using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IAuthServices
    {
        Task<ActionResponse> LoginAsync(LoginModel model);
        Task<ActionResponse> LoginSSOAsync(LoginModel model);
        // Task<ActionResponse> RefreshTokenAsync(string token);
        // Task<ActionResponse> RevokeTokenAsync(string token);
    }
}