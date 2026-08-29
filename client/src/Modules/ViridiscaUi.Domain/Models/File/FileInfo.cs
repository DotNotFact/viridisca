namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Информация о файле
/// </summary>
public class FileInfo
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
    /// Тип содержимого
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Идентификатор связанной сущности
    /// </summary>
    public Guid? EntityUid { get; set; }

    /// <summary>
    /// Тип связанной сущности
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
} 