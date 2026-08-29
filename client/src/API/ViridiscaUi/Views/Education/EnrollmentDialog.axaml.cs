using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

public partial class EnrollmentDialog : UserControl, IViewFor<EnrollmentDialogViewModel>
{
    public EnrollmentDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public EnrollmentDialogViewModel? ViewModel
    {
        get => DataContext as EnrollmentDialogViewModel;
        set => DataContext = value;
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as EnrollmentDialogViewModel;
    }
} 