using Dapper;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class TagsRepository(DpContext dpContext) : ITagsRepository {
  public async Task<IEnumerable<int>> CreateIfNotExistsAsync(IEnumerable<string> tags,
    CancellationToken cancellationToken = default) {
    using var connection = dpContext.Connection();
    connection.Open();
    var transaction = connection.BeginTransaction();

    try {
      var tagList = tags.ToList();

      // language=PostgreSQL
      var ids = await connection.QueryAsync<int>(
        @"
              INSERT INTO tag (name)
              SELECT unnest(@Names)
              ON CONFLICT (name) DO NOTHING;

              SELECT id FROM tag
              WHERE name = ANY(@Names)
            ",
        new { Names = tagList },
        transaction
      );

      transaction.Commit();
      return ids;
    }
    catch {
      transaction.Rollback();
    }

    return [];
  }
}