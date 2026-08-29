using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Domain.Entities.System;
using System.Reactive.Linq;

namespace ViridiscaUi.Views.System
{
    /// <summary>
    /// Полнофункциональное представление для управления отделами
    /// </summary>
    public partial class DepartmentsView : ReactiveUserControl<DepartmentsViewModel>
    {
        public DepartmentsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Обработка клика по карточке отдела для просмотра деталей
        /// </summary>
        private void OnDepartmentClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is Department department && ViewModel != null)
            {
                // Выполняем команду просмотра деталей отдела
                ViewModel.ViewDepartmentDetailsCommand.Execute(department).Subscribe(_ => { });
            }
        }
    }
} 