using Domain.Entities;

namespace Domain.Interfaces;

public interface INewsRepository {
  Task<IEnumerable<NewsArticle>> GetArticlesAsync(CancellationToken cancellationToken = default);
}