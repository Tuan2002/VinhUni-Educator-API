using VinhUni_Educator_API.Utils;
namespace VinhUni_Educator_API.Interfaces
{
    public interface ISemesterServices
    {
        Task<ActionResponse> SyncSchoolYearsAsync();
        Task<ActionResponse> GetSchoolYearsAsync(int? pageIndex, int? limit, bool skipCache);
        Task<ActionResponse> GetDeletedSchoolYearsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> SyncSemestersAsync();
        Task<ActionResponse> GetSemestersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetDeletedSemestersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetSemestersBySchoolYearAsync(int schoolYearId, bool skipCache);
        Task<ActionResponse> GetSemesterByIdAsync(int semesterId);
        Task<ActionResponse> DeleteSchoolYearAsync(int schoolYearIdId);
        Task<ActionResponse> DeleteSemesterAsync(int semesterId);
        Task<ActionResponse> RestoreSchoolYearAsync(int schoolYearId);
        Task<ActionResponse> RestoreSemesterAsync(int semesterId);
    }
}