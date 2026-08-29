using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education
{
    /// <summary>
    /// Представление для управления заданиями
    /// </summary>
    public partial class AssignmentsView : ReactiveUserControl<AssignmentsViewModel>
    {
        public AssignmentsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 