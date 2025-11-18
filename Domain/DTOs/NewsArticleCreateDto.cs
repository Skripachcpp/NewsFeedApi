namespace Domain.DTOs;

public class NewsArticleCreateDto {
  public required string Title { get; set; }
  public required string Content { get; set; }
  public string? Summary { get; set; }
  public DateTime PublicationDate { get; set; }
  public int? UserId { get; set; }
  public string? UserName { get; set; }
}