using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Domain.Services;
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.ViewModels.Bases;

/// <summary>
/// Базовый класс для всех ViewModels с общей функциональностью
/// </summary>
public abstract class ViewModelBase : ReactiveObject, IDisposable
{
    protected readonly CompositeDisposable Disposables = [];

    /// <summary>
    /// Логгер для записи событий (может быть null)
    /// </summary>
    protected readonly ILogger? _logger;

    /// <summary>
    /// Сервис диалогов для отображения диалоговых окон (может быть null)
    /// </summary>
    protected readonly IDialogService? _dialogService;

    [Reactive] public bool IsBusy { get; protected set; }
    [Reactive] public string? ErrorMessage { get; protected set; }
    [Reactive] public bool HasError { get; protected set; }
    [Reactive] public string? ValidationError { get; protected set; }
    [Reactive] public string Title { get; protected set; } = string.Empty;
    [Reactive] public bool IsEditMode { get; protected set; }

    /// <summary>
    /// Логгер для записи событий
    /// </summary>
    public ILogger? Logger => _logger;

    protected ViewModelBase()
    {
        // Автоматическое обновление HasError при изменении ErrorMessage
        this.WhenAnyValue(x => x.ErrorMessage)
            .Subscribe(error => HasError = !string.IsNullOrEmpty(error))
            .DisposeWith(Disposables);

        // Подписываемся на ThrownExceptions для предотвращения разрыва observable pipeline
        ThrownExceptions
            .Subscribe(ex =>
            {
                SetError("Произошла ошибка в реактивной команде", ex);
                StatusLogger.LogError(ex.Message + ":\n Unhandled exception in reactive pipeline for " + GetType().Name);
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Конструктор с логгером и сервисом диалогов
    /// </summary>
    protected ViewModelBase(ILogger? logger, IDialogService? dialogService) : this()
    {
        _logger = logger;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Логирует информационное сообщение
    /// </summary>
    protected void LogInfo(string message) => StatusLogger.LogInfo(message, GetType().Name);

    /// <summary>
    /// Логирует предупреждение
    /// </summary>
    protected void LogWarning(string message) => StatusLogger.LogWarning(message, GetType().Name);

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    protected void LogError(string message) => StatusLogger.LogError(message, GetType().Name);

    /// <summary>
    /// Логирует ошибку с исключением
    /// </summary>
    protected void LogError(Exception exception, string message) => StatusLogger.LogError($"{message}: {exception.Message}", GetType().Name);

    /// <summary>
    /// Логирует успешное сообщение
    /// </summary>
    protected void LogSuccess(string message) => StatusLogger.LogSuccess(message, GetType().Name);

    /// <summary>
    /// Логирует отладочное сообщение
    /// </summary>
    protected void LogDebug(string message) => StatusLogger.LogDebug(message, GetType().Name);

    /// <summary>
    /// Очищает ошибку
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = null;
        StatusLogger.LogDebug($"Error cleared in {GetType().Name}");
    }

    /// <summary>
    /// Очищает все ошибки
    /// </summary>
    protected void ClearErrors()
    {
        ClearError();
        ClearValidationError();
    }

    /// <summary>
    /// Устанавливает ошибку
    /// </summary>
    protected void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;
        StatusLogger.LogError($"Error in {GetType().Name}: {message}");
    }

    /// <summary>
    /// Очищает ошибку валидации
    /// </summary>
    protected void ClearValidationError()
    {
        ValidationError = null;
    }

    /// <summary>
    /// Устанавливает ошибку валидации
    /// </summary>
    protected void SetValidationError(string message)
    {
        ValidationError = message;
        StatusLogger.LogWarning($"Validation error in {GetType().Name}: {message}");
    }

    /// <summary>
    /// Выполняет валидацию (переопределяется в наследниках)
    /// </summary>
    protected virtual bool Validate()
    {
        ClearValidationError();
        return true;
    }

    /// <summary>
    /// Устанавливает состояние загрузки
    /// </summary>
    protected void SetLoading(bool isLoading, string? message = null)
    {
        IsBusy = isLoading;

        if (isLoading && !string.IsNullOrEmpty(message))
        {
            StatusLogger.LogDebug($"Loading started in {GetType().Name}: {message}");
        }
        else if (!isLoading)
        {
            StatusLogger.LogDebug($"Loading finished in {GetType().Name}");
        }
    }

    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    protected void ShowInfo(string message)
    {
        StatusLogger.LogInfo(message, GetType().Name);
    }

    /// <summary>
    /// Показывает сообщение об успехе
    /// </summary>
    protected void ShowSuccess(string message)
    {
        StatusLogger.LogSuccess(message, GetType().Name);
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    protected void ShowWarning(string message)
    {
        StatusLogger.LogWarning(message, GetType().Name);
    }

    /// <summary>
    /// Показывает ошибку
    /// </summary>
    protected void ShowError(string message)
    {
        StatusLogger.LogError(message, GetType().Name);
    }

    /// <summary>
    /// Выполняет асинхронную операцию с обработкой ошибок
    /// </summary>
    protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            await operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            StatusLogger.LogError($"Error executing operation in {GetType().Name}: {message}");
        }
    }

    /// <summary>
    /// Выполняет асинхронную операцию с обработкой ошибок и возвращает результат
    /// </summary>
    protected async Task<T?> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            return await operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            StatusLogger.LogError($"Error executing operation in {GetType().Name}: {message}");
            return default;
        }
    }

    /// <summary>
    /// Выполняет синхронную операцию с обработкой ошибок
    /// </summary>
    protected void ExecuteWithErrorHandling(Action operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            StatusLogger.LogError($"Error executing operation in {GetType().Name}: {message}");
        }
    }

    /// <summary>
    /// Выполняет синхронную операцию с обработкой ошибок и возвращает результат
    /// </summary>
    protected T? ExecuteWithErrorHandling<T>(Func<T> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            return operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            StatusLogger.LogError($"Error executing operation in {GetType().Name}: {message}");
            return default;
        }
    }

    #region Command Creation Helpers

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций без параметров
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateCommand(
        Func<Task> execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask(
            async () => await ExecuteWithErrorHandlingAsync(execute, errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с параметром
    /// </summary>
    protected ReactiveCommand<T, Unit> CreateCommand<T>(
        Func<T, Task> execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask<T>(
            async param => await ExecuteWithErrorHandlingAsync(() => execute(param), errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для синхронных операций без параметров
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateSyncCommand(
        Action execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.Create(
            () => ExecuteWithErrorHandling(execute, errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Sync command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для синхронных операций с параметром
    /// </summary>
    protected ReactiveCommand<T, Unit> CreateSyncCommand<T>(
        Action<T> execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.Create<T>(
            param => ExecuteWithErrorHandling(() => execute(param), errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Sync command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с возвращаемым значением
    /// </summary>
    protected ReactiveCommand<Unit, TResult> CreateCommand<TResult>(
        Func<Task<TResult>> execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask(
            async () => await ExecuteWithErrorHandlingAsync(execute, errorMessage) ?? default!,
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с параметром и возвращаемым значением
    /// </summary>
    protected ReactiveCommand<TParam, TResult> CreateCommand<TParam, TResult>(
        Func<TParam, Task<TResult>> execute,
        IObservable<bool>? canExecute = null,
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask<TParam, TResult>(
            async param => await ExecuteWithErrorHandlingAsync(() => execute(param), errorMessage) ?? default!,
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex =>
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                StatusLogger.LogError($"Command execution error in {GetType().Name}: {errorMessage}");
            })
            .DisposeWith(Disposables);

        return command;
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Disposables?.Dispose();
                StatusLogger.LogDebug($"sposed {GetType().Name}");
            }
            _disposed = true;
        }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

