using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания и редактирования преподавателя
/// </summary>
public partial class TeacherEditDialog : ReactiveWindow<TeacherEditorViewModel>
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public TeacherEditDialog()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Закрытие диалога при выполнении команд
            if (ViewModel != null)
            {
                ViewModel.SaveCommand.Subscribe(_ =>
                {
                    Close(ViewModel.CurrentTeacher);
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
    public TeacherEditDialog(TeacherEditorViewModel viewModel) : this()
    {
        ViewModel = viewModel;
        DataContext = viewModel;
    }
} 