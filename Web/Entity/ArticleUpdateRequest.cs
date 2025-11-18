namespace Web.Entity;

public record ArticleUpdateRequest(
  int Id,
  string Title,
  string Content,
  string? Summary,
  string[]? Tags
);