namespace Domain.Entities;

public sealed class NewsArticle {
  // id
  public int Id { get; set; }
  
  // заголовок
  public required string Title { get; set; }
  
  // контент
  public required string Content { get; set; }
  
  // описание
  public string? Summary { get; set; }
  
  // дата публикации
  public DateTime PublicationDate { get; set; }

  // теги
  public List<int> TagIds { get; set; } = new();

  // создатель
  public int? UserId { get; set; }
  
  // имя создателя
  public string? UserName { get; set; }
}