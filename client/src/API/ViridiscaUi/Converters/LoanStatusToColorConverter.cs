using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для определения цвета индикатора статуса займа библиотечного ресурса.
/// </summary>
/// <remarks>
/// LibraryLoanViewModel (see ViewModels/System/LibraryViewModel.cs) does not currently expose a
/// "Status" property (only LoanedAt/DueDate/ReturnedAt/IsReturned/IsOverdue) and there is no
/// domain LoanStatus enum, so the "Status" binding this converter is attached to
/// (LibraryView.axaml) does not resolve to a real value at runtime. This converter is written
/// defensively against the status names most libraries use (matched by string, case-insensitive,
/// in Russian and English) so it never throws regardless of what ends up bound here, and falls
/// back to a neutral gray. Palette follows the same family used by
/// ScheduleSlotViewModel.TypeColor/CourseStatusColor (blue/orange/purple/green/red/gray).
/// </remarks>
public class LoanStatusToColorConverter : IValueConverter
{
    public static readonly LoanStatusToColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString();

        if (string.IsNullOrWhiteSpace(status))
            return Colors.Gray;

        return status.Trim().ToLowerInvariant() switch
        {
            "active" or "активен" or "активно" or "выдан" or "выдано" => Color.Parse("#2196F3"),   // Blue
            "overdue" or "просрочен" or "просрочено" or "просроченный" => Color.Parse("#F44336"),   // Red
            "returned" or "возвращен" or "возвращён" or "возвращено" or "closed" => Color.Parse("#4CAF50"), // Green
            "extended" or "продлен" or "продлён" or "продлено" => Color.Parse("#FF9800"),           // Orange
            "lost" or "утерян" or "утеряно" => Color.Parse("#9C27B0"),                               // Purple
            _ => Color.Parse("#9E9E9E")                                                              // Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
