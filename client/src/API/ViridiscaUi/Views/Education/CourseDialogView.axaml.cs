using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ViridiscaUi.Views.Education;

public partial class CourseDialogView : UserControl
{
    public CourseDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 