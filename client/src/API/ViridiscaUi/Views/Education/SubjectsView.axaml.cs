using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// View для управления предметами
/// </summary>
public partial class SubjectsView : ReactiveUserControl<SubjectsViewModel>
{
    public SubjectsView()
    {
        InitializeComponent();
    }
} 