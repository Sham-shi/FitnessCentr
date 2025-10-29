namespace FitnessCentrApp.ViewModels.Base.Interfaces;

public interface IEditableViewModel
{
    event Action<object> BeginEditRequested;
    object? EditableItem { get; }
}
