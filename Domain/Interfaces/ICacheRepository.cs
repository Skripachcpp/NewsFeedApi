namespace Domain.Interfaces;

public interface ICacheRepository {
  Task<T?> GetAsync<T>(string keyPart, string?[] keyValues);
  Task<T> SetAsync<T>(string keyPart, string?[] values, T value);
  Task<IEnumerable<T>> SetAsync<T>(string keyPart, string?[] values, IEnumerable<T> value);
  Task<T> AutoCashAsync<T>(string keyPart, string?[] values, Func<Task<T>> getValue);
  Task Clear(string keyPart);

}