using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Системная информация
/// </summary>
public class SystemInfo : AuditableEntity
{
    /// <summary>
    /// Название приложения
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Версия приложения
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Окружение (Development, Production, etc.)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Имя машины
    /// </summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Операционная система
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// Количество процессоров
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// Рабочий набор памяти
    /// </summary>
    public long WorkingSet { get; set; }

    /// <summary>
    /// Время запуска
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время работы
    /// </summary>
    public TimeSpan UpTime { get; set; }

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public SystemInfo()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 