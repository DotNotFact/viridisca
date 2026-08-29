namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Результат валидации файла
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// Файл прошел валидацию
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Список ошибок валидации
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Информация о файле
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME-тип файла
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Расширение файла
    /// </summary>
    public string? FileExtension { get; set; }

    /// <summary>
    /// Дополнительные данные валидации
    /// </summary>
    public Dictionary<string, object>? ValidationData { get; set; }

    /// <summary>
    /// Создает успешный результат валидации
    /// </summary>
    public static FileValidationResult Valid(string fileName, long fileSize, string? contentType = null)
    {
        return new FileValidationResult
        {
            IsValid = true,
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            FileExtension = Path.GetExtension(fileName)
        };
    }

    /// <summary>
    /// Создает результат с ошибками валидации
    /// </summary>
    public static FileValidationResult Invalid(string fileName, params string[] errors)
    {
        return new FileValidationResult
        {
            IsValid = false,
            FileName = fileName,
            Errors = [.. errors]
        };
    }

    /// <summary>
    /// Добавляет ошибку валидации
    /// </summary>
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// Добавляет предупреждение
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    /// <summary>
    /// Проверяет, есть ли ошибки или предупреждения
    /// </summary>
    public bool HasIssues => Errors.Count > 0 || Warnings.Count > 0;
} 