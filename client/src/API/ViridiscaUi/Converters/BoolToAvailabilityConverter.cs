using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения текстового статуса доступности библиотечного ресурса
/// </summary>
public class BoolToAvailabilityConverter : IValueConverter
{
    public static readonly BoolToAvailabilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "Доступно" : "Недоступно";
        }

        return "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
}
