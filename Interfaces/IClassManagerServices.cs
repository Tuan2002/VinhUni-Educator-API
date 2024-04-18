using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IClassManagerServices
    {
        Task<ActionResponse> AddStudentToClassAsync(string moduleClassId, int studentId);
        Task<ActionResponse> RemoveStudentFromClassAsync(string moduleClassId, int studentId);
    }
}