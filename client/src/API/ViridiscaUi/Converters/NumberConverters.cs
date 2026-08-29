using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Статические конвертеры для работы с числами
/// </summary>
public static class NumberConverters
{
    public static readonly IValueConverter IsGreaterThanZero = new FuncValueConverter<int, bool>(i => i > 0);
    public static readonly IValueConverter IsZero = new FuncValueConverter<int, bool>(i => i == 0);
    public static readonly IValueConverter IsNull = new FuncValueConverter<int?, bool>(i => i == null);
    public static readonly IValueConverter IsNotNull = new FuncValueConverter<int?, bool>(i => i != null);
} 