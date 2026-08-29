using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Фильтр для поиска уведомлений
/// </summary>
public class NotificationFilter
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid? PersonUid { get; set; }

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// Приоритет уведомления
    /// </summary>
    public NotificationPriority? Priority { get; set; }

    /// <summary>
    /// Категория уведомления
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Включать прочитанные уведомления
    /// </summary>
    public bool IncludeRead { get; set; } = true;

    /// <summary>
    /// Только непрочитанные уведомления
    /// </summary>
    public bool OnlyUnread { get; set; } = false;

    /// <summary>
    /// Дата начала поиска
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Дата окончания поиска
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Поисковый запрос по тексту
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Максимальное количество результатов
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Смещение для пагинации
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Сортировка по дате (по убыванию по умолчанию)
    /// </summary>
    public bool SortDescending { get; set; } = true;

    /// <summary>
    /// Создает фильтр для получения непрочитанных уведомлений пользователя
    /// </summary>
    public static NotificationFilter ForUnreadByUser(Guid personUid)
    {
        return new NotificationFilter
        {
            PersonUid = personUid,
            OnlyUnread = true,
            IncludeRead = false
        };
    }

    /// <summary>
    /// Создает фильтр для получения всех уведомлений пользователя
    /// </summary>
    public static NotificationFilter ForUser(Guid personUid, int limit = 50)
    {
        return new NotificationFilter
        {
            PersonUid = personUid,
            Limit = limit,
            IncludeRead = true
        };
    }

    /// <summary>
    /// Создает фильтр для поиска по тексту
    /// </summary>
    public static NotificationFilter ForSearch(string searchText, Guid? personUid = null)
    {
        return new NotificationFilter
        {
            PersonUid = personUid,
            SearchText = searchText,
            IncludeRead = true
        };
    }
} 