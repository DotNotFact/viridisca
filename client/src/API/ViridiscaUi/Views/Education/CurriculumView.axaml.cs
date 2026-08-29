using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// View для управления учебными планами
/// </summary>
public partial class CurriculumView : ReactiveUserControl<CurriculumViewModel>
{
    public CurriculumView()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
} 