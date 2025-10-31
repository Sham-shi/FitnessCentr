using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.ValidationRules;

public class EmailValidationRule : ValidationRule
{
    // Регулярка для проверки email-адреса
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var email = (value ?? string.Empty).ToString();

        if (string.IsNullOrWhiteSpace(email))
            return new ValidationResult(false, "Email не может быть пустым");

        if (!EmailRegex.IsMatch(email))
            return new ValidationResult(false, "Некорректный формат Email");

        return ValidationResult.ValidResult;
    }
}
