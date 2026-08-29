using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Статические экземпляры конвертеров для строк
/// </summary>
public static class StringConverters
{
    /// <summary>
    /// Конвертер для преобразования boolean в пользовательские строки
    /// </summary>
    public static readonly BoolToCustomStringConverter BoolToCustomString = new();

    /// <summary>
    /// Конвертер, возвращающий true, если строка не null и не пуста
    /// </summary>
    public static readonly IValueConverter IsNotNullOrEmpty = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s));
} 