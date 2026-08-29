using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог просмотра деталей студента
/// </summary>
public partial class StudentDetailsDialog : ReactiveWindow<StudentEditorViewModel>
{
    public StudentDetailsDialog()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Подписываемся на команды
            if (ViewModel != null)
            {
                ViewModel.EditCommand.Subscribe(_ =>
                {
                    // Команда редактирования - закрываем диалог с результатом для редактирования
                    Close("edit");
                }).DisposeWith(disposables);
                
                ViewModel.CloseCommand.Subscribe(_ =>
                {
                    Close(null);
                }).DisposeWith(disposables);
            }
        });
    }
    
    /// <summary>
    /// Конструктор с ViewModel
    /// </summary>
    public StudentDetailsDialog(StudentEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
        ViewModel = viewModel;
    }
} 