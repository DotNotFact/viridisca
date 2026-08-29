using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания/редактирования студента
/// </summary>
public partial class StudentEditDialog : ReactiveWindow<StudentEditorViewModel>
{
    public StudentEditDialog()
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
    
    /// <summary>
    /// Конструктор с ViewModel
    /// </summary>
    public StudentEditDialog(StudentEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
        ViewModel = viewModel;
    }
} 