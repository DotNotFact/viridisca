using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог просмотра деталей предмета
/// </summary>
public partial class SubjectDetailsDialog : ReactiveWindow<SubjectViewModel>
{
    public SubjectDetailsDialog()
    {
        InitializeComponent();
    }

    private void CloseDialog(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 