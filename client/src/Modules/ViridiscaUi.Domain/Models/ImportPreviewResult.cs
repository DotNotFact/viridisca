namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Результат предварительного просмотра импорта
/// </summary>
public class ImportPreviewResult
{
    /// <summary>
    /// Валиден ли файл для импорта
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Формат файла
    /// </summary>
    public string? FileFormat { get; set; }

    /// <summary>
    /// Общее количество строк в файле
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Заголовки столбцов
    /// </summary>
    public List<string> Headers { get; set; } = [];

    /// <summary>
    /// Образцы данных для предпросмотра
    /// </summary>
    public List<Dictionary<string, object?>> SampleData { get; set; } = [];

    /// <summary>
    /// Список ошибок валидации
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Рекомендуемые настройки импорта
    /// </summary>
    public Dictionary<string, object> RecommendedSettings { get; set; } = [];

    /// <summary>
    /// Создает успешный результат предпросмотра
    /// </summary>
    public static ImportPreviewResult Success(string fileFormat, int totalRows, List<string> headers, List<Dictionary<string, object?>> sampleData)
    {
        return new ImportPreviewResult
        {
            IsValid = true,
            FileFormat = fileFormat,
            TotalRows = totalRows,
            Headers = headers,
            SampleData = sampleData
        };
    }

    /// <summary>
    /// Создает результат с ошибками
    /// </summary>
    public static ImportPreviewResult Failure(params string[] errors)
    {
        return new ImportPreviewResult
        {
            IsValid = false,
            Errors = [.. errors]
        };
    }
} 