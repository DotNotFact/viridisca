using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения текстового статуса активности учебного плана
/// </summary>
public class BoolToActiveStatusConverter : IValueConverter
{
    public static readonly BoolToActiveStatusConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Активен" : "Неактивен";
        }

        return "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
}
