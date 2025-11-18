using System.ComponentModel.DataAnnotations;
using Web.Application;

namespace Web.Entity;

public record ArticleCreateRequest
{
  [Required(ErrorMessage = "Заголовок обязателен")]
  [StringLength(500, MinimumLength = 1, ErrorMessage = "Заголовок должен быть от 1 до 500 символов")]
  public required string Title { get; init; }
  
  [Required(ErrorMessage = "Содержание обязательно")]
  public required  string Content { get; init; }
  
  [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
  public string? Summary { get; init; }
  
  [StringsLength(100, ErrorMessage = "Длина тега не должна превышать 100 символов")]
  public string[]? Tags { get; init; }
}