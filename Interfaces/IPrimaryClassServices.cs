
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IPrimaryClassServices
    {
        Task<ActionResponse> SyncPrimaryClassesAsync();
        Task<ActionResponse> GetPrimaryClassesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetDeletedPrimaryClassesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetPrimaryClassByIdAsync(int classId);
        Task<ActionResponse> DeletePrimaryClassAsync(int classId);
        Task<ActionResponse> UpdatePrimaryClassAsync(int classId, UpdateClassModel model);
        Task<ActionResponse> GetPrimaryClassesByCourseAsync(int courseId, int? pageIndex, int? limit);
        Task<ActionResponse> GetPrimaryClassesByProgramAsync(int programId, int? pageIndex, int? limit);
        Task<ActionResponse> SearchPrimaryClassesAsync(string? keyword, int? limit);

    }
}