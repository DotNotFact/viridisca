using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для отображения статуса департамента на основе свойства IsActive
/// </summary>
public class StatusDisplayConverter : IValueConverter
{
    public static readonly StatusDisplayConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Активный" : "Неактивный";
        }
        
        return "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status == "Активный";
        }
        
        return false;
    }
} 