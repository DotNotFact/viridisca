namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Информация о связанных данных персоны для безопасного удаления
/// </summary>
public class PersonRelatedDataInfo
{
    /// <summary>
    /// Количество связанных аккаунтов
    /// </summary>
    public int AccountsCount { get; set; }

    /// <summary>
    /// Количество ролей персоны
    /// </summary>
    public int RolesCount { get; set; }

    /// <summary>
    /// Количество записей студента (если персона является студентом)
    /// </summary>
    public int StudentsCount { get; set; }

    /// <summary>
    /// Количество записей преподавателя (если персона является преподавателем)
    /// </summary>
    public int TeachersCount { get; set; }

    /// <summary>
    /// Количество уведомлений
    /// </summary>
    public int NotificationsCount { get; set; }

    /// <summary>
    /// Количество файловых записей
    /// </summary>
    public int FileRecordsCount { get; set; }

    /// <summary>
    /// Есть ли связанные данные
    /// </summary>
    public bool HasRelatedData => 
        AccountsCount > 0 || 
        RolesCount > 0 || 
        StudentsCount > 0 || 
        TeachersCount > 0 ||
        NotificationsCount > 0 ||
        FileRecordsCount > 0;

    /// <summary>
    /// Можно ли удалить персону
    /// </summary>
    public bool CanDelete => !HasRelatedData;

    /// <summary>
    /// Причина блокировки удаления
    /// </summary>
    public string? DeletionBlockedReason
    {
        get
        {
            if (!HasRelatedData) return null;

            var reasons = new List<string>();
            
            if (AccountsCount > 0)
                reasons.Add($"аккаунтов: {AccountsCount}");
            
            if (RolesCount > 0)
                reasons.Add($"ролей: {RolesCount}");
            
            if (StudentsCount > 0)
                reasons.Add($"записей студента: {StudentsCount}");
            
            if (TeachersCount > 0)
                reasons.Add($"записей преподавателя: {TeachersCount}");

            if (NotificationsCount > 0)
                reasons.Add($"уведомлений: {NotificationsCount}");

            if (FileRecordsCount > 0)
                reasons.Add($"файловых записей: {FileRecordsCount}");

            return $"Удаление заблокировано из-за связанных данных: {string.Join(", ", reasons)}";
        }
    }
} 