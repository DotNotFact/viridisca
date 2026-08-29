namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для элемента активности пользователя
/// </summary>
public class ActivityItemViewModel : ReactiveObject
{
    [Reactive] public Guid Id { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public string Type { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string IconKey { get; set; } = "Information";

    /// <summary>
    /// Пользователь, выполнивший активность
    /// </summary>
    [Reactive] public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Детали активности (JSON или текст)
    /// </summary>
    [Reactive] public string? Details { get; set; }

    /// <summary>
    /// IP адрес пользователя
    /// </summary>
    [Reactive] public string? IpAddress { get; set; }

    /// <summary>
    /// Устройство пользователя
    /// </summary>
    [Reactive] public string? UserAgent { get; set; }

    /// <summary>
    /// Важная ли активность
    /// </summary>
    [Reactive] public bool IsImportant { get; set; }

    /// <summary>
    /// Успешна ли активность
    /// </summary>
    [Reactive] public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Команда клика по активности
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClickCommand { get; private set; } = null!;

    public ActivityItemViewModel()
    {
        ClickCommand = ReactiveCommand.Create<Unit, Unit>(_ => Unit.Default);
    }

    public ActivityItemViewModel(Guid id, string title, DateTime date, string type, string description = "", string iconKey = "Information")
    {
        Id = id;
        Title = title;
        Date = date;
        Type = type;
        Description = description;
        IconKey = iconKey;
        
        ClickCommand = ReactiveCommand.Create<Unit, Unit>(_ => Unit.Default);
    }

    /// <summary>
    /// Иконка для типа активности
    /// </summary>
    public string TypeIcon => Type.ToLower() switch
    {
        "login" => "Login",
        "logout" => "Logout",
        "create" => "Plus",
        "update" => "Edit",
        "delete" => "Delete",
        "view" => "Eye",
        "download" => "Download",
        "upload" => "Upload",
        _ => "Activity"
    };

    /// <summary>
    /// Цвет для типа активности
    /// </summary>
    public string TypeColor => IsSuccessful switch
    {
        true when IsImportant => "#FFC107",
        true => "#28A745",
        false => "#DC3545"
    };

    /// <summary>
    /// Форматированная дата для отображения
    /// </summary>
    public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Относительное время (например, "2 часа назад")
    /// </summary>
    public string RelativeTime
    {
        get
        {
            var timeSpan = DateTime.Now - Date;
            if (timeSpan.TotalMinutes < 1) return "только что";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} мин назад";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} ч назад";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} дн назад";
            return Date.ToString("dd.MM.yyyy");
        }
    }
} 