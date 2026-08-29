using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ViridiscaUi.Views.System;

public partial class DepartmentView : UserControl
{
    public DepartmentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 