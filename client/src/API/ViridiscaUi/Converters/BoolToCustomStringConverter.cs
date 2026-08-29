using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования boolean значений в пользовательские строки
/// </summary>
public class BoolToCustomStringConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML
    /// </summary>
    public static BoolToCustomStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string parameterString)
            return string.Empty;

        bool boolValue;
        
        // Обрабатываем различные типы входных значений
        if (value is bool directBool)
        {
            boolValue = directBool;
        }
        else if (value is int intValue)
        {
            boolValue = intValue > 0;
        }
        else if (value is double doubleValue)
        {
            boolValue = doubleValue > 0;
        }
        else if (value is string stringValue)
        {
            boolValue = !string.IsNullOrEmpty(stringValue);
        }
        else
        {
            boolValue = value != null;
        }

        // Проверяем на invert параметр
        if (parameterString.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            return !boolValue;
        }

        // Параметр должен быть в формате "TrueString|FalseString"
        var parts = parameterString.Split('|');
        if (parts.Length != 2)
            return boolValue.ToString();

        return boolValue ? parts[0] : parts[1];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        throw new NotImplementedException();
    }
} 