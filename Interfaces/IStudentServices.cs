using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IStudentServices
    {
        Task<ActionResponse> GetStudentsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetDeletedStudentsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetImportableStudentsByClassAsync(int classId);
        Task<ActionResponse> ImportStudentByClass(int classId, List<ImportStudentModel> students);
        Task<ActionResponse> GetStudentsByClassAsync(int classId);
        Task<ActionResponse> GetStudentByIdAsync(int studentId);
        Task<ActionResponse> GetStudentByCodeAsync(string studentCode);
        Task<ActionResponse> UpdateStudentAsync(int studentId, UpdateStudentModel model);
        Task<ActionResponse> DeleteStudentAsync(int studentId);
        Task<ActionResponse> RestoreStudentAsync(int studentId);
        Task<ActionResponse> LinkUserAccountAsync(int studentId, string userId);
        Task<ActionResponse> UnlinkUserAccountAsync(int studentId);
        Task<ActionResponse> SearchStudentAsync(string? searchKey, int? limit);
    }
}