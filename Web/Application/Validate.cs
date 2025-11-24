using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Web.Application;

internal sealed class Validate(): ValidationAttribute
{
    public int Max { get; set; } = int.MinValue;

    public int MaxBytes { get; set; } = int.MinValue;

    public int Min { get; set; } = int.MinValue;

    public bool Required { get; set; }

    private string? MessageString => this.ErrorMessage ?? $"{(this.Required ? "обязательно, " : string.Empty)}должно содержать {(this.Min != int.MinValue ? $"от {this.Min} " : string.Empty)}{(this.Max != int.MinValue ? $"до {this.Max} " : string.Empty)}символ(ов)";

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (this.Required && value == null)
            return new ValidationResult("обязательно");

        if (value == null)
            return ValidationResult.Success;

        if (value is string text)
        {
            if (this.Min != int.MinValue && text.Length < this.Min)
                return new ValidationResult(this.MessageString);

            if (this.Max != int.MinValue && text.Length > this.Max)
                return new ValidationResult(this.MessageString);

            if (this.MaxBytes != int.MinValue)
            {
                int byteLength = Encoding.UTF8.GetByteCount(text);

                if (this.MaxBytes != int.MinValue && byteLength > this.MaxBytes)
                    return new ValidationResult("слишком длинный текст");
            }

            return ValidationResult.Success;
        }

        if (value is string[] textArray)
        {
            if (this.Min != int.MinValue && textArray.Any(it => it.Length < this.Min))
                return new ValidationResult(this.MessageString);

            if (this.Max != int.MinValue && textArray.Any(it => it.Length > this.Max))
                return new ValidationResult(this.MessageString);

            // в массиве должно что то быть иначе какой он обязательный
            if (this.Required && textArray.Length <= 0)
                return new ValidationResult(this.MessageString);

            return ValidationResult.Success;
        }

        if (value is int numeric)
        {
            if (this.Min != int.MinValue && numeric < this.Min)
                return new ValidationResult(this.ErrorMessage ?? $"не должно быть меньше {this.Min}");

            if (this.Max != int.MinValue && numeric > this.Max)
                return new ValidationResult(this.ErrorMessage ?? $"не должно быть больше {this.Max}");

            return ValidationResult.Success;
        }

        return new ValidationResult("Тип значения не поддерживается атрибутом валидации");
    }
}
