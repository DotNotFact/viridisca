namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Результат загрузки файла
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// Успешность операции
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Уникальный идентификатор загруженного файла
    /// </summary>
    public Guid? FileUid { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME-тип файла
    /// </summary>
    public string? ContentType { get; set; }

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
    public static FileUploadResult Success(Guid fileUid, string fileName, string filePath, long fileSize, string? contentType = null)
    {
        return new FileUploadResult
        {
            IsSuccess = true,
            FileUid = fileUid,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            ContentType = contentType
        };
    }

    /// <summary>
    /// Создает результат с ошибкой
    /// </summary>
    public static FileUploadResult Failure(string errorMessage)
    {
        return new FileUploadResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
} 