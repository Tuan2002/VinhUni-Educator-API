
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IUserServices
    {
        Task<ActionResponse> SyncUserFromSSO(string token);
        Task<ActionResponse> CreateUserAsync(CreateUserModel model);
        Task<ActionResponse> GetUsersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetUserByIdAsync(string userId);
        Task<ActionResponse> GetUserByNameAsync(string userName);
        Task<ActionResponse> GetDeletedUsersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> DeleteUserAsync(string userId);
        Task<ActionResponse> RestoreUserAsync(string userId);
        Task<ActionResponse> UpdateUserAsync(string userId, UpdateProfileModel model);
        Task<ActionResponse> ResetUserPasswordAsync(string userId, ResetPasswordModel model);
        Task<ActionResponse> SearchUsersAsync(string keyword, int? limit);
    }
}