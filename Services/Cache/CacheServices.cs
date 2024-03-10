using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Services
{
    public class CacheServices : ICacheServices
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheServices> _logger;
        public CacheServices(IDistributedCache distributedCache, ILogger<CacheServices> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }
        public async Task<T?> GetDataAsync<T>(string cacheKey)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(cacheKey);
                if (data is null) return default;
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception)
            {
                _logger.LogError($"Error occurred while getting data from cache: {cacheKey} at {DateTime.UtcNow}");
                return default;
            }
        }
        public async Task<bool> SetDataAsync<T>(string cacheKey, T value, DateTimeOffset expiration)
        {
            var data = JsonConvert.SerializeObject(value, Formatting.Indented,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            await _distributedCache.SetStringAsync(cacheKey, data, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiration
            });
            return true;
        }
        public async Task<bool> RemoveDataAsync(string cacheKey)
        {
            await _distributedCache.RemoveAsync(cacheKey);
            return true;
        }
    }
}