
namespace VinhUni_Educator_API.Interfaces
{
    public interface ICacheServices
    {
        Task<T?> GetDataAsync<T>(string cacheKey);
        Task<bool> SetDataAsync<T>(string cacheKey, T value, DateTimeOffset expiration);
        Task<bool> RemoveDataAsync(string cacheKey);
    }
}