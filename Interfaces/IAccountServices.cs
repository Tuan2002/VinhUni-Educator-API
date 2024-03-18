using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IAccountServices
    {
        Task<ActionResponse> GetCurrentUserAsync(bool? skipCache = false);
        Task<ActionResponse> FirstChangePasswordAsync(ResetPasswordModel model);
        Task<ActionResponse> ChangePasswordAsync(ChangePasswordModel model);
    }
}