using System.ComponentModel.DataAnnotations;

namespace Web.Application;

public class Validate() : ValidationAttribute {
  public int Max { get; set; } = -1;
  public int Min { get; set; } = -1;
  public bool Required { get; set; } = false;

  private string? Message => ErrorMessage ?? $"должно содержать {(Min != -1 ? $"от {Min} " : string.Empty)}{(Max != -1 ? $"до {Max} " : string.Empty)}символ(ов)";
  
  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
    if (Required && value == null) return new ValidationResult("обязательно");
    
    if (value == null) return ValidationResult.Success;
    
    if (value is string text) {
      if (Min != -1 && text.Length < Min) return new ValidationResult(Message);
      if (Max != -1 && text.Length > Max) return new ValidationResult(Message);

      return ValidationResult.Success;
    }

    if (value is string[] stringArray) {
      if (Min != -1 && stringArray.Any(it => it.Length < Min)) return new ValidationResult(Message);
      if (Max != -1 && stringArray.Any(it => it.Length > Max)) return new ValidationResult(Message);

      return ValidationResult.Success;
    }

    return new ValidationResult("Тип значения не поддерживается атрибутом валидации");
  }
}