namespace Domain.Entities;

public sealed class Tag {
  public int Id { get; init; }
  public required string Name { get; init; }
}