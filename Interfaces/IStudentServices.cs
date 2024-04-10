using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IStudentServices
    {
        Task<ActionResponse> GetStudentsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetAImportableStudentsByClassAsync(int classId);
        Task<ActionResponse> ImportStudentByClass(int classId, List<ImportStudentModel> students);
    }
}