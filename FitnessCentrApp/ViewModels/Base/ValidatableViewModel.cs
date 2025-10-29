using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FitnessCentrApp.ViewModels.Base;

public abstract class ValidatableViewModel : BaseViewModel, IDataErrorInfo
{
    protected readonly Dictionary<string, List<string>> _errors = new();

    public string this[string columnName]
    {
        get
        {
            if (_errors.ContainsKey(columnName))
                return string.Join(Environment.NewLine, _errors[columnName]);
            return string.Empty;
        }
    }

    public string Error => string.Join(Environment.NewLine,
        _errors.Values.SelectMany(x => x));

    public bool HasErrors => _errors.Any();

    protected void ValidateProperty(object value, [CallerMemberName] string propertyName = "")
    {
        ClearPropertyErrors(propertyName);

        // Валидация с использованием DataAnnotations
        var propertyInfo = GetType().GetProperty(propertyName);
        if (propertyInfo != null)
        {
            var attributes = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true)
                .Cast<ValidationAttribute>();

            foreach (var attribute in attributes)
            {
                if (!attribute.IsValid(value))
                {
                    AddError(propertyName, attribute.ErrorMessage ?? $"{propertyName} is not valid");
                }
            }
        }

        // Дополнительная кастомная валидация
        CustomValidation(value, propertyName);

        OnErrorsChanged();
    }

    protected virtual void CustomValidation(object value, string propertyName)
    {
        // Переопределите в дочерних классах для дополнительной валидации
    }

    public void ClearErrors()
    {
        _errors.Clear();
        OnErrorsChanged();
    }

    public void ClearPropertyErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors[propertyName].Clear();
        }
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();

        if (!_errors[propertyName].Contains(error))
            _errors[propertyName].Add(error);

        OnErrorsChanged();
    }

    protected void RemoveError(string propertyName, string error)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors[propertyName].Remove(error);
            if (!_errors[propertyName].Any())
                _errors.Remove(propertyName);

            OnErrorsChanged();
        }
    }

    private void OnErrorsChanged()
    {
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(Error));
    }

    // Метод для валидации всей модели
    public bool Validate()
    {
        ClearErrors();

        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            ValidateProperty(value, property.Name);
        }

        return !HasErrors;
    }
}
