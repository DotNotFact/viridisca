using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Views.Components;

/// <summary>
/// Современный адаптивный компонент боковой панели навигации
/// Поддерживает сворачивание, Material Design иконки и условное отображение
/// </summary>
public partial class SidebarComponent : UserControl
{
    private readonly CompositeDisposable _disposables = new();

    public SidebarComponent()
    {
        InitializeComponent();
        SetupDataContextBinding();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SetupDataContextBinding()
    {
        // Подписываемся на изменения DataContext для получения MainViewModel
        this.WhenAnyValue(x => x.DataContext)
            .OfType<MainViewModel>()
            .Subscribe(mainViewModel =>
            {
                StatusLogger.LogInfo($"Sidebar получил MainViewModel с {mainViewModel.GroupedMenuItems.Count} группами меню", "SidebarComponent");
            })
            .DisposeWith(_disposables);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    }

    ~SidebarComponent()
    {
        _disposables?.Dispose();
    }
} 