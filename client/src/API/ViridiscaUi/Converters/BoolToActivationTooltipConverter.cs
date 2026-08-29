using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения подсказки к кнопке переключения активации учебного плана
/// </summary>
public class BoolToActivationTooltipConverter : IValueConverter
{
    public static readonly BoolToActivationTooltipConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Деактивировать" : "Активировать";
        }

        return "Переключить статус";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
}
