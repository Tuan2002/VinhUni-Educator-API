using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IQuestionServices
    {
        Task<ActionResponse> CreateQuestionKitAsync(CreateQuestionKitModel questionKit);
        Task<ActionResponse> GetQuestionKitsAsync(string categoryId, int? pageIndex, int? limit);
        Task<ActionResponse> GetQuestionKitByIdAsync(string questionKitId);
        Task<ActionResponse> UpdateQuestionKitAsync(string questionKitId, UpdateQuestionKitModel questionKit);
        Task<ActionResponse> DeleteQuestionKitAsync(string questionKitId);
        Task<ActionResponse> ShareQuestionKitAsync(string questionKitId);
        Task<ActionResponse> UnShareQuestionKitAsync(string questionKitId);

    }
}