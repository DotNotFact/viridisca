using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System;

/// <summary>
/// View для библиотечной системы
/// </summary>
public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
} 