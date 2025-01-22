using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace ECommerce.ProductService.Service
{
    public interface IRedisCacheService {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;

        public RedisCacheService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
            _redisDb = redisConnection.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _redisDb.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _redisDb.StringSetAsync(key, json, expiry);
        }

        public async Task RemoveAsync(string key)
        {
            await _redisDb.KeyDeleteAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints.First());
            var keys = server.Keys(pattern: pattern);
            foreach (var key in keys)
            {
                await _redisDb.KeyDeleteAsync(key);
            }
        }
    }
}
