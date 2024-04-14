using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface ITeacherServices
    {
        Task<ActionResponse> GetTeachersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetTeacherByIdAsync(int teacherId);
        Task<ActionResponse> GetTeacherByCodeAsync(int teacherCode);
        Task<ActionResponse> GetTeachersByOrganizationAsync(int organizationId);
        Task<ActionResponse> GetDeletedTeachersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> DeleteTeacherAsync(int teacherId);
        Task<ActionResponse> RestoreTeacherAsync(int teacherId);
        Task<ActionResponse> UpdateTeacherAsync(int teacherId, UpdateTeacherModel model);
        Task<ActionResponse> GetImportableTeachersByOrganization(int organizationId);
        Task<ActionResponse> ImportTeachersByOrganizationAsync(int organizationId, List<ImportTeacherModel> teachers);
        Task<ActionResponse> SearchTeacherAsync(string? searchKey, int? limit);

    }
}