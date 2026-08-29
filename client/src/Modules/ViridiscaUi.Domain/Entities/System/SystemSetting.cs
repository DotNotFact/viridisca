using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Системная настройка
/// </summary>
public class SystemSetting : AuditableEntity
{
    /// <summary>
    /// Ключ настройки
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Значение настройки
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Описание настройки
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Тип данных настройки
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Категория настройки
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Является ли настройка системной (нельзя удалить)
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public SystemSetting()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public SystemSetting(string key, string value, string? description = null) : this()
    {
        Key = key;
        Value = value;
        Description = description;
    }
} 