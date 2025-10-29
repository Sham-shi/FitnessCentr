using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FitnessCentrApp.ViewModels.Base;

public abstract class ValidatableViewModel : BaseViewModel, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName != null && _errors.TryGetValue(propertyName, out var list))
            return list;
        return Enumerable.Empty<string>();
    }

    protected void ValidateProperty(object? value, string propertyName)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(this) { MemberName = propertyName };

        _errors.Remove(propertyName);

        if (!Validator.TryValidateProperty(value, context, results))
        {
            _errors[propertyName] = results.Select(r => r.ErrorMessage ?? "Ошибка").ToList();
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    public void ValidateAll()
    {
        _errors.Clear();
        var context = new ValidationContext(this);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(this, context, results, true))
        {
            foreach (var group in results.GroupBy(r => r.MemberNames.FirstOrDefault() ?? ""))
            {
                _errors[group.Key] = group.Select(r => r.ErrorMessage ?? "Ошибка").ToList();
            }
        }

        foreach (var key in _errors.Keys)
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));

        OnPropertyChanged(nameof(HasErrors));
    }
}
