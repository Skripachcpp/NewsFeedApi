using Domain.DTOs;

namespace Domain.Interfaces;

public interface INewsRepository {
  Task<PageDto<NewsArticleDto>> GetArticlesAsync(int offset = 0, int count = 10, CancellationToken cancellationToken = default);

  Task<NewsArticleDto?> GetArticleAsync(int id, CancellationToken cancellationToken = default);

  Task<NewsArticleDto> CreateArticleAsync(NewsArticleCreateDto article, CancellationToken cancellationToken = default);
  Task<bool> DeleteArticleAsync(int id, CancellationToken cancellationToken = default);
  Task<NewsArticleDto?> UpdateArticleAsync(NewsArticleUpdateDto article, CancellationToken cancellationToken = default);
}