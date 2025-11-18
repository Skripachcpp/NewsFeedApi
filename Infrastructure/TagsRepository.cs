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

  public async Task<IEnumerable<int>> CreateIfNotExistsAsync(IEnumerable<string> tags,
    CancellationToken cancellationToken = default) {
    var tagList = tags.ToList();

    // language=PostgreSQL
    var result = await dpContext.QueryWithTransactionAsync<int>(
      @"
        INSERT INTO tag (name)
        SELECT unnest(@Names)
        ON CONFLICT (name) DO NOTHING;

        SELECT id FROM tag
        WHERE name = ANY(@Names)
      ",
      parameters: new { Names = tagList },
      cancellationToken: cancellationToken
    );

    return result;
  }
}