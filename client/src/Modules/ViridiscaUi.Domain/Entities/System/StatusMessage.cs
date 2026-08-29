using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Сообщение статуса системы
/// </summary>
public class StatusMessage : AuditableEntity
{
    /// <summary>
    /// Уникальный идентификатор сообщения
    /// </summary>
    public new Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Временная метка
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>
    /// Уровень логирования
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Источник сообщения
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Категория сообщения
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Тип сообщения на основе уровня
    /// </summary>
    public StatusMessageType Type => Level switch
    {
        LogLevel.Critical or LogLevel.Error => StatusMessageType.Error,
        LogLevel.Warning => StatusMessageType.Warning,
        LogLevel.Information => StatusMessageType.Info,
        LogLevel.Debug => StatusMessageType.Debug,
        LogLevel.Trace => StatusMessageType.Trace,
        _ => StatusMessageType.Info
    };

    /// <summary>
    /// Иконка для отображения
    /// </summary>
    public string Icon => Level switch
    {
        LogLevel.Critical or LogLevel.Error => "Alert",
        LogLevel.Warning => "AlertTriangle",
        LogLevel.Information => "Information",
        LogLevel.Debug => "Bug",
        LogLevel.Trace => "Magnify",
        _ => "Information"
    };

    /// <summary>
    /// Цвет для отображения
    /// </summary>
    public string Color => Level switch
    {
        LogLevel.Critical or LogLevel.Error => "Red",
        LogLevel.Warning => "Orange",
        LogLevel.Information => "Blue",
        LogLevel.Debug => "Purple",
        LogLevel.Trace => "Gray",
        _ => "Blue"
    };

    /// <summary>
    /// Форматированное сообщение
    /// </summary>
    public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] {(string.IsNullOrEmpty(Source) ? "" : $"[{Source}] ")}{Message}";

    /// <summary>
    /// Текст для копирования
    /// </summary>
    public string CopyableText => $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {(string.IsNullOrEmpty(Source) ? "" : $"[{Source}] ")}{Message}";
} 