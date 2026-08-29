using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Запись о файле в системе
/// </summary>
public class FileRecord : AuditableEntity
{
    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Имя файла в хранилище
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// MIME-тип файла
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Идентификатор пользователя, загрузившего файл
    /// </summary>
    public Guid UploadedByUid { get; set; }

    /// <summary>
    /// Тип сущности, к которой привязан файл
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Идентификатор сущности, к которой привязан файл
    /// </summary>
    public Guid? EntityUid { get; set; }

    /// <summary>
    /// Описание файла
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Флаг публичного доступа к файлу
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Дата истечения срока действия файла
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Пользователь, загрузивший файл
    /// </summary>
    public Person? UploadedBy { get; set; }

    /// <summary>
    /// Создает новый экземпляр записи о файле
    /// </summary>
    public FileRecord()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр записи о файле с указанными параметрами
    /// </summary>
    public FileRecord(string originalFileName, string storedFileName, string filePath, 
                     string contentType, long fileSize, Guid uploadedByUid)
    {
        Uid = Guid.NewGuid();
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedByUid = uploadedByUid;
        CreatedAt = DateTime.UtcNow;
    }
} 