using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования boolean значения в double
/// Используется для управления прозрачностью, размерами и другими числовыми свойствами
/// </summary>
public class BoolToDoubleConverter : IValueConverter
{
    public static readonly BoolToDoubleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return targetType == typeof(int) ? 1 : 1.0;

        if (parameter is not string parameterString)
            return boolValue ? (targetType == typeof(int) ? 1 : 1.0) : (targetType == typeof(int) ? 0 : 0.0);

        var values = parameterString.Split('|');
        if (values.Length != 2)
            return boolValue ? (targetType == typeof(int) ? 1 : 1.0) : (targetType == typeof(int) ? 0 : 0.0);

        // Если true - возвращаем первое значение, если false - второе
        var targetValue = boolValue ? values[0] : values[1];
        
        // Пробуем преобразовать в int для Grid свойств
        if (targetType == typeof(int) && int.TryParse(targetValue, out var intValue))
            return intValue;
            
        // Пробуем преобразовать в double для других свойств
        if (double.TryParse(targetValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            return targetType == typeof(int) ? (int)doubleValue : doubleValue;

        return boolValue ? (targetType == typeof(int) ? 1 : 1.0) : (targetType == typeof(int) ? 0 : 0.0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
            return doubleValue > 0.5;
        
        return false;
    }
} 