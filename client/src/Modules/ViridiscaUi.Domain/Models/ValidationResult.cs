namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Результат валидации (обычная модель, не сущность БД)
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Успешна ли валидация
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Список ошибок валидации
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Дополнительные данные
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>
    /// Создает успешный результат валидации
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Создает неуспешный результат валидации с ошибками
    /// </summary>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Создает неуспешный результат валидации с ошибками
    /// </summary>
    public static ValidationResult Failure(IEnumerable<string> errors) => new()
    {
        IsValid = false,
        Errors = [.. errors]
    };

    /// <summary>
    /// Добавляет ошибку
    /// </summary>
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// Добавляет ошибку с указанием поля (для совместимости с ViewModels)
    /// </summary>
    public void AddError(string field, string error)
    {
        Errors.Add($"{field}: {error}");
        IsValid = false;
    }

    /// <summary>
    /// Добавляет предупреждение
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    /// <summary>
    /// Объединяет результаты валидации
    /// </summary>
    public void Merge(ValidationResult other)
    {
        if (other == null) return;

        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
        
        foreach (var kvp in other.Data)
        {
            Data[kvp.Key] = kvp.Value;
        }

        if (!other.IsValid)
            IsValid = false;
    }
} 