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
  Lazy<IConnectionMultiplexer> connectionMultiplexer
    ): ICacheRepository {

  private string GetKey(string keyPart, string?[] values) {
    return $"{keyPart}_{string.Join("_", values)}";
  }

  public async Task<T> AutoCashAsync<T>(string keyPart, string?[] values, Func<Task<T>> getValue) {
    var valueFromCash = await GetAsync<T>(keyPart, values);
    if (valueFromCash != null) return valueFromCash;
    
    var value = await getValue();
    if (value == null) return value;
    
    return await SetAsync(keyPart, values, value);
  }
  
  public async Task<T?> GetAsync<T>(string keyPart, string?[] values) {
    var cacheKey = GetKey(keyPart, values);
    
    var cachedJson = await cache.GetStringAsync(cacheKey);
    if (cachedJson != null) {
      var cachedObject = JsonSerializer.Deserialize<T>(cachedJson);
      if (cachedObject != null) return cachedObject;
    }

    return default;
  }

  public async Task<IEnumerable<T>> SetAsync<T>(string keyPart, string?[] values, IEnumerable<T> value) {
    var valueList = value.ToList();
    return await SetAsync(keyPart, values, valueList);;
  }
  
  public async Task<T> SetAsync<T>(string keyPart, string?[] values, T value) {
    var cacheKey = GetKey(keyPart, values);
    
    var cacheOptions = new DistributedCacheEntryOptions {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    var json = JsonSerializer.Serialize(value);
    await cache.SetStringAsync(cacheKey, json, cacheOptions);

    return value;
  }

  public async Task Clear(string keyPart) {
    var instanceName = redisOptions.Value.InstanceName;
    var redisConnection = connectionMultiplexer.Value;

    var database = redisConnection.GetDatabase();
    var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());

    var keysToDelete = new List<RedisKey>();
    await foreach (var key in server.KeysAsync(pattern: instanceName + keyPart + "*"))
      keysToDelete.Add(key);

    if (keysToDelete.Count > 0) 
      await database.KeyDeleteAsync(keysToDelete.ToArray());
  }
}