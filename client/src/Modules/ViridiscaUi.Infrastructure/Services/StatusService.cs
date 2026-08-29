using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Services.Statistic;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Сервис для управления статус-сообщениями и интеграции с логированием
/// </summary>
public class StatusService : ReactiveObject, IStatusService
{
    private readonly ObservableCollection<StatusMessage> _messages = [];
    private readonly ILogger<StatusService> _logger;

    private readonly Timer _messageTimer;

    private StatusMessage? _currentMessage;
    private int _maxMessagesCount = 500;

    public ReadOnlyObservableCollection<StatusMessage> Messages { get; }

    public StatusMessage? CurrentMessage
    {
        get => _currentMessage;
        private set
        {
            if (_currentMessage != value)
            {
                this.RaiseAndSetIfChanged(ref _currentMessage, value);
                CurrentMessageChanged?.Invoke(this, value);
            }
        }
    }

    public int TotalMessagesCount => _messages.Count;

    public int ErrorsCount => _messages.Count(m => m.Level == LogLevel.Error || m.Level == LogLevel.Critical);

    public int WarningsCount => _messages.Count(m => m.Level == LogLevel.Warning);

    public int InfoCount => _messages.Count(m => m.Level == LogLevel.Information);

    public int MaxMessagesCount
    {
        get => _maxMessagesCount;
        set => this.RaiseAndSetIfChanged(ref _maxMessagesCount, Math.Max(1, value));
    }

    public event EventHandler<StatusMessage?>? CurrentMessageChanged;
    public event EventHandler<StatusMessage>? MessageAdded;

    public StatusService(ILogger<StatusService> logger)
    {
        _logger = logger;
        Messages = new ReadOnlyObservableCollection<StatusMessage>(_messages);

        // Таймер для автоматического скрытия статус-сообщений через определенное время
        _messageTimer = new Timer(OnMessageTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

        // Подписка на изменения в коллекции сообщений для обновления счетчиков
        _messages.CollectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(TotalMessagesCount));
            this.RaisePropertyChanged(nameof(ErrorsCount));
            this.RaisePropertyChanged(nameof(WarningsCount));
            this.RaisePropertyChanged(nameof(InfoCount));
        };
    }

    public void ShowInfo(string message, string? source = null)
    {
        AddMessage(LogLevel.Information, message, source);
    }

    public void ShowWarning(string message, string? source = null)
    {
        AddMessage(LogLevel.Warning, message, source);
    }

    public void ShowError(string message, string? source = null)
    {
        AddMessage(LogLevel.Error, message, source);
    }

    public void ShowSuccess(string message, string? source = null)
    {
        // Используем Information уровень для успешных сообщений, но добавляем категорию
        var statusMessage = new StatusMessage
        {
            Level = LogLevel.Information,
            Message = message,
            Source = source,
            Category = "Success"
        };

        AddMessageInternal(statusMessage);
    }

    public void AddMessage(LogLevel level, string message, string? source = null)
    {
        var statusMessage = new StatusMessage
        {
            Level = level,
            Message = message,
            Source = source
        };

        AddMessageInternal(statusMessage);
    }

    private void AddMessageInternal(StatusMessage statusMessage)
    {
        try
        {
            // Логируем сообщение в стандартный логгер
            _logger.Log(statusMessage.Level, "{Source}: {Message}", statusMessage.Source ?? "StatusService", statusMessage.Message);

            // Добавляем в коллекцию
            AddToCollection(statusMessage);
            SetCurrentMessage(statusMessage);
            MessageAdded?.Invoke(this, statusMessage);
        }
        catch (Exception ex)
        {
            // Безопасное логирование ошибки добавления сообщения
            _logger.LogError(ex, "Ошибка при добавлении статус-сообщения: {Message}", statusMessage.Message);
        }
    }

    private readonly Queue<StatusMessage> _pendingMessages = new();
    private readonly object _queueLock = new();
    private bool _isProcessingQueue;

    private void AddToCollection(StatusMessage statusMessage)
    {
        // A CollectionChanged handler (this class's own counter refresh, or a UI-bound
        // consumer) can synchronously trigger another AddMessage call while _messages.Add()
        // is still dispatching its event, which ObservableCollection rejects with
        // "Cannot change ObservableCollection during a CollectionChanged event." Queueing and
        // draining here instead of adding directly means a reentrant call just enqueues and
        // returns; the outer call's loop picks it up on its next iteration, once the previous
        // Add's event dispatch has already finished.
        //
        // ViewModels also call ShowError/LogError from catch blocks that can resume on a
        // thread-pool thread, so this is genuinely multi-threaded, not just reentrant on one
        // thread - Queue<T> is not thread-safe, and two threads both seeing Count > 0 and
        // racing on Dequeue() threw "Queue empty". Lock guards every access to the queue and
        // the processing flag; the ObservableCollection mutations themselves stay inside the
        // lock too since only one thread should ever be draining at a time.
        lock (_queueLock)
        {
            _pendingMessages.Enqueue(statusMessage);

            if (_isProcessingQueue)
            {
                return;
            }

            _isProcessingQueue = true;
        }

        try
        {
            while (true)
            {
                StatusMessage next;
                lock (_queueLock)
                {
                    if (_pendingMessages.Count == 0)
                    {
                        break;
                    }
                    next = _pendingMessages.Dequeue();
                }

                _messages.Add(next);

                while (_messages.Count > MaxMessagesCount)
                {
                    _messages.RemoveAt(0);
                }
            }
        }
        finally
        {
            lock (_queueLock)
            {
                _isProcessingQueue = false;
            }
        }
    }

    private void SetCurrentMessage(StatusMessage statusMessage)
    {
        CurrentMessage = statusMessage;

        // Устанавливаем таймер для автоматического скрытия сообщения
        var delay = GetMessageDisplayDelay(statusMessage.Level);
        if (delay > TimeSpan.Zero)
        {
            _messageTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    private TimeSpan GetMessageDisplayDelay(LogLevel level)
    {
        return level switch
        {
            LogLevel.Error or LogLevel.Critical => TimeSpan.FromSeconds(10), // Ошибки показываем дольше
            LogLevel.Warning => TimeSpan.FromSeconds(7),
            LogLevel.Information => TimeSpan.FromSeconds(5),
            LogLevel.Debug => TimeSpan.FromSeconds(3),
            LogLevel.Trace => TimeSpan.FromSeconds(2),
            _ => TimeSpan.FromSeconds(5)
        };
    }

    private void OnMessageTimerElapsed(object? state)
    {
        CurrentMessage = null;
    }

    public void Clear()
    {
        try
        {
            _logger.LogInformation("Очистка всех статус-сообщений");

            _messages.Clear();
            CurrentMessage = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке статус-сообщений");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageTimer?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}