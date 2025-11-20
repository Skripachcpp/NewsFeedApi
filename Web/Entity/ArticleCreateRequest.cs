using Web.Application;

namespace Web.Entity;

public record ArticleCreateRequest
{
  [Validate(Required = true, Min = 1, Max = 500)]
  public required string Title { get; init; }
  
  [Validate(Required = true)]
  public required  string Content { get; init; }
  
  [Validate(Max = 1000)]
  public string? Summary { get; init; }
  
  [Validate(Max = 100)]
  public string[]? Tags { get; init; }
}