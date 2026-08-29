using Avalonia.Controls;
using ReactiveUI;
using ViridiscaUi.ViewModels.System;
using Avalonia;
using System;

namespace ViridiscaUi.Views.System;

/// <summary>
/// View для управления профилем пользователя с адаптивным дизайном
/// </summary>
public partial class ProfileView : UserControl, IViewFor<ProfileViewModel>
{
    public ProfileView()
    {
        InitializeComponent();
        
        // Подписываемся на изменение размера для адаптивности
        this.GetObservable(BoundsProperty).Subscribe(OnBoundsChanged);
    }

    private void OnBoundsChanged(Rect bounds)
    {
        // Адаптивная логика: если ширина меньше 800px, переключаемся на мобильную версию
        var mainContentGrid = this.FindControl<Grid>("MainContentGrid");
        var personalInfoGrid = this.FindControl<Grid>("PersonalInfoGrid");
        
        if (mainContentGrid != null)
        {
            if (bounds.Width < 800)
            {
                // Мобильная версия: одна колонка
                mainContentGrid.ColumnDefinitions.Clear();
                mainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                
                // Убираем отступы между колонками
                var rightColumn = mainContentGrid.Children.Count > 1 ? mainContentGrid.Children[1] : null;
                if (rightColumn is StackPanel rightPanel)
                {
                    rightPanel.Margin = new Thickness(0, 16, 0, 0);
                    Grid.SetColumn(rightPanel, 0);
                    Grid.SetRow(rightPanel, 1);
                }
            }
            else
            {
                // Десктопная версия: две колонки
                mainContentGrid.ColumnDefinitions.Clear();
                mainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
                mainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                
                // Восстанавливаем отступы
                var rightColumn = mainContentGrid.Children.Count > 1 ? mainContentGrid.Children[1] : null;
                if (rightColumn is StackPanel rightPanel)
                {
                    rightPanel.Margin = new Thickness(8, 0, 0, 0);
                    Grid.SetColumn(rightPanel, 1);
                    Grid.SetRow(rightPanel, 0);
                }
            }
        }
        
        // Адаптивность для полей ввода
        if (personalInfoGrid != null)
        {
            if (bounds.Width < 600)
            {
                // Очень узкий экран: все поля в одну колонку
                personalInfoGrid.ColumnDefinitions.Clear();
                personalInfoGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                
                // Перестраиваем поля в одну колонку
                for (int i = 0; i < personalInfoGrid.Children.Count; i++)
                {
                    var child = personalInfoGrid.Children[i];
                    Grid.SetColumn(child, 0);
                    Grid.SetRow(child, i);
                    
                    if (child is StackPanel panel)
                    {
                        panel.Margin = new Thickness(0, 0, 0, 8);
                    }
                }
            }
            else
            {
                // Широкий экран: две колонки
                personalInfoGrid.ColumnDefinitions.Clear();
                personalInfoGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                personalInfoGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }
        }
    }

    public ProfileViewModel? ViewModel
    {
        get => DataContext as ProfileViewModel;
        set => DataContext = value;
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as ProfileViewModel;
    }
} 