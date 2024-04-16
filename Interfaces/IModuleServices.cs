using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IModuleServices
    {
        Task<ActionResponse> SyncModulesAsync();
        Task<ActionResponse> GetModulesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetDeletedModulesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetModuleByIdAsync(int moduleId);
        Task<ActionResponse> GetModuleByCodeAsync(string moduleCode);
        Task<ActionResponse> DeleteModuleAsync(int moduleId);
        Task<ActionResponse> RestoreModuleAsync(int moduleId);
        Task<ActionResponse> SearchModulesAsync(string? keyword, int? limit);
    }
}