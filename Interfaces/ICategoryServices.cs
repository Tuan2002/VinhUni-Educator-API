using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface ICategoryServices
    {
        Task<ActionResponse> GetMyCategories(int? pageIndex, int? limit);
        Task<ActionResponse> CreateCategoryAsync(CreateCategoryModel model);
        Task<ActionResponse> GetCategoryByIdAsync(string categoryId);
        Task<ActionResponse> UpdateCategoryAsync(string categoryId, UpdateCategoryModel model);
        Task<ActionResponse> DeleteCategoryAsync(string categoryId);
        Task<ActionResponse> DeleteSharedCategoryAsync(string categoryId);
        Task<ActionResponse> ShareCategoryAsync(string categoryId, ShareCategoryModel model);
        Task<ActionResponse> UnShareCategoryAsync(string categoryId, ShareCategoryModel model);
        Task<ActionResponse> GetMySharingCategoriesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetMySharedCategoriesAsync(int? pageIndex, int? limit);
        Task<ActionResponse> GetTeachersSharedAsync(string categoryId);
    }
}