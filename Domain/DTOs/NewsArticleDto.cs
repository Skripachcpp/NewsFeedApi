namespace Domain.DTOs;

public class NewsArticleDto {
  public int Id { get; set; }
  public required string Title { get; set; }
  public required string Content { get; set; }
  public string? Summary { get; set; }
  public DateTime PublicationDate { get; set; }
  public string? UserName { get; set; }
}