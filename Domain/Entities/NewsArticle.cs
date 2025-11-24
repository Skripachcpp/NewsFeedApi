namespace Domain.Entities;

public sealed class NewsArticle
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public string? Summary { get; init; }
    public DateTime PublicationDate { get; init; }
    public int? UserId { get; init; }
    public string? UserName { get; init; }
}
