using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IExamManagerServices
    {
        Task<ActionResponse> CreateExamAsync(CreateExamModel model);
        Task<ActionResponse> GetExamsAsync(int? pageIndex, int? limit);
        Task<ActionResponse> UpdateExamAsync(string examId, UpdateExamModel model);
        Task<ActionResponse> DeleteExamAsync(string examId);
        Task<ActionResponse> GetQuestionsByExamAsync(string examId);
        Task<ActionResponse> AddQuestionsToExamAsync(string examId, List<string> questionIds);
        Task<ActionResponse> RemoveQuestionsFromExamAsync(string examId, List<string> questionIds);
    }
}