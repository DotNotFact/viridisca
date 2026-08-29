using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог просмотра деталей преподавателя
/// </summary>
public partial class TeacherDetailsDialog : ReactiveWindow<TeacherEditorViewModel>
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public TeacherDetailsDialog()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Закрытие диалога при выполнении команд
            if (ViewModel != null)
            {
                ViewModel.EditCommand.Subscribe(_ =>
                {
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
    public TeacherDetailsDialog(TeacherEditorViewModel viewModel) : this()
    {
        ViewModel = viewModel;
        DataContext = viewModel;
    }
} 