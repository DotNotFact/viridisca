using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Диалог создания/редактирования предмета
/// </summary>
public partial class SubjectEditDialog : ReactiveWindow<SubjectEditorViewModel>
{
    public SubjectEditDialog()
    {
        InitializeComponent();
    }
} 