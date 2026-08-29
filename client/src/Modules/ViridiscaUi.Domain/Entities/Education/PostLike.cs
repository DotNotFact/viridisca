using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Лайк к сообщению в обсуждении
/// </summary>
public class PostLike : AuditableEntity
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public Guid PostUid { get; set; }

    /// <summary>
    /// Идентификатор пользователя, поставившего лайк
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Сообщение
    /// </summary>
    public DiscussionPost? Post { get; set; }

    /// <summary>
    /// Пользователь, поставивший лайк
    /// </summary>
    public Person? Person { get; set; }
} 