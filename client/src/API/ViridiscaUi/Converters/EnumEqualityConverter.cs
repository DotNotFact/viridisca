using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для сравнения enum значений и возврата соответствующего значения
/// Поддерживает конвертацию в Brush для цветов
/// </summary>
public class EnumEqualityConverter : IMultiValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML
    /// </summary>
    public static EnumEqualityConverter Instance { get; } = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
            return null;

        var actualValue = values[0];
        var expectedValue = values[1];

        // Если есть третий параметр, используем его как возвращаемое значение
        var returnValue = values.Count > 2 ? values[2] : true;
        var defaultValue = values.Count > 3 ? values[3] : false;

        // Сравниваем значения
        bool isEqual = Equals(actualValue, expectedValue);
        var result = isEqual ? returnValue : defaultValue;

        // Конвертируем в нужный тип
        return ConvertToTarget(result, targetType, culture);
    }

    /// <summary>
    /// Конвертирует значение в целевой тип
    /// </summary>
    private object? ConvertToTarget(object? value, Type targetType, CultureInfo culture)
    {
        if (value == null)
            return null;

        try
        {
            // Если целевой тип - Brush и значение - строка (цвет)
            if (targetType == typeof(Brush) || targetType == typeof(IBrush))
            {
                if (value is string colorString)
                {
                    // Пытаемся создать SolidColorBrush из строки
                    if (colorString.StartsWith("#"))
                    {
                        if (Color.TryParse(colorString, out var color))
                        {
                            return new SolidColorBrush(color);
                        }
                    }
                    // Если это именованный цвет
                    else if (Enum.TryParse<KnownColor>(colorString, true, out var knownColor))
                    {
                        var color = Color.FromArgb(255, knownColor.ToColor().R, knownColor.ToColor().G, knownColor.ToColor().B);
                        return new SolidColorBrush(color);
                    }
                }
                else if (value is Color color)
                {
                    return new SolidColorBrush(color);
                }
            }

            // Если целевой тип совпадает с типом значения
            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            // Пытаемся выполнить стандартное преобразование
            return System.Convert.ChangeType(value, targetType, culture);
        }
        catch
        {
            // Если преобразование не удалось, возвращаем исходное значение
            return value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("EnumEqualityConverter поддерживает только одностороннее преобразование");
    }
}

/// <summary>
/// Простой перечень известных цветов для базовой поддержки
/// </summary>
public enum KnownColor
{
    Red, Green, Blue, Yellow, Orange, Purple, Pink, Brown, Black, White, Gray, Grey
}

/// <summary>
/// Расширения для KnownColor
/// </summary>
public static class KnownColorExtensions
{
    public static (byte R, byte G, byte B) ToColor(this KnownColor color)
    {
        return color switch
        {
            KnownColor.Red => (255, 0, 0),
            KnownColor.Green => (0, 255, 0),
            KnownColor.Blue => (0, 0, 255),
            KnownColor.Yellow => (255, 255, 0),
            KnownColor.Orange => (255, 165, 0),
            KnownColor.Purple => (128, 0, 128),
            KnownColor.Pink => (255, 192, 203),
            KnownColor.Brown => (165, 42, 42),
            KnownColor.Black => (0, 0, 0),
            KnownColor.White => (255, 255, 255),
            KnownColor.Gray or KnownColor.Grey => (128, 128, 128),
            _ => (128, 128, 128)
        };
    }
} 