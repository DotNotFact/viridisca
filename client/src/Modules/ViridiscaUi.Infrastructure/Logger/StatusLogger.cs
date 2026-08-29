using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Domain.Services.Statistic;
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.Logger;

/// <summary>
/// Статический класс для логирования сообщений в StatusService
/// Заменяет Console.WriteLine для централизованного отображения сообщений в UI
/// 
/// Примечание: Это удобный фасад для IStatusService. Можно использовать как:
/// 1. StatusLogger.LogInfo() - статический метод (требует инициализации)
/// 2. IStatusService напрямую через DI - для сложных сценариев
/// </summary>
public static class StatusLogger
{
    private static IStatusService _statusService = null!;

    /// <summary>
    /// Инициализация StatusLogger с StatusService
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _statusService = serviceProvider.GetRequiredService<IStatusService>()
            ?? throw new NullReferenceException("IStatusService is null");
    }

    /// <summary>
    /// Логирует информационное сообщение
    /// </summary>
    public static void LogInfo(string message, string? source = null)
    {
        _statusService.ShowInfo(message, source);
    }

    /// <summary>
    /// Логирует предупреждение
    /// </summary>
    public static void LogWarning(string message, string? source = null)
    {
        _statusService.ShowWarning(message, source);
    }

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    public static void LogError(string message, string? source = null)
    {
        _statusService.ShowError(message, source);
    }

    /// <summary>
    /// Логирует успешное сообщение
    /// </summary>
    public static void LogSuccess(string message, string? source = null)
    {
        _statusService.ShowSuccess(message, source);
    }

    /// <summary>
    /// Логирует отладочное сообщение
    /// </summary>
    public static void LogDebug(string message, string? source = null)
    {
        _statusService.AddMessage(LogLevel.Debug, message, source);
    }

    /// <summary>
    /// Универсальный метод логирования с указанием уровня
    /// </summary>
    public static void Log(LogLevel level, string message, string? source = null)
    {
        _statusService.AddMessage(level, message, source);
    }
}