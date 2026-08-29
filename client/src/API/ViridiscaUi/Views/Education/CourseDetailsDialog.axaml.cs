using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог просмотра деталей курса
/// </summary>
public partial class CourseDetailsDialog : ReactiveWindow<CourseEditorViewModel>
{
    public CourseDetailsDialog()
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
    public CourseDetailsDialog(CourseEditorViewModel viewModel) : this()
    {
        ViewModel = viewModel;
    }
} 