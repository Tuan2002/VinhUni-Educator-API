
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IOrganizationServices
    {
        Task<ActionResponse> SyncOrganizationsAsync();
        // Task<ActionResponse> UpdateOrganizationAsync(int id, OrganizationModel model);
        // Task<ActionResponse> DeleteOrganizationAsync(int id);
        // Task<ActionResponse> GetOrganizationAsync(int id);
        // Task<ActionResponse> GetAllOrganizationsAsync();
    }
}