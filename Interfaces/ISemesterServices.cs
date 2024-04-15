using VinhUni_Educator_API.Utils;
namespace VinhUni_Educator_API.Interfaces
{
    public interface ISemesterServices
    {
        Task<ActionResponse> SyncSchoolYearsAsync();
        Task<ActionResponse> GetSchoolYearsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> SyncSemestersAsync();
        Task<ActionResponse> GetSemestersAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetSemestersBySchoolYearAsync(int schoolYearId);

    }
}