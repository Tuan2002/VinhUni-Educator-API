using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IProgramServices
    {
        Task<ActionResponse> SyncProgramsAsync();
        Task<ActionResponse> GetProgramsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetProgramByIdAsync(int programId);
        Task<ActionResponse> DeleteProgramAsync(int programId);
        Task<ActionResponse> RestoreProgramAsync(int programId);
        Task<ActionResponse> UpdateProgramAsync(int programId, UpdateProgramModel model);
        Task<ActionResponse> GetProgramsByMajorAsync(int majorId, int? pageIndex, int? limit);
        Task<ActionResponse> GetProgramsByCourseAsync(int courseId, int? pageIndex, int? limit);
        Task<ActionResponse> SearchProgramsAsync(string keyword, int? limit);
    }
}