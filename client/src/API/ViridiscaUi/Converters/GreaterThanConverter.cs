using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для сравнения значений
/// </summary>
public class GreaterThanConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML
    /// </summary>
    public static readonly GreaterThanConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        try
        {
            // Пробуем преобразовать значения в double для сравнения
            var valueDouble = System.Convert.ToDouble(value);
            var parameterDouble = System.Convert.ToDouble(parameter);

            return valueDouble > parameterDouble;
        }
        catch
        {
            return false;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 