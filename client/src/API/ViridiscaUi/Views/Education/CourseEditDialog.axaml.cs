using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания/редактирования курса
/// </summary>
public partial class CourseEditDialog : ReactiveWindow<CourseEditorViewModel>
{
    public CourseEditDialog()
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
    public CourseEditDialog(CourseEditorViewModel viewModel) : this()
    {
        ViewModel = viewModel;
    }
} 