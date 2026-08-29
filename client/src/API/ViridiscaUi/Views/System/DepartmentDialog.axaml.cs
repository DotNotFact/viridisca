using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System;

/// <summary>
/// Диалог создания/редактирования департамента
/// </summary>
public partial class DepartmentDialog : ReactiveWindow<DepartmentDialogViewModel>
{
    public DepartmentDialog()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Подписываемся на команды
            if (ViewModel != null)
            {
                ViewModel.SaveCommand.Subscribe(result =>
                {
                    if (result != null)
                    {
                        Close(result);
                    }
                }).DisposeWith(disposables);

                ViewModel.CancelCommand.Subscribe(_ =>
                {
                    Close(null);
                }).DisposeWith(disposables);
            }
        });
    }
} 