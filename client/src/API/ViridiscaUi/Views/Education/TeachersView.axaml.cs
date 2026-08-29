using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

public partial class TeachersView : ReactiveUserControl<TeachersViewModel>
{
    public TeachersView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }
} 