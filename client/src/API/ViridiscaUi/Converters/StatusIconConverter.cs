using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Material.Icons;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения иконки статуса департамента для кнопки переключения
/// </summary>
public class StatusIconConverter : IValueConverter
{
    public static readonly StatusIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            // Если активный - показываем иконку для деактивации (пауза)
            // Если неактивный - показываем иконку для активации (play)
            return isActive ? MaterialIconKind.Pause : MaterialIconKind.Play;
        }
        
        return MaterialIconKind.Help;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
} 