using Domain.DTOs;
using Domain.Entities;

namespace Domain.Interfaces;

public interface ITagsRepository {
  Task<IEnumerable<TagDto>> GetTags(CancellationToken cancellationToken = default);
  Task DeleteTag(int id, CancellationToken cancellationToken = default);
}