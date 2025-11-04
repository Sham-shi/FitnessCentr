using System.Threading.Tasks;
using SW = System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace FitnessCentrApp.ViewModels.Services;

public static class MessageBoxService
{
    /// <summary>
    /// Отображает простое информационное сообщение.
    /// </summary>
    public static async Task ShowInfoAsync(string title, string message)
    {
        var dialog = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "ОК",
            CloseButtonIcon = new SymbolIcon { Symbol = SymbolRegular.Checkmark24 }
        };

        await dialog.ShowDialogAsync();
    }

    /// <summary>
    /// Отображает предупреждение.
    /// </summary>
    public static async Task ShowWarningAsync(string title, string message)
    {
        var dialog = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "Понял",
            CloseButtonIcon = new SymbolIcon { Symbol = SymbolRegular.Warning24 }
        };

        await dialog.ShowDialogAsync();
    }

    /// <summary>
    /// Отображает сообщение об ошибке.
    /// </summary>
    public static async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "Закрыть",
            CloseButtonIcon = new SymbolIcon { Symbol = SymbolRegular.ErrorCircle24 }
            // Добавление стиля для визуального акцента, если нужно
            // PrimaryButtonAppearance = ControlAppearance.Danger, 
            // Если бы у ошибки была Primary кнопка, это бы ее подсветило красным
        };

        await dialog.ShowDialogAsync();
    }

    /// <summary>
    /// Диалог подтверждения (Да/Нет).
    /// Возвращает true, если пользователь нажал Да.
    /// </summary>
    public static async Task<bool> ShowConfirmAsync(string title, string message)
    {
        var dialog = new MessageBox
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Да",
            CloseButtonText = "Нет",
            PrimaryButtonIcon = new SymbolIcon { Symbol = SymbolRegular.Checkmark24 },
            CloseButtonIcon = new SymbolIcon { Symbol = SymbolRegular.Dismiss24 },
            // Явно делаем кнопку "Да" акцентной
            PrimaryButtonAppearance = ControlAppearance.Primary
        };

        var result = await dialog.ShowDialogAsync();

        return result == MessageBoxResult.Primary;
    }

    /// <summary>
    /// Универсальный вариант — позволяет задать произвольные кнопки и иконку.
    /// </summary>
    public static async Task<MessageBoxResult> ShowCustomAsync(
        string title,
        string message,
        string? primaryButton = null,
        string? secondaryButton = null,
        string? closeButton = "ОК", // Устанавливаем "ОК" по умолчанию для Close
        SymbolRegular? primarySymbol = null,
        SymbolRegular? closeSymbol = null,
        ControlAppearance primaryAppearance = ControlAppearance.Primary)
    {
        var dialog = new MessageBox
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButton,
            SecondaryButtonText = secondaryButton,
            CloseButtonText = closeButton,
            PrimaryButtonAppearance = primaryAppearance
        };

        if (primarySymbol.HasValue)
            dialog.PrimaryButtonIcon = new SymbolIcon { Symbol = primarySymbol.Value };

        if (closeSymbol.HasValue)
            dialog.CloseButtonIcon = new SymbolIcon { Symbol = closeSymbol.Value };

        return await dialog.ShowDialogAsync();
    }
}
