namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Запись о файле в системе
/// </summary>
public class FileRecord
{
    /// <summary>
    /// Уникальный идентификатор файла
    /// </summary>
    public Guid Uid { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME тип файла
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Расширение файла
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания файла
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения файла
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, загрузившего файл
    /// </summary>
    public Guid? UploadedByUserUid { get; set; }

    /// <summary>
    /// Директория, в которой находится файл
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    /// Теги файла
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Дополнительные метаданные
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Активен ли файл
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Хэш файла для проверки целостности
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// Создает новую запись о файле
    /// </summary>
    public static FileRecord Create(string fileName, string filePath, long fileSize, string contentType)
    {
        return new FileRecord
        {
            Uid = Guid.NewGuid(),
            FileName = fileName,
            OriginalFileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            ContentType = contentType,
            Extension = Path.GetExtension(fileName),
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            Directory = Path.GetDirectoryName(filePath) ?? string.Empty
        };
    }
} 