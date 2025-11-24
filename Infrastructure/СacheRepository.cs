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

    public async Task<T> AutoCacheAsync<T>(string keyPart, string?[] values, Func<Task<T>> getValue)
    {
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));

        var valueFromCash = await this.GetAsync<T>(keyPart, values).ConfigureAwait(false);
        if (valueFromCash != null)
            return valueFromCash;

        var value = await getValue().ConfigureAwait(false);
        if (value == null)
            return value;

        return await this.SetAsync(keyPart, values, value).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string keyPart, string?[] values)
    {
        var cacheKey = this.GetKey(keyPart, values);

        var cachedJson = await cache.GetStringAsync(cacheKey).ConfigureAwait(false);
        if (cachedJson != null)
        {
            // объект может поменяться и не будет десериализован
            try
            {
                var cachedObject = JsonSerializer.Deserialize<T>(cachedJson);
                if (cachedObject != null)
                    return cachedObject;
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    public async Task<IEnumerable<T>> SetAsync<T>(string keyPart, string?[] values, IEnumerable<T> value)
    {
        var valueList = value.ToList();
        return await this.SetAsync(keyPart, values, valueList).ConfigureAwait(false);
    }

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

    public async Task Clear(string keyPart)
    {
        var instanceName = redisOptions.Value.InstanceName;
        var redisConnection = connectionMultiplexer.Value;

        var database = redisConnection.GetDatabase();
        var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());

        var keysToDelete = new List<RedisKey>();

        // удаляем ключи кусками // можно вообще на Lua скрипт перейти но это будет уже перебор для пет проекта
        await foreach (var key in server.KeysAsync(pattern: instanceName + keyPart + "*", pageSize: 100, pageOffset: 0).ConfigureAwait(false))
        {
            keysToDelete.Add(key);

            if (keysToDelete.Count >= 100)
            {
                await database.KeyDeleteAsync(keysToDelete.ToArray()).ConfigureAwait(false);
                keysToDelete.Clear();
            }
        }

        if (keysToDelete.Count > 0)
        {
            await database.KeyDeleteAsync(keysToDelete.ToArray()).ConfigureAwait(false);
        }
    }
}
