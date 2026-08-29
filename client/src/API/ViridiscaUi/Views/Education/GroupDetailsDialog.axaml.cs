using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог просмотра деталей группы
/// </summary>
public partial class GroupDetailsDialog : ReactiveWindow<GroupEditorViewModel>
{
    public GroupDetailsDialog()
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
                    // Команда закрытия - просто закрываем диалог
                    Close();
                }).DisposeWith(disposables);
            }
        });
    }
    
    /// <summary>
    /// Конструктор с ViewModel
    /// </summary>
    public GroupDetailsDialog(GroupEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
        ViewModel = viewModel;
    }
} 