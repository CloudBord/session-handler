namespace Session.Socket.Services
{
    public interface IRedisCacheService
    {
        Task<T> GetCacheValueAsync<T>(string key);
        Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration);
        Task<bool> RemoveCacheValueAsync(string key);
    }
}
