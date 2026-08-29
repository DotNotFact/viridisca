using Avalonia.Controls;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System;

public partial class DepartmentDetailsDialog : Window
{
    public DepartmentDetailsDialog()
    {
        InitializeComponent();
    }

    public DepartmentDetailsDialog(DepartmentDetailsDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
} 