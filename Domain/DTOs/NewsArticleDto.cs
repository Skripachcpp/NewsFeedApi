using System.Collections.ObjectModel;

namespace Domain.DTOs;

public sealed record NewsArticleDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public string? Summary { get; init; }
    public DateTime PublicationDate { get; init; }
    public string? UserName { get; init; }
    public ICollection<string> Tags { get; init; } = [];
}