using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования оценки в цвет
/// </summary>
public class GradeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not decimal grade)
            return new SolidColorBrush(Colors.Gray);

        return grade switch
        {
            >= 4.5m => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Зеленый для отлично (5)
            >= 3.5m => new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Синий для хорошо (4)
            >= 2.5m => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Оранжевый для удовлетворительно (3)
            >= 1.0m => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Красный для неудовлетворительно (2)
            _ => new SolidColorBrush(Colors.Gray)                         // Серый для не оценено
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
} 