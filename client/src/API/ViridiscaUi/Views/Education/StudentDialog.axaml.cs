using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания/редактирования студента
/// </summary>
public partial class StudentDialog : ReactiveWindow<StudentDialogViewModel>
{
    public StudentDialog()
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
    
    public StudentDialog(StudentDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
} 