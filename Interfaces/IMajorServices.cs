using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IMajorServices
    {
        Task<ActionResponse> SyncMajorAsync();
        Task<ActionResponse> GetMajorsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetMajorByIdAsync(int majorId);
        Task<ActionResponse> DeleteMajorAsync(int majorId);
        Task<ActionResponse> UpdateMajorAsync(int majorId, UpdateMajorModel model);
        Task<ActionResponse> RestoreMajorAsync(int majorId);
        Task<ActionResponse> SearchMajorsAsync(string keyword, int? limit);
    }
}