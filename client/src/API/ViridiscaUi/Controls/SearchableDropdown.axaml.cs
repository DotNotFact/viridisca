using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System.Windows.Input;
using Avalonia.Controls;
using System.Linq;
using Avalonia;

namespace ViridiscaUi.Controls;

public partial class SearchableDropdown : UserControl
{
    public static readonly StyledProperty<IEnumerable<object>> ItemsProperty =
        AvaloniaProperty.Register<SearchableDropdown, IEnumerable<object>>(nameof(Items));

    public static readonly StyledProperty<ICommand> AddCommandProperty =
        AvaloniaProperty.Register<SearchableDropdown, ICommand>(nameof(AddCommand));

    public static readonly StyledProperty<object> SelectedItemProperty =
        AvaloniaProperty.Register<SearchableDropdown, object>(nameof(SelectedItem));

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<SearchableDropdown, string>(nameof(Watermark));

    public static readonly StyledProperty<string> DisplayMemberPathProperty =
        AvaloniaProperty.Register<SearchableDropdown, string>(nameof(DisplayMemberPath));

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<SearchableDropdown, string>(nameof(PlaceholderText));

    private readonly ObservableCollection<object> _filteredItems = [];

    public IEnumerable<object> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public ICommand AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public string DisplayMemberPath
    {
        get => GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public SearchableDropdown()
    {
        InitializeComponent();
        
        // Initialize collections
        Items = new ObservableCollection<DropdownItem>();

        // Wire up events
        SearchBox.TextChanged += OnSearchTextChanged;
        AddButton.Click += OnAddButtonClick;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text?.ToLower() ?? string.Empty;
        
        if (Items == null)
            return;

        var filteredItems = Items.Where(item => 
        {
            var displayText = GetDisplayText(item);
            return displayText.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase);
        }).ToList();
        
        _filteredItems.Clear();
      
        foreach (var item in filteredItems)
        {
            _filteredItems.Add(item);
        }
        
        // Assuming ItemsList is a ListBox or similar control
        ItemsList?.ItemsSource = _filteredItems;
    }

    private string GetDisplayText(object item)
    {
        if (!string.IsNullOrEmpty(DisplayMemberPath) && item != null)
        {
            var property = item.GetType().GetProperty(DisplayMemberPath);
      
            if (property != null)
            {
                var value = property.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }
        }

        return item switch
        {
            DropdownItem dropdownItem => dropdownItem.DisplayText ?? string.Empty,
            string str => str,
            _ => item?.ToString() ?? string.Empty
        };
    }

    private void OnAddButtonClick(object sender, RoutedEventArgs e)
    {
        AddCommand?.Execute(null);
    }
}

public class DropdownItem
{
    public string DisplayText { get; set; } = string.Empty;
    public object? Value { get; set; }
}
