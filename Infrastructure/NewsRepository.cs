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
    user_id as UserId,
    user_name as UserName
  FROM news_article 
  ";

  public async Task<IEnumerable<NewsArticle>> GetArticlesAsync(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<NewsArticle>(
      BaseSelectQuery,
      cancellationToken: cancellationToken);

    return result;
  }

  public async Task<NewsArticle?> GetArticleAsync(int id, CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryFirstOrDefaultAsync<NewsArticle>(
      $@"{BaseSelectQuery}
        WHERE id = @Id",
      parameters: new { Id = id },
      cancellationToken: cancellationToken);

    return result;
  }
}