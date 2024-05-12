using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IStudentExamServices
    {
        Task<ActionResponse> GetExamSeasonAsync(string examSeasonCode);
        Task<ActionResponse> GetExamTurnsAsync(string examSeasonCode);
        Task<ActionResponse> StartExamAsync(string examSeasonCode, string moduleClassId);
        Task<ActionResponse> ForceFinishExamTurnAsync(string examSeasonCode, string turnId);
        Task<ActionResponse> GetExamQuestionsAsync(string seasonCode, int? pageIndex, int? limit);
        Task<ActionResponse> SubmitExamAnswersAsync(string seasonCode, string turnId, List<SubmitQuestionModel> submitQuestions);
        Task<ActionResponse> GetExamResultAsync(string seasonCode, string turnId);
        Task<ActionResponse> ResumeExamTurnAsync(string seasonCode, string turnId);
    }
}