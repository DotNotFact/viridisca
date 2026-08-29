using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// View для управления расписанием
/// </summary>
public partial class ScheduleView : ReactiveUserControl<ScheduleViewModel>
{
    public ScheduleView()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
} 