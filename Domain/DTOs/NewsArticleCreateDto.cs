namespace Domain.DTOs;

public class NewsArticleCreateDto {
  public required string Title { get; init; }
  public required string Content { get; init; }
  public string? Summary { get; init; }
  public int? UserId { get; init; }
  public string? UserName { get; init; }
  public string[]? Tags { get; init; }
}