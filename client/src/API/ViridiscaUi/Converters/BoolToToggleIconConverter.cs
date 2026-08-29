using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования логического значения IsActive в иконку переключения
/// </summary>
public class BoolToToggleIconConverter : IValueConverter
{
    public static readonly BoolToToggleIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? MaterialIconKind.Pause : MaterialIconKind.Play;
        }

        return MaterialIconKind.Help;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 