using Avalonia.Controls;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System;

public partial class DepartmentEditDialog : Window
{
    public DepartmentEditDialog()
    {
        InitializeComponent();
    }

    public DepartmentEditDialog(DepartmentEditDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
} 