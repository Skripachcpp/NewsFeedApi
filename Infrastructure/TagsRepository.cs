using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class TagsRepository(DpContext dpContext) : ITagsRepository {
  public async Task<IEnumerable<Tag>> GetTags(CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();

    // language=PostgreSQL
    var tags = await connection.QueryAsync<Tag>(new CommandDefinition(
      @"SELECT id as Id, name as Name FROM tag",
      cancellationToken: cancellationToken
    ));

    return tags;
  }

  public async Task DeleteTag(string id, CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();
    var transaction = connection.BeginTransaction();

    try {
      // language=PostgreSQL
      await connection.ExecuteAsync(new CommandDefinition(
        @"DELETE FROM tag WHERE id = @Id",
        new { Id = id },
        transaction,
        cancellationToken: cancellationToken
      ));

      transaction.Commit();
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }

  public async Task<IEnumerable<int>> CreateIfNotExistsAsync(IEnumerable<string> tags,
    CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();
    var transaction = connection.BeginTransaction();

    try {
      var tagList = tags.ToList();

      // language=PostgreSQL
      var ids = await connection.QueryAsync<int>(new CommandDefinition(
        @"
              INSERT INTO tag (name)
              SELECT unnest(@Names)
              ON CONFLICT (name) DO NOTHING;

              SELECT id FROM tag
              WHERE name = ANY(@Names)
            ",
        new { Names = tagList },
        transaction: transaction,
        cancellationToken: cancellationToken
      ));

      transaction.Commit();
      return ids;
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }
}