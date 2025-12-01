using System.Text.Json;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public class CacheRepository(
    IDistributedCache cache,
    IOptions<RedisCacheOptions> redisOptions,
    Lazy<IConnectionMultiplexer> connectionMultiplexer): ICacheRepository
{
    private string GetKey(string keyPart, string?[] values)
    {
        return $"{keyPart}_{string.Join("_", values)}";
    }

    /// <inheritdoc/>
    public async Task<T> AutoCacheAsync<T>(string keyPart, string?[] values, Func<Task<T>> getValue)
    {
        if (getValue is null) throw new ArgumentNullException(nameof(getValue));

        var valueFromCash = await this.GetAsync<T>(keyPart, values).ConfigureAwait(false);
        if (valueFromCash is not null)
            return valueFromCash;

        var value = await getValue().ConfigureAwait(false);
        if (value is null)
            return value;

        return await this.SetAsync(keyPart, values, value).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string keyPart, string?[] values)
    {
        var cacheKey = this.GetKey(keyPart, values);

        var cachedJson = await cache.GetStringAsync(cacheKey).ConfigureAwait(false);
        if (cachedJson is not null)
        {
            // объект может поменяться и не будет десериализован
            try
            {
                var cachedObject = JsonSerializer.Deserialize<T>(cachedJson);
                if (cachedObject is not null)
                    return cachedObject;
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> SetAsync<T>(string keyPart, string?[] values, IEnumerable<T> value)
    {
        var valueList = value.ToList();
        return await this.SetAsync(keyPart, values, valueList).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<T> SetAsync<T>(string keyPart, string?[] values, T value)
    {
        var cacheKey = this.GetKey(keyPart, values);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        };

        var json = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(cacheKey, json, cacheOptions).ConfigureAwait(false);

        return value;
    }

    /// <inheritdoc/>
    public async Task Clear(string keyPart)
    {
        var instanceName = redisOptions.Value.InstanceName;
        var redisConnection = connectionMultiplexer.Value;
        var database = redisConnection.GetDatabase();

        var pattern = instanceName + keyPart + "*";

        await database.ScriptEvaluateAsync(
            @"
                local pattern = ARGV[1]
                local deleted = 0
                local cursor = '0'
                local maxIterations = 1000
                
                repeat
                    local result = redis.call('SCAN', cursor, 'MATCH', pattern, 'COUNT', 100)
                    cursor = result[1]
                    local keys = result[2]
                    
                    if #keys > 0 then
                        deleted = deleted + redis.call('DEL', unpack(keys))
                    end
                    
                    maxIterations = maxIterations - 1
                until cursor == '0' or maxIterations <= 0
                
                return deleted
            ",
            keys: Array.Empty<RedisKey>(),
            values: new[] { (RedisValue)pattern })
        .ConfigureAwait(false);
    }
}
