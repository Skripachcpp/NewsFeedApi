using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class NewsRepository(DpContext dpContext) : INewsRepository {
  public async Task<IEnumerable<NewsArticle>> GetArticlesAsync(CancellationToken cancellationToken = default) {
    // language=PostgreSQL
    var result = await dpContext.QueryAsync<NewsArticle>(
    @"
       SELECT
        id as Id,
        title as Title,
        content as Content,
        summary as Summary,
        publication_date as PublicationDate,
        user_id as UserId,
        user_name as UserName
      FROM news_article 
      ",
      cancellationToken: cancellationToken);

    return result;
  }
}