using Domain.DTOs;

namespace Domain.Interfaces;

public interface ITagsRepository {
  Task<IEnumerable<TagDto>> GetTags(CancellationToken cancellationToken = default);
  Task DeleteTag(int id, CancellationToken cancellationToken = default);
}