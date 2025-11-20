using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Web.Application;

public class Validate() : ValidationAttribute {
  public int Max { get; set; } = int.MinValue;
  public int MaxBytes { get; set; } = int.MinValue;
  public int Min { get; set; } = int.MinValue;
  public bool Required { get; set; } = false;

  private string? MessageString => ErrorMessage ?? $"{(Required ? "обязательно, " : "")}должно содержать {(Min != int.MinValue ? $"от {Min} " : string.Empty)}{(Max != int.MinValue ? $"до {Max} " : string.Empty)}символ(ов)";
  
  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
    if (Required && value == null) return new ValidationResult("обязательно");
    
    if (value == null) return ValidationResult.Success;
    
    if (value is string text) {
      if (Min != int.MinValue && text.Length < Min) return new ValidationResult(MessageString);
      if (Max != int.MinValue && text.Length > Max) return new ValidationResult(MessageString);

      if (MaxBytes != int.MinValue) {
        int byteLength = Encoding.UTF8.GetByteCount(text);
            
        if (MaxBytes != -1 && byteLength > MaxBytes) return new ValidationResult("слишком длинный текст");
      }

      return ValidationResult.Success;
    }

    if (value is string[] textArray) {
      if (Min != int.MinValue && textArray.Any(it => it.Length < Min)) return new ValidationResult(MessageString);
      if (Max != int.MinValue && textArray.Any(it => it.Length > Max)) return new ValidationResult(MessageString);

      // в массиве должно что то быть иначе какой он обязательный
      if (Required && !textArray.Any()) return new ValidationResult(MessageString);

      return ValidationResult.Success;
    }

    if (value is int numeric) {
      if (Min != int.MinValue && numeric < Min) return new ValidationResult(ErrorMessage ?? $"не должно быть меньше {Min}");
      if (Max != int.MinValue && numeric > Max) return new ValidationResult(ErrorMessage ?? $"не должно быть больше {Max}");
      
      return ValidationResult.Success;
    }

    return new ValidationResult("Тип значения не поддерживается атрибутом валидации");
  }
}