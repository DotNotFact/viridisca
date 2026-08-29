using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер, возвращающий все возможные значения enum-а по текущему значению.
/// Используется для заполнения ItemsSource выпадающих списков (ComboBox) значениями enum-а.
/// </summary>
public class EnumToCollectionConverter : IValueConverter
{
    public static readonly EnumToCollectionConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        return Enum.GetValues(value.GetType());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
}
