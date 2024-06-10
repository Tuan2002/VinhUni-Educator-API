using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IClassModuleServices
    {
        Task<ActionResponse> SyncClassModulesByTeacherIdAsync(int teacherId, int semesterId);
        Task<ActionResponse> SyncClassModulesByTeacher(int semesterId);
        Task<ActionResponse> GetClassByTeacherAsync(int teacherId, int semesterId, int? pageIndex, int? pageSize, bool? cached = true);
        Task<ActionResponse> GetClassByStudentAsync(int studentId, int semesterId, int? pageIndex, int? pageSize, bool? cached = true);
        Task<ActionResponse> GetClassModulesAsync(int semesterId, int? pageIndex, int? limit);
        Task<ActionResponse> SyncClassModuleStudentsAsync(string moduleClassId);
        Task<ActionResponse> GetStudentsByModuleClass(string moduleClassId);
        Task<ActionResponse> GetClassModuleAsync(string moduleClassId);
    }
}