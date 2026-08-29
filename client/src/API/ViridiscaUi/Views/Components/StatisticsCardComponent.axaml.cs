using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reactive.Linq;

namespace ViridiscaUi.Views.Components
{
    /// <summary>
    /// Компонент для отображения статистических карточек
    /// </summary>
    public partial class StatisticsCardComponent : UserControl
    {
        // Dependency Properties
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, string>(nameof(Title), "Статистика");

        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, string>(nameof(Value), "0");

        public static readonly StyledProperty<string> IconProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, string>(nameof(Icon), "📊");

        public static readonly StyledProperty<bool> ShowChangeProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, bool>(nameof(ShowChange), false);

        public static readonly StyledProperty<string> ChangeTextProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, string>(nameof(ChangeText), "");

        public static readonly StyledProperty<string> ChangeIconProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, string>(nameof(ChangeIcon), "");

        public static readonly StyledProperty<ChangeType> ChangeTypeProperty =
            AvaloniaProperty.Register<StatisticsCardComponent, ChangeType>(nameof(ChangeType), ChangeType.Neutral);

        // Properties
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool ShowChange
        {
            get => GetValue(ShowChangeProperty);
            set => SetValue(ShowChangeProperty, value);
        }

        public string ChangeText
        {
            get => GetValue(ChangeTextProperty);
            set => SetValue(ChangeTextProperty, value);
        }

        public string ChangeIcon
        {
            get => GetValue(ChangeIconProperty);
            set => SetValue(ChangeIconProperty, value);
        }

        public ChangeType ChangeType
        {
            get => GetValue(ChangeTypeProperty);
            set => SetValue(ChangeTypeProperty, value);
        }

        public StatisticsCardComponent()
        {
            InitializeComponent();
            
            // Устанавливаем начальную иконку изменения
            UpdateChangeIcon(ChangeType);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateChangeIcon(ChangeType changeType)
        {
            ChangeIcon = changeType switch
            {
                ChangeType.Positive => "📈",
                ChangeType.Negative => "📉",
                ChangeType.Neutral => "➡️",
                _ => ""
            };
        }

        /// <summary>
        /// Устанавливает изменение с автоматическим определением типа
        /// </summary>
        public void SetChange(double change, string suffix = "")
        {
            if (change > 0)
            {
                ChangeType = ChangeType.Positive;
                ChangeText = $"+{change:F1}{suffix}";
            }
            else if (change < 0)
            {
                ChangeType = ChangeType.Negative;
                ChangeText = $"{change:F1}{suffix}";
            }
            else
            {
                ChangeType = ChangeType.Neutral;
                ChangeText = $"0{suffix}";
            }
            
            ShowChange = true;
        }

        /// <summary>
        /// Устанавливает процентное изменение
        /// </summary>
        public void SetPercentageChange(double percentage)
        {
            SetChange(percentage, "%");
        }
    }

    /// <summary>
    /// Тип изменения для статистической карточки
    /// </summary>
    public enum ChangeType
    {
        Positive,
        Negative,
        Neutral
    }
} 