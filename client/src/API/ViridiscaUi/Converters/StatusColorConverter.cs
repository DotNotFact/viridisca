using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для определения цвета индикатора статуса
/// </summary>
public class StatusColorConverter : IMultiValueConverter, IValueConverter
{
    public static readonly StatusColorConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
            return Colors.Green;

        var errorsCount = values[0] as int? ?? 0;
        var warningsCount = values[1] as int? ?? 0;

        if (errorsCount > 0)
            return Colors.Red;
        
        if (warningsCount > 0)
            return Colors.Orange;
        
        return Colors.Green;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "#27AE60" : "#E67E22"; // Green for active, Orange for inactive
        }
        
        return "#95A5A6"; // Gray for unknown
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return new object[] { AvaloniaProperty.UnsetValue };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
} 