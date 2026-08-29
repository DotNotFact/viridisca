using System;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Статические конвертеры для работы с объектами
/// </summary>
public static class ObjectConverters
{
    public static readonly IValueConverter IsNull = new FuncValueConverter<object?, bool>(obj => obj == null);
    public static readonly IValueConverter IsNotNull = new FuncValueConverter<object?, bool>(obj => obj != null);
} 