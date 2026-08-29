using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Обсуждение/форум в рамках курса
/// </summary>
public class Discussion : AuditableEntity
{
    /// <summary>
    /// Заголовок обсуждения
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание обсуждения
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Идентификатор создателя обсуждения
    /// </summary>
    public Guid CreatedByUid { get; set; }

    /// <summary>
    /// Закреплено ли обсуждение
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Заблокировано ли обсуждение для новых сообщений
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Активно ли обсуждение
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// Создатель обсуждения
    /// </summary>
    public Person? CreatedBy { get; set; }

    /// <summary>
    /// Сообщения в обсуждении
    /// </summary>
    public ICollection<DiscussionPost> Posts { get; set; } = new List<DiscussionPost>();
} 