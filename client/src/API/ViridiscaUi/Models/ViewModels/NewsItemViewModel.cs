namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для новостного элемента
/// </summary>
public class NewsItemViewModel : ViewModelBase
{
    /// <summary>
    /// Заголовок новости
    /// </summary>
    [Reactive] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Краткое содержание новости
    /// </summary>
    [Reactive] public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Полное содержание новости
    /// </summary>
    [Reactive] public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Дата публикации
    /// </summary>
    [Reactive] public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Дата публикации (алиас для совместимости)
    /// </summary>
    public DateTime Date => PublishedAt;

    /// <summary>
    /// Автор новости
    /// </summary>
    [Reactive] public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Категория новости
    /// </summary>
    [Reactive] public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Важная ли новость
    /// </summary>
    [Reactive] public bool IsImportant { get; set; }

    /// <summary>
    /// Прочитана ли новость
    /// </summary>
    [Reactive] public bool IsRead { get; set; }

    /// <summary>
    /// URL изображения для новости
    /// </summary>
    [Reactive] public string? ImageUrl { get; set; }

    /// <summary>
    /// Уникальный идентификатор новости
    /// </summary>
    [Reactive] public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Команда клика по новости
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClickCommand { get; }

    /// <summary>
    /// Команда отметки как прочитанной
    /// </summary>
    public ReactiveCommand<Unit, Unit> MarkAsReadCommand { get; }

    /// <summary>
    /// Команда для чтения подробностей
    /// </summary>
    public ReactiveCommand<Unit, Unit> ReadMoreCommand { get; private set; }

    /// <summary>
    /// Создает новый элемент новости
    /// </summary>
    public NewsItemViewModel()
    {
        ClickCommand = ReactiveCommand.Create<Unit, Unit>(_ => Unit.Default);
        
        MarkAsReadCommand = ReactiveCommand.Create<Unit, Unit>(_ => {
            IsRead = true;
            return Unit.Default;
        });
        
        ReadMoreCommand = ReactiveCommand.Create<Unit, Unit>(_ => {
            /* Handle read more */
            return Unit.Default;
        });
    }

    /// <summary>
    /// Создает элемент новости с базовыми данными
    /// </summary>
    public static NewsItemViewModel Create(
        string title,
        string summary,
        string author,
        DateTime? publishedAt = null)
    {
        return new NewsItemViewModel
        {
            Title = title,
            Summary = summary,
            Author = author,
            PublishedAt = publishedAt ?? DateTime.Now
        };
    }

    /// <summary>
    /// Форматированная дата для отображения
    /// </summary>
    public string FormattedDate => PublishedAt.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Время, прошедшее с публикации
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - PublishedAt;
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