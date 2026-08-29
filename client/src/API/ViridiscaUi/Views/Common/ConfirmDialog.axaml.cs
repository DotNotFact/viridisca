using Avalonia.Controls;
using Avalonia.Interactivity;
using Material.Icons;

namespace ViridiscaUi.Views.Common;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; } = false;

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string title, string message, string confirmText = "Подтвердить", string cancelText = "Отмена", MaterialIconKind iconKind = MaterialIconKind.HelpCircle)
    {
        InitializeComponent();
        
        Title = title;
        DialogTitle.Text = title;
        DialogMessage.Text = message;
        ConfirmButton.Content = confirmText;
        CancelButton.Content = cancelText;
        DialogIcon.Kind = iconKind;
        
        // Set appropriate styling based on action type
        if (confirmText.Contains("Удалить") || confirmText.Contains("Delete"))
        {
            ConfirmButton.Classes.Clear();
            ConfirmButton.Classes.Add("btn-danger");
            DialogIcon.Kind = MaterialIconKind.DeleteAlert;
            DialogIcon.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red);
        }
        else if (title.Contains("Предупреждение") || title.Contains("Warning"))
        {
            DialogIcon.Kind = MaterialIconKind.Alert;
            DialogIcon.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Orange);
        }
        else if (title.Contains("Ошибка") || title.Contains("Error"))
        {
            DialogIcon.Kind = MaterialIconKind.AlertCircle;
            DialogIcon.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red);
        }
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    public static async Task<bool> ShowAsync(Window? owner, string title, string message, string confirmText = "Подтвердить", string cancelText = "Отмена", MaterialIconKind iconKind = MaterialIconKind.HelpCircle)
    {
        var dialog = new ConfirmDialog(title, message, confirmText, cancelText, iconKind);
        if (owner != null)
        {
            await dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }
        return dialog.Result;
    }
} 