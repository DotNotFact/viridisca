namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Результат скачивания файла
/// </summary>
public class FileDownloadResult
{
    /// <summary>
    /// Успешность операции
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Содержимое файла
    /// </summary>
    public byte[]? FileContent { get; set; }

    /// <summary>
    /// MIME-тип файла
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Путь к файлу на диске
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если операция неуспешна)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Дополнительные данные
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Создает успешный результат
    /// </summary>
    public static FileDownloadResult Success(string fileName, byte[] fileContent, string? contentType = null)
    {
        return new FileDownloadResult
        {
            IsSuccess = true,
            FileName = fileName,
            FileContent = fileContent,
            ContentType = contentType,
            FileSize = fileContent.Length
        };
    }

    /// <summary>
    /// Создает успешный результат с путем к файлу
    /// </summary>
    public static FileDownloadResult SuccessWithPath(string fileName, string filePath, string? contentType = null)
    {
        return new FileDownloadResult
        {
            IsSuccess = true,
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType
        };
    }

    /// <summary>
    /// Создает результат с ошибкой
    /// </summary>
    public static FileDownloadResult Failure(string errorMessage)
    {
        return new FileDownloadResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
} 