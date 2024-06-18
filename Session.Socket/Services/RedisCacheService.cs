using StackExchange.Redis;
using System.Text.Json;

namespace Session.Socket.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<T> GetCacheValueAsync<T>(string key)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration)
        {
            var db = _redis.GetDatabase();
            var jsonValue = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, jsonValue, expiration);
        }

        public async Task<bool> RemoveCacheValueAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
    }
}
