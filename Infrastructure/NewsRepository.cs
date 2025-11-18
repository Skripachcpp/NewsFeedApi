using System.Data;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class NewsRepository(DpContext dpContext) : INewsRepository {
  // language=PostgreSQL
  private const string BaseSelectQuery = @"
   SELECT
    id as Id,
    title as Title,
    content as Content,
    summary as Summary,
    publication_date as PublicationDate,
    user_name as UserName
  FROM news_article 
  ";

  public async Task<IEnumerable<NewsArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<NewsArticleDto>(
      BaseSelectQuery,
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
      $@"{BaseSelectQuery}
        WHERE id = @Id",
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

  public async Task<NewsArticleDto> CreateArticleAsync(NewsArticleCreateDto article,
    CancellationToken cancellationToken = default) {
    using var connection = dpContext.OpenConnection();
    using var transaction = connection.BeginTransaction();

    try
    {
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

      var articleNext = await QueryGetArticleAsync(articleId, connection, cancellationToken, transaction);
      if (articleNext == null) throw new InvalidOperationException("Не удалось загрузить созданную статью");
      
      transaction.Commit();
      
      return articleNext;
    }
    catch 
    {
      transaction.Rollback();
      throw;
    }
  }
}