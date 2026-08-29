namespace ViridiscaUi.Domain.Entities.Base;

/// <summary>
/// Интерфейс для сущностей с поддержкой мягкого удаления
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Флаг удаления (мягкое удаление)
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Дата удаления
    /// </summary>
    DateTime? DeletedAt { get; set; }
} 