using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Material.Icons;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения иконки статуса активации учебного плана
/// </summary>
public class BoolToActivationIconConverter : IValueConverter
{
    public static readonly BoolToActivationIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? MaterialIconKind.CheckCircle : MaterialIconKind.PauseCircle;
        }

        return MaterialIconKind.Help;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
}
