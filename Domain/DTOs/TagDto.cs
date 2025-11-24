namespace Domain.DTOs;

public sealed record TagDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
