using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IQuestionManagerServices
    {
        Task<ActionResponse> ImportQuestionsAsync(string questionKitId, List<CreateQuestionModel> questions);
        Task<ActionResponse> GetQuestionsNByKitAsync(string questionKitId, int? pageIndex, int? limit);
        Task<ActionResponse> GetQuestionByIdAsync(string questionId);
        Task<ActionResponse> UpdateQuestionsByKitAsync(string questionKitId, List<UpdateQuestionModel> questionsToUpdate);
        Task<ActionResponse> DeleteQuestionsAsync(string questionKitId, List<string> questionIds);
    }
}