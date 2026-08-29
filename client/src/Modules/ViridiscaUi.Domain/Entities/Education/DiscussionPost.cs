using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Сообщение в обсуждении
/// </summary>
public class DiscussionPost : AuditableEntity
{
    /// <summary>
    /// Содержание сообщения
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор обсуждения
    /// </summary>
    public Guid DiscussionUid { get; set; }

    /// <summary>
    /// Идентификатор автора сообщения
    /// </summary>
    public Guid AuthorUid { get; set; }

    /// <summary>
    /// Идентификатор родительского сообщения (для ответов)
    /// </summary>
    public Guid? ParentPostUid { get; set; }

    /// <summary>
    /// Отредактировано ли сообщение
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Дата последнего редактирования
    /// </summary>
    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// Активно ли сообщение
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Обсуждение, к которому относится сообщение
    /// </summary>
    public Discussion? Discussion { get; set; }

    /// <summary>
    /// Автор сообщения
    /// </summary>
    public Person? Author { get; set; }

    /// <summary>
    /// Родительское сообщение (если это ответ)
    /// </summary>
    public DiscussionPost? ParentPost { get; set; }

    /// <summary>
    /// Ответы на это сообщение
    /// </summary>
    public ICollection<DiscussionPost> Replies { get; set; } = new List<DiscussionPost>();

    /// <summary>
    /// Лайки к сообщению
    /// </summary>
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
} 