using System;
using System.Collections;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ViridiscaUi.Views.Components
{
    /// <summary>
    /// Переиспользуемый компонент DataGrid с поиском и пагинацией
    /// </summary>
    public partial class DataGridComponent : UserControl
    {
        // Dependency Properties
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<DataGridComponent, string>(nameof(Title), "Данные");

        public static readonly StyledProperty<IEnumerable> ItemsProperty =
            AvaloniaProperty.Register<DataGridComponent, IEnumerable>(nameof(Items));

        public static readonly StyledProperty<object?> SelectedItemProperty =
            AvaloniaProperty.Register<DataGridComponent, object?>(nameof(SelectedItem));

        public static readonly StyledProperty<int> CurrentPageProperty =
            AvaloniaProperty.Register<DataGridComponent, int>(nameof(CurrentPage), 1);

        public static readonly StyledProperty<int> TotalPagesProperty =
            AvaloniaProperty.Register<DataGridComponent, int>(nameof(TotalPages), 1);

        public static readonly StyledProperty<int> ItemsCountProperty =
            AvaloniaProperty.Register<DataGridComponent, int>(nameof(ItemsCount));

        public static readonly StyledProperty<int> TotalCountProperty =
            AvaloniaProperty.Register<DataGridComponent, int>(nameof(TotalCount));

        public static readonly StyledProperty<bool> CanGoToPreviousPageProperty =
            AvaloniaProperty.Register<DataGridComponent, bool>(nameof(CanGoToPreviousPage));

        public static readonly StyledProperty<bool> CanGoToNextPageProperty =
            AvaloniaProperty.Register<DataGridComponent, bool>(nameof(CanGoToNextPage));

        // Command Properties
        public static readonly StyledProperty<ICommand?> SearchCommandProperty =
            AvaloniaProperty.Register<DataGridComponent, ICommand?>(nameof(SearchCommand));

        public static readonly StyledProperty<ICommand?> FirstPageCommandProperty =
            AvaloniaProperty.Register<DataGridComponent, ICommand?>(nameof(FirstPageCommand));

        public static readonly StyledProperty<ICommand?> PreviousPageCommandProperty =
            AvaloniaProperty.Register<DataGridComponent, ICommand?>(nameof(PreviousPageCommand));

        public static readonly StyledProperty<ICommand?> NextPageCommandProperty =
            AvaloniaProperty.Register<DataGridComponent, ICommand?>(nameof(NextPageCommand));

        public static readonly StyledProperty<ICommand?> LastPageCommandProperty =
            AvaloniaProperty.Register<DataGridComponent, ICommand?>(nameof(LastPageCommand));

        // Properties
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public IEnumerable Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public int CurrentPage
        {
            get => GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public int TotalPages
        {
            get => GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }

        public int ItemsCount
        {
            get => GetValue(ItemsCountProperty);
            set => SetValue(ItemsCountProperty, value);
        }

        public int TotalCount
        {
            get => GetValue(TotalCountProperty);
            set => SetValue(TotalCountProperty, value);
        }

        public bool CanGoToPreviousPage
        {
            get => GetValue(CanGoToPreviousPageProperty);
            set => SetValue(CanGoToPreviousPageProperty, value);
        }

        public bool CanGoToNextPage
        {
            get => GetValue(CanGoToNextPageProperty);
            set => SetValue(CanGoToNextPageProperty, value);
        }

        // Commands
        public ICommand? SearchCommand
        {
            get => GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public ICommand? FirstPageCommand
        {
            get => GetValue(FirstPageCommandProperty);
            set => SetValue(FirstPageCommandProperty, value);
        }

        public ICommand? PreviousPageCommand
        {
            get => GetValue(PreviousPageCommandProperty);
            set => SetValue(PreviousPageCommandProperty, value);
        }

        public ICommand? NextPageCommand
        {
            get => GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }

        public ICommand? LastPageCommand
        {
            get => GetValue(LastPageCommandProperty);
            set => SetValue(LastPageCommandProperty, value);
        }

        public DataGridComponent()
        {
            InitializeComponent();
            
            // Обновляем computed properties при изменении CurrentPage и TotalPages
            this.GetObservable(CurrentPageProperty).Subscribe(_ => UpdatePaginationState());
            this.GetObservable(TotalPagesProperty).Subscribe(_ => UpdatePaginationState());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdatePaginationState()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }

        /// <summary>
        /// Получает DataGrid для программного добавления колонок
        /// </summary>
        public DataGrid GetDataGrid()
        {
            return this.FindControl<DataGrid>("MainDataGrid") ?? throw new InvalidOperationException("DataGrid not found");
        }
    }
} 