using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;
using ViridiscaUi.Windows;

namespace ViridiscaUi.ViewModels.Components;

/// <summary>
/// ViewModel для StatusBar компонента
/// </summary>
public class StatusBarViewModel : RoutableViewModelBase
{
    private readonly IStatusService _statusService;
    private StatusHistoryWindow? _historyWindow;

    public ReadOnlyObservableCollection<StatusMessage> Messages => _statusService.Messages;

    [Reactive] public StatusMessage? CurrentMessage { get; private set; }
    [Reactive] public bool IsStatusMessageVisible { get; private set; }
    [Reactive] public bool IsHistoryVisible { get; private set; }
    [Reactive] public int TotalMessagesCount { get; private set; }
    [Reactive] public int ErrorsCount { get; private set; }
    [Reactive] public int WarningsCount { get; private set; }
    [Reactive] public int InfoCount { get; private set; }

    public ReactiveCommand<Unit, Unit> ToggleHistoryCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; private set; } = null!;
    public ReactiveCommand<StatusMessage, Unit> CopyMessageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CopyAllMessagesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CloseHistoryCommand { get; private set; } = null!;
    
    // Новые команды для копирования по типу сообщений
    public ReactiveCommand<Unit, Unit> CopyErrorMessagesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CopyWarningMessagesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CopyInfoMessagesCommand { get; private set; } = null!;

    public StatusBarViewModel(IStatusService statusService, IScreen hostScreen) : base(hostScreen)
    {
        _statusService = statusService;

        // Initialize commands
        ToggleHistoryCommand = ReactiveCommand.Create<Unit, Unit>(_ => {
            ToggleHistory();
            return Unit.Default;
        });
        
        ClearHistoryCommand = ReactiveCommand.Create<Unit, Unit>(_ => {
            ClearHistory();
            return Unit.Default;
        });
        
        CopyMessageCommand = ReactiveCommand.CreateFromTask<StatusMessage>(CopyMessageAsync);
        
        CopyAllMessagesCommand = ReactiveCommand.CreateFromTask(CopyAllMessagesAsync);
        
        CloseHistoryCommand = ReactiveCommand.Create<Unit, Unit>(_ => {
            IsHistoryVisible = false;
            return Unit.Default;
        });
        
        CopyErrorMessagesCommand = ReactiveCommand.CreateFromTask(() => CopyMessagesOfTypeAsync(StatusMessageType.Error));
        
        CopyWarningMessagesCommand = ReactiveCommand.CreateFromTask(() => CopyMessagesOfTypeAsync(StatusMessageType.Warning));
        
        CopyInfoMessagesCommand = ReactiveCommand.CreateFromTask(() => CopyMessagesOfTypeAsync(StatusMessageType.Info));

        // Инициализация начальных значений
        CurrentMessage = _statusService.CurrentMessage;
        IsStatusMessageVisible = CurrentMessage != null;
        UpdateCounts();

        this.WhenAnyValue(x => x.IsHistoryVisible)
            .Subscribe(visible =>
            {
                // Handle history visibility change
                if (visible)
                {
                    // Load history if needed
                }
            })
            .DisposeWith(Disposables);
    }

    private void ClearHistory()
    {
        _statusService.Clear();
        UpdateCounts();
    }

    private void CopyMessage(StatusMessage message)
    {
        _ = CopyMessageAsync(message);
    }

    private void CopyAllMessages()
    {
        _ = CopyAllMessagesAsync();
    }

    private void CopyErrorMessages()
    {
        _ = CopyMessagesOfTypeAsync(StatusMessageType.Error);
    }

    private void CopyWarningMessages()
    {
        _ = CopyMessagesOfTypeAsync(StatusMessageType.Warning);
    }

    private void CopyInfoMessages()
    {
        _ = CopyMessagesOfTypeAsync(StatusMessageType.Info);
    }

    private void ToggleHistory()
    {
        if (_historyWindow == null || !_historyWindow.IsVisible)
        {
            ShowHistory();
        }
        else
        {
            CloseHistory();
        }
    }

    private void ShowHistory()
    {
        if (_historyWindow != null && _historyWindow.IsVisible)
            return;

        try
        {
            // Получаем главное окно
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;

            // Создаем новое окно истории
            _historyWindow = new StatusHistoryWindow
            {
                DataContext = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Подписываемся на закрытие окна
            _historyWindow.Closed += (s, e) =>
            {
                IsHistoryVisible = false;
                _historyWindow = null;
            };

            // Подписываемся на закрытие главного окна, чтобы закрыть окно истории
            void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
            {
                if (_historyWindow != null && _historyWindow.IsVisible)
                {
                    _historyWindow.Close();
                }
                mainWindow.Closing -= OnMainWindowClosing;
            }
            
            mainWindow.Closing += OnMainWindowClosing;

            // Для истории используем немодальное окно (Show), но с правильным управлением
            _historyWindow.Show(mainWindow);
            IsHistoryVisible = true;
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка открытия окна истории: {ex.Message}", "StatusBar");
        }
    }

    private void CloseHistory()
    {
        if (_historyWindow != null)
        {
            _historyWindow.Close();
            _historyWindow = null;
        }
        IsHistoryVisible = false;
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private void UpdateCounts()
    {
        TotalMessagesCount = _statusService?.TotalMessagesCount ?? 0;
        ErrorsCount = _statusService?.ErrorsCount ?? 0;
        WarningsCount = _statusService?.WarningsCount ?? 0;
        InfoCount = _statusService?.InfoCount ?? 0;
    }

    private async Task CopyMessageAsync(StatusMessage message)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(message.CopyableText);
                    _statusService?.ShowSuccess("Сообщение скопировано в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования: {ex.Message}", "StatusBar");
        }
    }

    private async Task CopyAllMessagesAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null && Messages.Count > 0)
                {
                    var allMessages = string.Join(Environment.NewLine, Messages.Select(m => m.CopyableText));
                    await topLevel.Clipboard.SetTextAsync(allMessages);
                    _statusService?.ShowSuccess($"Скопировано {Messages.Count} сообщений в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования всех сообщений: {ex.Message}", "StatusBar");
        }
    }

    private async Task CopyMessagesOfTypeAsync(StatusMessageType type)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null)
                {
                    var messagesOfType = Messages.Where(m => m.Type == type).ToList();
                    
                    if (messagesOfType.Any())
                    {
                        var messagesText = string.Join(Environment.NewLine, messagesOfType.Select(m => m.CopyableText));
                        await topLevel.Clipboard.SetTextAsync(messagesText);
                        
                        string typeName = type switch {
                            StatusMessageType.Error => "ошибок",
                            StatusMessageType.Warning => "предупреждений",
                            StatusMessageType.Info => "информационных сообщений",
                            _ => "сообщений"
                        };
                        
                        _statusService?.ShowSuccess($"Скопировано {messagesOfType.Count} {typeName} в буфер обмена", "StatusBar");
                    }
                    else
                    {
                        _statusService?.ShowInfo($"Нет сообщений выбранного типа для копирования", "StatusBar");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования сообщений: {ex.Message}", "StatusBar");
        }
    }

    private void OnStatusChanged(StatusMessage message)
    {
        CurrentMessage = message;
        IsStatusMessageVisible = message != null;
        UpdateCounts();
    }

    private Unit ShowNotificationAsync()
    {
        Task.Run(async () =>
        {
            await Task.Delay(100);
            // Notification logic here
        });
        return Unit.Default;
    }

    private Unit ClearNotificationAsync()
    {
        Task.Run(async () =>
        {
            await Task.Delay(50);
            // Clear logic here
        });
        return Unit.Default;
    }

    private Unit ShowProgressAsync()
    {
        Task.Run(async () =>
        {
            await Task.Delay(50);
            // Progress logic here
        });
        return Unit.Default;
    }

    private Unit HideProgressAsync()
    {
        Task.Run(async () =>
        {
            await Task.Delay(50);
            // Hide logic here
        });
        return Unit.Default;
    }
} 

