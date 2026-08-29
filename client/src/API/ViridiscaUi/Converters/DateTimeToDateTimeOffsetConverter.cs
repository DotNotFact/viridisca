using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер между DateTime/DateTime? (используется во ViewModels) и DateTimeOffset?,
/// к которому в реальности привязывается DatePicker.SelectedDate в Avalonia.
/// </summary>
/// <remarks>
/// Every DatePicker.SelectedDate bound directly to a DateTime/DateTime? property (no
/// converter) threw System.InvalidCastException: Could not convert '...' (System.DateTime)
/// to 'System.Nullable`1[System.DateTimeOffset]' the moment the picker rendered a non-null
/// value - found via StudentDialog's "Дата рождения" field, but the same direct binding
/// pattern was used in ~15 other dialogs/views across the app.
/// </remarks>
public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public static readonly DateTimeToDateTimeOffsetConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified)),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset dto)
        {
            return Nullable.GetUnderlyingType(targetType) == null && targetType == typeof(DateTime)
                ? DateTime.Today
                : null;
        }

        return dto.DateTime;
    }
}
