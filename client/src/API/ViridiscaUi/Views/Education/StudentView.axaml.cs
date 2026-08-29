using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ViridiscaUi.Views.Education;

public partial class StudentView : UserControl
{
    public StudentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 