using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using VendorRiskAPI.Application.Interfaces;

namespace VendorRiskAPI.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string _instanceName;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _instanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "VendorRisk_";
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var redisKey = GetKey(key);
            var value = await _database.StringGetAsync(redisKey);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var redisKey = GetKey(key);
            var serializedValue = JsonSerializer.Serialize(value);
            var expiry = expiration ?? TimeSpan.FromMinutes(30);

            await _database.StringSetAsync(redisKey, serializedValue, expiry);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var redisKey = GetKey(key);
            await _database.KeyDeleteAsync(redisKey);
            _logger.LogDebug("Removed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var redisKey = GetKey(key);
            return await _database.KeyExistsAsync(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
            return false;
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            var pattern = GetKey($"{prefix}*");
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();

            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("Removed {Count} keys with prefix: {Prefix}", keys.Length, prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys by prefix: {Prefix}", prefix);
        }
    }

    private string GetKey(string key) => $"{_instanceName}{key}";
}
