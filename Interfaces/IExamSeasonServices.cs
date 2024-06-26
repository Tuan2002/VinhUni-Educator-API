using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IExamSeasonServices
    {
        Task<ActionResponse> CreateExamSeasonAsync(CreateSeasonModel model);
        Task<ActionResponse> GetExamSeasonsByClassAsync(string moduleClassId, int? pageIndex, int? limit);
        Task<ActionResponse> GetExamSeasonByIdAsync(string examSeasonId);
        Task<ActionResponse> GetAssignClassAsync(string examSeasonId);
        Task<ActionResponse> GetParticipantsBySeasonAsync(string examSeasonId, string moduleClassId, int? pageIndex, int? limit);
        Task<ActionResponse> AddClassToExamSeasonAsync(string examSeasonId, List<string> moduleClassIds);
        Task<ActionResponse> RemoveClassFromExamSeasonAsync(string examSeasonId, List<string> moduleClassIds);
        Task<ActionResponse> ForceFinishExamSeasonAsync(string examSeasonId);
        Task<ActionResponse> UpdateExamSeasonAsync(string examSeasonId, UpdateSeasonModel model);
        Task<ActionResponse> DeleteExamSeasonAsync(string examSeasonId, bool foreverDelete = false);
        Task<ActionResponse> ChangeExamAsync(string examSeasonId, string examId);
        Task<ActionResponse> GetStudentExamTurnsAsync(string examSeasonId, int studentId);
        Task<ActionResponse> GetStudentExamResultAsync(string turnId);
        Task<ActionResponse> GetExamSeasonReportAsync(string examSeasonId, string moduleClassId);
    }
}