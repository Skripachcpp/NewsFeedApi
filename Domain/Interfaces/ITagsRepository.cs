namespace Domain.Interfaces;

public interface ITagsRepository {
  Task<IEnumerable<int>> CreateIfNotExistsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}