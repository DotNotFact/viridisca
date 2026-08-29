using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ViridiscaUi.Views.Components_New;

public partial class SidebarComponent : UserControl
{
    public SidebarComponent()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 