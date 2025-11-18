using Domain.DTOs;

namespace Domain.Interfaces;

public interface INewsRepository {
  Task<IEnumerable<NewsArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default);
  Task<NewsArticleDto?> GetArticleAsync(int id, CancellationToken cancellationToken = default);

  Task<NewsArticleDto> CreateArticleAsync(NewsArticleCreateDto article, CancellationToken cancellationToken = default);
  Task DeleteArticleAsync(int id, CancellationToken cancellationToken = default);
  Task<NewsArticleDto> UpdateArticleAsync(NewsArticleUpdateDto article, CancellationToken cancellationToken = default);
}