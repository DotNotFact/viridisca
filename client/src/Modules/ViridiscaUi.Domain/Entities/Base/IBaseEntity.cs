namespace ViridiscaUi.Domain.Entities.Base;

/// <summary>
/// Базовый интерфейс для всех сущностей
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    Guid Uid { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    DateTime LastModifiedAt { get; set; }
} 