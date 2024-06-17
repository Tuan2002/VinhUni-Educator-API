using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface ICourseServices
    {
        Task<ActionResponse> SyncCoursesAsync();
        Task<ActionResponse> GetCoursesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetDeletedCoursesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetCourseByIdAsync(int courseDd);
        Task<ActionResponse> DeleteCourseAsync(int courseId);
        Task<ActionResponse> RestoreCourseAsync(int courseId);
        Task<ActionResponse> UpdateCourseAsync(int courseId, UpdateCourseModel model);
        Task<ActionResponse> SearchCourseAsync(string? keyword, int? limit);
    }
}