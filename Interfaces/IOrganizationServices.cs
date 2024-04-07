
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IOrganizationServices
    {
        Task<ActionResponse> SyncOrganizationsAsync();
        Task<ActionResponse> GetOrganizationsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetOrganizationByIdAsync(int organizationId);
        Task<ActionResponse> GetDeletedOrganizationsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> DeleteOrganizationAsync(int organizationId);
        Task<ActionResponse> UpdateOrganizationAsync(int organizationId, UpdateOrganizationModel model);
        Task<ActionResponse> RestoreOrganizationAsync(int organizationId);
        Task<ActionResponse> SearchOrganizationsAsync(string? searchKey, int? limit);
    }
}