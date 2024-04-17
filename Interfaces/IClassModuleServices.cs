using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IClassModuleServices
    {
        Task<ActionResponse> SyncModulesByTeacherIdAsync(int teacherId, int semesterId);
        Task<ActionResponse> GetClassByTeacherAsync(int teacherId, int semesterId, int? pageIndex, int? pageSize);
    }
}