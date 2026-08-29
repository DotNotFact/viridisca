namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для элемента быстрого доступа
/// </summary>
public class QuickLinkViewModel : ReactiveObject
{
    /// <summary>
    /// Название ссылки
    /// </summary>
    [Reactive] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание ссылки
    /// </summary>
    [Reactive] public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Иконка ссылки
    /// </summary>
    [Reactive] public string IconKey { get; set; } = "Link";

    /// <summary>
    /// Путь для навигации
    /// </summary>
    [Reactive] public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Цвет ссылки
    /// </summary>
    [Reactive] public string Color { get; set; } = "#007ACC";

    /// <summary>
    /// Активна ли ссылка
    /// </summary>
    [Reactive] public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Команда клика по ссылке
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateCommand { get; }

    /// <summary>
    /// Создает новую быструю ссылку
    /// </summary>
    public QuickLinkViewModel()
    {
        NavigateCommand = ReactiveCommand.Create(() => { });
    }

    /// <summary>
    /// Создает быструю ссылку с базовыми данными
    /// </summary>
    public QuickLinkViewModel(string title, string description, string iconKey, string route)
    {
        Title = title;
        Description = description;
        IconKey = iconKey;
        Route = route;
        NavigateCommand = ReactiveCommand.Create(() => { });
    }
} 