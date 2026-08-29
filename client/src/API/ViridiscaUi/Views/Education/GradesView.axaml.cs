using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

public partial class GradesView : ReactiveUserControl<GradesViewModel>
{
    public GradesView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 