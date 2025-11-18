namespace Web.Entity;

public record ArticleCreateRequest(
  string Title,
  string Content,
  string? Summary
  // string?[]? Tags = null
);