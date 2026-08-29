using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Entities.System;

namespace ViridiscaUi.Domain.Services.Statistic;

/// <summary>
/// Сервис для управления статус-сообщениями и интеграции с логированием
/// </summary>
public interface IStatusService
{
    /// <summary>
    /// Коллекция статус-сообщений для отображения в UI
    /// </summary>
    ReadOnlyObservableCollection<StatusMessage> Messages { get; }

    /// <summary>
    /// Текущее статус-сообщение
    /// </summary>
    StatusMessage? CurrentMessage { get; }

    /// <summary>
    /// Общее количество сообщений
    /// </summary>
    int TotalMessagesCount { get; }

    /// <summary>
    /// Количество сообщений ошибок
    /// </summary>
    int ErrorsCount { get; }

    /// <summary>
    /// Количество предупреждений
    /// </summary>
    int WarningsCount { get; }

    /// <summary>
    /// Количество информационных сообщений
    /// </summary>
    int InfoCount { get; }

    /// <summary>
    /// Максимальное количество сообщений в истории
    /// </summary>
    int MaxMessagesCount { get; set; }

    /// <summary>
    /// Показать информационное сообщение
    /// </summary>
    void ShowInfo(string message, string? source = null);

    /// <summary>
    /// Показать предупреждение
    /// </summary>
    void ShowWarning(string message, string? source = null);

    /// <summary>
    /// Показать ошибку
    /// </summary>
    void ShowError(string message, string? source = null);

    /// <summary>
    /// Показать успешное сообщение
    /// </summary>
    void ShowSuccess(string message, string? source = null);

    /// <summary>
    /// Добавить сообщение с определенным уровнем логирования
    /// </summary>
    void AddMessage(LogLevel level, string message, string? source = null);

    /// <summary>
    /// Очистить все сообщения
    /// </summary>
    void Clear();

    /// <summary>
    /// Событие изменения текущего сообщения
    /// </summary>
    event EventHandler<StatusMessage?> CurrentMessageChanged;

    /// <summary>
    /// Событие добавления нового сообщения
    /// </summary>
    event EventHandler<StatusMessage> MessageAdded;
}