namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для элемента события
/// </summary>
public class EventItemViewModel : ViewModelBase
{
    /// <summary>
    /// Название события
    /// </summary>
    [Reactive] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события
    /// </summary>
    [Reactive] public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    [Reactive] public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    [Reactive] public DateTime EndDate { get; set; }

    /// <summary>
    /// Место проведения события
    /// </summary>
    [Reactive] public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Организатор события
    /// </summary>
    [Reactive] public string Organizer { get; set; } = string.Empty;

    /// <summary>
    /// Тип события
    /// </summary>
    [Reactive] public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Важное ли событие
    /// </summary>
    [Reactive] public bool IsImportant { get; set; }

    /// <summary>
    /// Подтверждено ли участие
    /// </summary>
    [Reactive] public bool IsConfirmed { get; set; }

    /// <summary>
    /// Цвет события для календаря
    /// </summary>
    [Reactive] public string Color { get; set; } = "#007ACC";

    /// <summary>
    /// Дата события
    /// </summary>
    [Reactive] public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Тип события
    /// </summary>
    [Reactive] public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    [Reactive] public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Команда для выполнения действия по событию
    /// </summary>
    public ReactiveCommand<Unit, Unit> ActionCommand { get; private set; }

    /// <summary>
    /// Создает новый элемент события
    /// </summary>
    public EventItemViewModel()
    {
        ActionCommand = ReactiveCommand.Create(() => { });
    }

    /// <summary>
    /// Создает элемент события с базовыми данными
    /// </summary>
    public static EventItemViewModel Create(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        string location = "")
    {
        return new EventItemViewModel
        {
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Location = location
        };
    }

    /// <summary>
    /// Форматированная дата начала для отображения
    /// </summary>
    public string FormattedStartDate => StartDate.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Форматированная дата окончания для отображения
    /// </summary>
    public string FormattedEndDate => EndDate.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Продолжительность события
    /// </summary>
    public string Duration
    {
        get
        {
            var timeSpan = EndDate - StartDate;
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} мин.";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ч. {(int)(timeSpan.TotalMinutes % 60)} мин.";
            return $"{(int)timeSpan.TotalDays} дн. {(int)(timeSpan.TotalHours % 24)} ч.";
        }
    }

    /// <summary>
    /// Статус события (предстоящее, текущее, завершенное)
    /// </summary>
    public string Status
    {
        get
        {
            var now = DateTime.Now;
            if (now < StartDate)
                return "Предстоящее";
            if (now >= StartDate && now <= EndDate)
                return "Текущее";
            return "Завершенное";
        }
    }

    /// <summary>
    /// Время до начала события
    /// </summary>
    public string TimeUntilStart
    {
        get
        {
            var timeSpan = StartDate - DateTime.Now;
            if (timeSpan.TotalMinutes < 0)
                return "Началось";
            if (timeSpan.TotalMinutes < 60)
                return $"через {(int)timeSpan.TotalMinutes} мин.";
            if (timeSpan.TotalHours < 24)
                return $"через {(int)timeSpan.TotalHours} ч.";
            return $"через {(int)timeSpan.TotalDays} дн.";
        }
    }
} 