namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для элемента уведомления
/// </summary>
public class NotificationItemViewModel : ViewModelBase
{
    /// <summary>
    /// Заголовок уведомления
    /// </summary>
    [Reactive] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Содержание уведомления
    /// </summary>
    [Reactive] public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Тип уведомления
    /// </summary>
    [Reactive] public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Дата создания уведомления
    /// </summary>
    [Reactive] public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Прочитано ли уведомление
    /// </summary>
    [Reactive] public bool IsRead { get; set; }

    /// <summary>
    /// Важное ли уведомление
    /// </summary>
    [Reactive] public bool IsImportant { get; set; }

    /// <summary>
    /// Отправитель уведомления
    /// </summary>
    [Reactive] public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// Действие для выполнения при клике
    /// </summary>
    [Reactive] public string? ActionUrl { get; set; }

    /// <summary>
    /// Дата уведомления
    /// </summary>
    [Reactive] public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Уникальный идентификатор уведомления
    /// </summary>
    [Reactive] public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Приоритет уведомления
    /// </summary>
    [Reactive] public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Команда клика по уведомлению
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClickCommand { get; }

    /// <summary>
    /// Команда для отметки как прочитанное
    /// </summary>
    public ReactiveCommand<Unit, Unit> MarkAsReadCommand { get; private set; }

    /// <summary>
    /// Команда удаления уведомления
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    /// <summary>
    /// Создает новый элемент уведомления
    /// </summary>
    public NotificationItemViewModel()
    {
        ClickCommand = ReactiveCommand.Create(() => { });
        MarkAsReadCommand = ReactiveCommand.Create(() => { });
        DeleteCommand = ReactiveCommand.Create(() => { });
    }

    /// <summary>
    /// Создает элемент уведомления с базовыми данными
    /// </summary>
    public static NotificationItemViewModel Create(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string sender = "")
    {
        return new NotificationItemViewModel
        {
            Title = title,
            Message = message,
            Type = type,
            Sender = sender,
            CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Иконка для типа уведомления
    /// </summary>
    public string TypeIcon => Type switch
    {
        NotificationType.Info => "Information",
        NotificationType.Success => "CheckCircle",
        NotificationType.Warning => "AlertTriangle",
        NotificationType.Error => "AlertCircle",
        _ => "Information"
    };

    /// <summary>
    /// Цвет для типа уведомления
    /// </summary>
    public string TypeColor => Type switch
    {
        NotificationType.Info => "#007ACC",
        NotificationType.Success => "#28A745",
        NotificationType.Warning => "#FFC107",
        NotificationType.Error => "#DC3545",
        _ => "#007ACC"
    };

    /// <summary>
    /// Форматированная дата для отображения
    /// </summary>
    public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Время, прошедшее с создания
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - CreatedAt;
            if (timeSpan.TotalMinutes < 1)
                return "только что";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} мин. назад";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} ч. назад";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} дн. назад";
            return FormattedDate;
        }
    }
} 