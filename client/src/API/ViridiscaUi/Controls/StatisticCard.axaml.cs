using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Material.Icons;

namespace ViridiscaUi.Controls;

/// <summary>
/// Пользовательский контрол для отображения статистических карточек
/// </summary>
public partial class StatisticCard : UserControl
{
    public StatisticCard()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Основное значение статистики
    /// </summary>
    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Value), "0");

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Подпись статистики
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Label), "Статистика");

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Иконка статистики
    /// </summary>
    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<StatisticCard, MaterialIconKind>(nameof(Icon), MaterialIconKind.ChartLine);

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Изменение значения (например, "+5%" или "-2%")
    /// </summary>
    public static readonly StyledProperty<string> ChangeProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Change), string.Empty);

    public string Change
    {
        get => GetValue(ChangeProperty);
        set => SetValue(ChangeProperty, value);
    }

    /// <summary>
    /// Дополнительное описание
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<StatisticCard, string>(nameof(Description), string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Цветовая тема карточки
    /// </summary>
    public static readonly StyledProperty<StatisticCardTheme> CardThemeProperty =
        AvaloniaProperty.Register<StatisticCard, StatisticCardTheme>(nameof(CardTheme), StatisticCardTheme.Default);

    public StatisticCardTheme CardTheme
    {
        get => GetValue(CardThemeProperty);
        set => SetValue(CardThemeProperty, value);
    }

    /// <summary>
    /// Показывать ли изменение
    /// </summary>
    public static readonly StyledProperty<bool> ShowChangeProperty =
        AvaloniaProperty.Register<StatisticCard, bool>(nameof(ShowChange), false);

    public bool ShowChange
    {
        get => GetValue(ShowChangeProperty);
        set => SetValue(ShowChangeProperty, value);
    }

    /// <summary>
    /// Показывать ли описание
    /// </summary>
    public static readonly StyledProperty<bool> ShowDescriptionProperty =
        AvaloniaProperty.Register<StatisticCard, bool>(nameof(ShowDescription), false);

    public bool ShowDescription
    {
        get => GetValue(ShowDescriptionProperty);
        set => SetValue(ShowDescriptionProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CardThemeProperty)
        {
            UpdateTheme();
        }
    }

    private void UpdateTheme()
    {
        if (CardBorder == null) return;

        // Удаляем все темы
        CardBorder.Classes.Remove("primary");
        CardBorder.Classes.Remove("success");
        CardBorder.Classes.Remove("warning");
        CardBorder.Classes.Remove("danger");

        // Добавляем нужную тему
        switch (CardTheme)
        {
            case StatisticCardTheme.Primary:
                CardBorder.Classes.Add("primary");
                break;
            case StatisticCardTheme.Success:
                CardBorder.Classes.Add("success");
                break;
            case StatisticCardTheme.Warning:
                CardBorder.Classes.Add("warning");
                break;
            case StatisticCardTheme.Danger:
                CardBorder.Classes.Add("danger");
                break;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateTheme();
    }
}

/// <summary>
/// Темы для статистических карточек
/// </summary>
public enum StatisticCardTheme
{
    Default,
    Primary,
    Success,
    Warning,
    Danger
} 