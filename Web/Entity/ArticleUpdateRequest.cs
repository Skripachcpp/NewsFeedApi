using System.ComponentModel.DataAnnotations;
using Web.Application;

namespace Web.Entity;

public record ArticleUpdateRequest : ArticleCreateRequest {
  [Required(ErrorMessage = "идентификатор обязателен")]
  public int Id { get; init; }
}