namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// ViewModel для карточки статистики
/// </summary>
public class StatisticCardViewModel : ViewModelBase
{
    /// <summary>
    /// Заголовок карточки
    /// </summary>
    [Reactive] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Значение статистики
    /// </summary>
    [Reactive] public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Описание или подзаголовок
    /// </summary>
    [Reactive] public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Иконка для карточки
    /// </summary>
    [Reactive] public string IconKey { get; set; } = string.Empty;

    /// <summary>
    /// Цвет карточки (для стилизации)
    /// </summary>
    [Reactive] public string Color { get; set; } = "#007ACC";

    /// <summary>
    /// Показывать ли тренд (стрелку вверх/вниз)
    /// </summary>
    [Reactive] public bool ShowTrend { get; set; }

    /// <summary>
    /// Тренд положительный (true) или отрицательный (false)
    /// </summary>
    [Reactive] public bool IsPositiveTrend { get; set; }

    /// <summary>
    /// Процент изменения для тренда
    /// </summary>
    [Reactive] public double TrendPercentage { get; set; }

    /// <summary>
    /// Команда клика по карточке
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClickCommand { get; }

    /// <summary>
    /// Создает новую карточку статистики
    /// </summary>
    public StatisticCardViewModel()
    {
        ClickCommand = ReactiveCommand.Create(() => { });
    }

    /// <summary>
    /// Создает карточку статистики с базовыми данными
    /// </summary>
    public static StatisticCardViewModel Create(
        string title,
        string value,
        string description = "",
        string iconKey = "Information",
        string color = "#007ACC")
    {
        return new StatisticCardViewModel
        {
            Title = title,
            Value = value,
            Description = description,
            IconKey = iconKey,
            Color = color
        };
    }

    /// <summary>
    /// Устанавливает тренд для карточки
    /// </summary>
    public void SetTrend(double percentage, bool isPositive = true)
    {
        ShowTrend = true;
        TrendPercentage = Math.Abs(percentage);
        IsPositiveTrend = isPositive;
    }
} 