using System.ComponentModel.DataAnnotations;

namespace Web.Application;

public class StringsLengthAttribute(int maxLength) : ValidationAttribute {
  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
    if (value == null) return ValidationResult.Success;

    if (value is string[] stringArray) {
      if (stringArray.Any(it => it.Length > maxLength)) {
        var errorMessage = ErrorMessage ?? $"Строка не должна превышать {maxLength} символов";
        return new ValidationResult(errorMessage);
      }

      return ValidationResult.Success;
    }

    return new ValidationResult("Тип значения не поддерживается атрибутом валидации");
  }
}