namespace ViridiscaUi.Domain.Entities.System.Enums;

/// <summary>
/// Кнопки диалогового окна
/// </summary>
public enum DialogButtons
{
    /// <summary>
    /// Только OK
    /// </summary>
    OK = 0,

    /// <summary>
    /// OK и Cancel
    /// </summary>
    OKCancel = 1,

    /// <summary>
    /// Да и Нет
    /// </summary>
    YesNo = 2,

    /// <summary>
    /// Да, Нет и Отмена
    /// </summary>
    YesNoCancel = 3,

    /// <summary>
    /// Повторить и Отмена
    /// </summary>
    RetryCancel = 4,

    /// <summary>
    /// Прервать, Повторить и Игнорировать
    /// </summary>
    AbortRetryIgnore = 5
}

/// <summary>
/// Результат диалогового окна
/// </summary>
public enum DialogResult
{
    /// <summary>
    /// Не определено
    /// </summary>
    None = 0,

    /// <summary>
    /// OK
    /// </summary>
    OK = 1,

    /// <summary>
    /// Отмена
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// Да
    /// </summary>
    Yes = 3,

    /// <summary>
    /// Нет
    /// </summary>
    No = 4,

    /// <summary>
    /// Повторить
    /// </summary>
    Retry = 5,

    /// <summary>
    /// Игнорировать
    /// </summary>
    Ignore = 6,

    /// <summary>
    /// Прервать
    /// </summary>
    Abort = 7
}