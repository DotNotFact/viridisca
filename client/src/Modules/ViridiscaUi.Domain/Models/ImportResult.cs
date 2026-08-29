namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Результат операции импорта (обычная модель, не сущность БД)
/// </summary>
/// <typeparam name="T">Тип импортируемых объектов</typeparam>
public class ImportResult<T>
{
    /// <summary>
    /// Количество успешно импортированных элементов
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество элементов с ошибками
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Общее количество обработанных элементов
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;

    /// <summary>
    /// Количество импортированных элементов (для совместимости)
    /// </summary>
    public int ImportedCount => SuccessCount;

    /// <summary>
    /// Успешно импортированные элементы
    /// </summary>
    public IList<T> ImportedItems { get; set; } = [];

    /// <summary>
    /// Список ошибок
    /// </summary>
    public IList<string> Errors { get; set; } = [];

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public IList<string> Warnings { get; set; } = [];

    /// <summary>
    /// Успешность операции импорта
    /// </summary>
    public bool IsSuccess => FailureCount == 0 && Errors.Count == 0;

    /// <summary>
    /// Процент успешности
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

    /// <summary>
    /// Сводка результата импорта
    /// </summary>
    public string Summary => $"Импортировано: {SuccessCount}, Ошибок: {FailureCount}, Всего: {TotalCount}";

    /// <summary>
    /// Создает успешный результат импорта
    /// </summary>
    public static ImportResult<T> Success(int count, IList<T>? items = null)
    {
        return new ImportResult<T>
        {
            SuccessCount = count,
            FailureCount = 0,
            ImportedItems = items ?? []
        };
    }

    /// <summary>
    /// Создает результат импорта с ошибками
    /// </summary>
    public static ImportResult<T> Failure(IList<string> errors)
    {
        return new ImportResult<T>
        {
            SuccessCount = 0,
            FailureCount = errors.Count,
            Errors = errors
        };
    }

    /// <summary>
    /// Создает смешанный результат импорта
    /// </summary>
    public static ImportResult<T> Mixed(int successCount, int failureCount, IList<T> items, IList<string> errors)
    {
        return new ImportResult<T>
        {
            SuccessCount = successCount,
            FailureCount = failureCount,
            ImportedItems = items,
            Errors = errors
        };
    }
}

/// <summary>
/// Не-дженерик версия ImportResult для совместимости
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Количество успешно импортированных элементов
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество элементов с ошибками
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Общее количество обработанных элементов
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;

    /// <summary>
    /// Количество импортированных элементов (для совместимости)
    /// </summary>
    public int ImportedCount => SuccessCount;

    /// <summary>
    /// Список ошибок
    /// </summary>
    public IList<string> Errors { get; set; } = [];

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public IList<string> Warnings { get; set; } = [];

    /// <summary>
    /// Успешность операции импорта
    /// </summary>
    public bool IsSuccess => FailureCount == 0 && Errors.Count == 0;

    /// <summary>
    /// Процент успешности
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

    /// <summary>
    /// Сводка результата импорта
    /// </summary>
    public string Summary => $"Импортировано: {SuccessCount}, Ошибок: {FailureCount}, Всего: {TotalCount}";

    /// <summary>
    /// Создает успешный результат импорта
    /// </summary>
    public static ImportResult Success(int count)
    {
        return new ImportResult
        {
            SuccessCount = count,
            FailureCount = 0
        };
    }

    /// <summary>
    /// Создает результат импорта с ошибками
    /// </summary>
    public static ImportResult Failure(IList<string> errors)
    {
        return new ImportResult
        {
            SuccessCount = 0,
            FailureCount = errors.Count,
            Errors = errors
        };
    }
} 