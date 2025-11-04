using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.ValidationRules;

public class PhoneValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var phone = value as string;
        if (string.IsNullOrWhiteSpace(phone))
            return new ValidationResult(false, "Введите номер телефона");

        if (!Regex.IsMatch(phone, @"^(\+7|8)\s?\(?\d{3}\)?\s?\d{3}-?\d{2}-?\d{2}$"))
            return new ValidationResult(false, "Неверный формат номера");

        return ValidationResult.ValidResult;
    }
}
