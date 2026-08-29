using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Представление для управления курсами
/// </summary>
public partial class CoursesView : ReactiveUserControl<CoursesViewModel>
{
    public CoursesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 