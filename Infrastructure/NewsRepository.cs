using System.Data;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class NewsRepository(DpContext dpContext) : INewsRepository {
  // language=PostgreSQL
  private const string BaseSelectQuerySelect = @"
   SELECT
    na.id as Id,
    na.title as Title,
    na.content as Content,
    na.summary as Summary,
    na.publication_date as PublicationDate,
    na.user_name as UserName,
    COALESCE(array_agg(t.name) FILTER (WHERE t.name IS NOT NULL), ARRAY[]::text[]) as Tags
  FROM news_article na 
  LEFT JOIN news_article_tag nat ON na.id = nat.news_article_id
  LEFT JOIN tag t ON nat.tag_id = t.id
  ";


  private const string BaseSelectQueryEnding = @"
  GROUP BY na.id, na.title, na.content, na.summary, na.publication_date, na.user_name
  ORDER BY na.publication_date DESC
  ";

  public async Task<IEnumerable<NewsArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<NewsArticleDto>(
      $@"
        {BaseSelectQuerySelect}
        {BaseSelectQueryEnding}
      ",
      cancellationToken: cancellationToken);

    return result;
  }

  private async Task<NewsArticleDto?> QueryGetArticleAsync(
    int id,
    IDbConnection connection,
    CancellationToken cancellationToken = default,
    IDbTransaction? transaction = default) {
    // language=PostgreSQL
    var result = await connection.QueryFirstOrDefaultAsync<NewsArticleDto>(new CommandDefinition(
      $@"{BaseSelectQuerySelect}
        WHERE na.id = @Id
        {BaseSelectQueryEnding}",
      parameters: new { Id = id },
      cancellationToken: cancellationToken,
      transaction: transaction
    ));

    return result;
  }


  public async Task<NewsArticleDto?> GetArticleAsync(int id, CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();
    var result = await QueryGetArticleAsync(id, connection, cancellationToken);
    return result;
  }

  private async Task<IEnumerable<int>> QueryTagCreateIfNotExistsAsync(
    IEnumerable<string> tags,
    IDbConnection connection,
    CancellationToken cancellationToken = default,
    IDbTransaction? transaction = default) {
    var tagList = tags.ToList();
    if (!tagList.Any()) return [];

    // language=PostgreSQL
    var result = await connection.QueryAsync<int>(new CommandDefinition(
      @"
        INSERT INTO tag (name)
        SELECT unnest(@Names)
        ON CONFLICT (name) DO NOTHING;

        SELECT id FROM tag
        WHERE name = ANY(@Names)
      ",
      parameters: new { Names = tagList },
      transaction: transaction,
      cancellationToken: cancellationToken));

    return result;
  }

  private async Task QueryAttachTagsToArticleAsyncAsync(
    IEnumerable<int> tagIds,
    int articleId,
    IDbConnection connection,
    CancellationToken cancellationToken = default,
    IDbTransaction? transaction = default) {
    var tagIdsList = tagIds.ToList();
    if (!tagIdsList.Any()) return;

    // language=PostgreSQL
    await connection.ExecuteAsync(new CommandDefinition(
      @"
            INSERT INTO news_article_tag (news_article_id, tag_id)
            SELECT @ArticleId, unnest(@TagIds)
            ON CONFLICT (news_article_id, tag_id) DO NOTHING;           
          ",
      parameters: new { ArticleId = articleId, TagIds = tagIdsList },
      cancellationToken: cancellationToken,
      transaction: transaction
    ));
  }

  public async Task<NewsArticleDto> CreateArticleAsync(NewsArticleCreateDto article,
    CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();
    using var transaction = connection.BeginTransaction();

    try {
      // language=PostgreSQL
      var articleId = await connection.QuerySingleAsync<int>(new CommandDefinition(
        @"
        INSERT INTO news_article (title, content, summary, publication_date, user_id, user_name)
        VALUES (@Title, @Content, @Summary, @PublicationDate, @UserId, @UserName)
        RETURNING id
        ",
        parameters: new {
          Title = article.Title,
          Content = article.Content,
          Summary = article.Summary,
          PublicationDate = article.PublicationDate,
          UserId = article.UserId,
          UserName = article.UserName
        },
        transaction: transaction,
        cancellationToken: cancellationToken
      ));

      var tagIds = (await QueryTagCreateIfNotExistsAsync(
        article.Tags ?? [],
        connection: connection,
        cancellationToken: cancellationToken,
        transaction: transaction
      ));

      await QueryAttachTagsToArticleAsyncAsync(
        tagIds,
        articleId,
        connection,
        transaction: transaction,
        cancellationToken: cancellationToken
      );

      var articleNext = await QueryGetArticleAsync(articleId, connection, cancellationToken, transaction);
      if (articleNext == null) throw new InvalidOperationException("Не удалось загрузить созданную статью");

      transaction.Commit();

      return articleNext;
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }

  public async Task DeleteArticleAsync(int id, CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    await dpContext.ExecuteWithTransactionAsync(
      @"DELETE FROM news_article WHERE id = @Id",
      parameters: new { Id = id },
      cancellationToken: cancellationToken
    );
  }
}