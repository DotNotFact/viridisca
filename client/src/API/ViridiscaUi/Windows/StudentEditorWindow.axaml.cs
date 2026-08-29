using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Windows;

public partial class StudentEditorWindow : Window
{
    /// <summary>
    /// ViewModel для окна редактирования студента
    /// </summary>
    public StudentEditorViewModel? ViewModel
    {
        get => DataContext as StudentEditorViewModel;
        set => DataContext = value;
    }

    public StudentEditorWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 