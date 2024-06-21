using StackExchange.Redis;
using System.Text.Json;

namespace Session.Socket.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<T> GetCacheValueAsync<T>(string key)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync(key, jsonValue, expiration);
        }

        public async Task<bool> RemoveCacheValueAsync(string key)
        {
            var db = _connectionMultiplexer.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
    }
}
