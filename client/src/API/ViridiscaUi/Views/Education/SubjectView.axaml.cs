using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ViridiscaUi.Views.Education;

public partial class SubjectView : UserControl
{
    public SubjectView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 