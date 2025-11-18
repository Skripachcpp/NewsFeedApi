using Domain.Entities;

namespace Domain.Interfaces;

public interface ITagsRepository {
  Task<IEnumerable<Tag>> GetTags(CancellationToken cancellationToken = default);
  Task DeleteTag(string id, CancellationToken cancellationToken = default);
  Task<IEnumerable<int>> CreateIfNotExistsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}