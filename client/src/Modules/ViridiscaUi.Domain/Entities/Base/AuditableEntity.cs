using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Base;

/// <summary>
/// Базовая сущность с аудитом и мягким удалением
/// </summary>
public abstract class AuditableEntity : IBaseEntity, ISoftDeletable
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Флаг удаления (мягкое удаление)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Дата удаления
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
