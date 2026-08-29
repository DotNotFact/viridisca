using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education
{
    /// <summary>
    /// Представление для управления группами
    /// </summary>
    public partial class GroupsView : ReactiveUserControl<GroupsViewModel>
    {
        public GroupsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 