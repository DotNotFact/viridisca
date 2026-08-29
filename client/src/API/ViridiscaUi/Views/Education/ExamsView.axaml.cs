using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// View для управления экзаменами
/// </summary>
public partial class ExamsView : ReactiveUserControl<ExamsViewModel>
{
    public ExamsView()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
} 