using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class TagsRepository(DpContext dpContext) : ITagsRepository {
  public async Task<IEnumerable<TagDto>> GetTags(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<TagDto>(
      @"SELECT id as Id, name as Name FROM tag",
      cancellationToken: cancellationToken
    );

    return result;
  }

  public async Task DeleteTag(int id, CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    await dpContext.ExecuteAsync(
      @"DELETE FROM tag WHERE id = @Id",
      parameters: new { Id = id },
      cancellationToken: cancellationToken
    );
  }
}