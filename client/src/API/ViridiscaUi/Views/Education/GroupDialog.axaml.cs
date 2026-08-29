using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания/редактирования группы
/// </summary>
public partial class GroupDialog : ReactiveWindow<GroupDialogViewModel>
{
    public GroupDialog()
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
    
    public GroupDialog(GroupDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
} 