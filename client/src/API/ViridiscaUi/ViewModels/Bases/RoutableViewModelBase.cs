using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Domain.Services;
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.ViewModels.Bases;

/// <summary>
/// Минималистичный базовый класс для ViewModel с поддержкой маршрутизации
/// Навигация полностью обрабатывается через атрибут Route и UnifiedNavigationService
/// </summary>
public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel, IActivatableViewModel
{
    private bool _isFirstTimeLoaded = true;
    private bool _isActivated = false;

    /// <summary>
    /// Логгер для записи событий
    /// </summary>
    protected readonly ILogger? _logger;

    /// <summary>
    /// Сервис диалогов для отображения диалоговых окон
    /// </summary>
    protected readonly IDialogService? _dialogService;

    /// <summary>
    /// Активатор для ReactiveUI
    /// </summary>
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    /// <summary>
    /// Уникальный идентификатор маршрута ViewModel
    /// Получается автоматически из атрибута Route
    /// </summary>
    public string UrlPathSegment { get; }
    
    /// <summary>
    /// Хост-экран для навигации
    /// </summary>
    public IScreen? HostScreen { get; }

    /// <summary>
    /// Команда для навигации назад
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; private set; } = null!;

    /// <summary>
    /// Создает новый экземпляр маршрутизируемой ViewModel
    /// </summary>
    /// <param name="hostScreen">IScreen хост (может быть null для диалогов)</param>
    protected RoutableViewModelBase(IScreen? hostScreen)
    {
        HostScreen = hostScreen;
        
        // Автоматически получаем UrlPathSegment из атрибута Route
        var routeAttribute = GetType().GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;
        
        UrlPathSegment = routeAttribute?.Path ?? GetType().Name.ToLowerInvariant();

        // Настройка активации/деактивации
        this.WhenActivated(disposables =>
        {
            OnActivated();
            _isActivated = true;

            if (_isFirstTimeLoaded)
            {
                OnFirstTimeLoaded();
                _isFirstTimeLoaded = false;
            }

            Disposable.Create(() =>
            {
                OnDeactivated();
                _isActivated = false;
            }).DisposeWith(disposables);
        });
        
        InitializeCommands();
        
        StatusLogger.LogDebug($"Created routable ViewModel: {UrlPathSegment}", GetType().Name);
    }

    /// <summary>
    /// Создает новый экземпляр маршрутизируемой ViewModel с логгером и сервисом диалогов
    /// </summary>
    /// <param name="hostScreen">IScreen хост (может быть null для диалогов)</param>
    /// <param name="logger">Логгер</param>
    /// <param name="dialogService">Сервис диалогов</param>
    protected RoutableViewModelBase(IScreen? hostScreen, ILogger? logger, IDialogService? dialogService) : this(hostScreen)
    {
        _logger = logger;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Команда "Назад" доступна только если можно вернуться назад и есть HostScreen
        var canGoBack = this.WhenAnyValue(x => x.HostScreen)
            .Select(screen => screen?.Router.NavigationStack.Count > 1);

        GoBackCommand = ReactiveCommand.CreateFromTask(
            GoBackAsync, 
            canGoBack,
            RxApp.MainThreadScheduler);

        GoBackCommand.ThrownExceptions
            .Subscribe(ex => SetError("Ошибка навигации назад", ex))
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Вызывается при первой активации ViewModel
    /// </summary>
    protected virtual void OnFirstTimeLoaded()
    {
        StatusLogger.LogDebug($"First time loaded: {UrlPathSegment}", GetType().Name);

        // Call async version if overridden.
        // Not Task.Run: that forces the whole continuation chain onto a thread-pool thread
        // with no UI affinity, so any override that touches a plain ObservableCollection
        // directly (most of them do - CourseInstances.Clear()/.Add(), etc.) throws
        // "Call from invalid thread" the moment the collection change tries to notify a
        // bound control. Firing the async method directly from the UI thread instead means
        // it runs synchronously up to its first await, and Avalonia's UI-thread
        // SynchronizationContext resumes every later continuation back on the UI thread -
        // the normal, safe pattern for fire-and-forget async from sync code.
        _ = InvokeFirstTimeLoadedAsync();
    }

    private async Task InvokeFirstTimeLoadedAsync()
    {
        try
        {
            await OnFirstTimeLoadedAsync();
        }
        catch (Exception ex)
        {
            SetError("Ошибка при первой загрузке", ex);
        }
    }

    /// <summary>
    /// Асинхронная версия OnFirstTimeLoaded для переопределения в наследниках
    /// </summary>
    protected virtual async Task OnFirstTimeLoadedAsync()
    {
        await Task.CompletedTask;
        StatusLogger.LogDebug($"First time loaded async: {UrlPathSegment}", GetType().Name);
    }

    /// <summary>
    /// Вызывается при активации ViewModel
    /// </summary>
    protected virtual void OnActivated()
    {
        StatusLogger.LogDebug($"Activated: {UrlPathSegment}", GetType().Name);
    }

    /// <summary>
    /// Вызывается при деактивации ViewModel
    /// </summary>
    protected virtual void OnDeactivated()
    {
        StatusLogger.LogDebug($"Deactivated: {UrlPathSegment}", GetType().Name);
    }

    /// <summary>
    /// Проверяет, активирована ли ViewModel
    /// </summary>
    public bool IsActivated => _isActivated;

    /// <summary>
    /// Возврат назад по стеку навигации
    /// </summary>
    private async Task GoBackAsync()
    {
        if (HostScreen?.Router.NavigationStack.Count <= 1)
        {
            ShowWarning("Невозможно вернуться назад - это первая страница");
            return;
        }

        if (HostScreen == null)
        {
            ShowWarning("Невозможно вернуться назад - отсутствует хост-экран");
            return;
        }

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            await HostScreen.Router.NavigateBack.Execute();
            StatusLogger.LogInfo($"Navigated back from {UrlPathSegment}", GetType().Name);
        }, "Ошибка при возврате назад");
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    public override void Dispose()
    {
        OnDeactivated();
        base.Dispose();
        StatusLogger.LogDebug($"Disposed routable ViewModel: {UrlPathSegment}", GetType().Name);
    }

    /// <summary>
    /// Устанавливает состояние загрузки
    /// </summary>
    protected new virtual void SetLoading(bool isLoading, string? message = null)
    {
        IsBusy = isLoading;
        if (!string.IsNullOrEmpty(message))
        {
            LogInfo($"Loading state changed to: {isLoading}, Message: {message}");
        }
        else
        {
            LogInfo($"Loading state changed to: {isLoading}");
        }
    }

    /// <summary>
    /// Навигация к указанному пути (заглушка)
    /// </summary>
    protected virtual async Task NavigateToAsync(string path)
    {
        try
        {
            LogInfo($"Navigation requested to: {path}");
            // Заглушка - в реальной реализации здесь будет навигация
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Navigation failed to path: {path}");
        }
    }

    /// <summary>
    /// Навигация к ViewModel указанного типа (заглушка)
    /// </summary>
    protected virtual async Task NavigateToAsync<T>() where T : class, IRoutableViewModel
    {
        try
        {
            var typeName = typeof(T).Name;
            LogInfo($"Navigation requested to ViewModel: {typeName}");
            // Заглушка - в реальной реализации здесь будет навигация
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Navigation failed to ViewModel: {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Показывает диалог подтверждения (заглушка)
    /// </summary>
    protected virtual async Task<bool> ShowConfirmationAsync(string message, string title = "Подтверждение")
    {
        try
        {
            LogInfo($"Confirmation dialog requested: {title} - {message}");
            // Заглушка - в реальной реализации здесь будет диалог подтверждения
            await Task.CompletedTask;
            return true; // По умолчанию возвращаем true для заглушки
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to show confirmation dialog: {title}");
            return false;
        }
    }

    /// <summary>
    /// Показывает диалог ввода текста (заглушка)
    /// </summary>
    protected virtual async Task<string?> ShowInputAsync(string title, string message = "", string defaultValue = "")
    {
        try
        {
            LogInfo($"Input dialog requested: {title} - {message}");
            // Заглушка - в реальной реализации здесь будет диалог ввода
            await Task.CompletedTask;
            return defaultValue; // По умолчанию возвращаем значение по умолчанию
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to show input dialog: {title}");
            return null;
        }
    }
} 

