using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class TagsRepository(DpContext dpContext) : ITagsRepository {
  public async Task<IEnumerable<Tag>> GetTags(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<Tag>(
      @"SELECT id as Id, name as Name FROM tag",
      cancellationToken: cancellationToken
    );

    return result;
  }

  public async Task DeleteTag(int id, CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    await dpContext.ExecuteWithTransactionAsync(
      @"DELETE FROM tag WHERE id = @Id",
      parameters: new { Id = id },
      cancellationToken: cancellationToken
    );
  }
}