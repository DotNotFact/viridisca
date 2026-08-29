namespace ViridiscaUi.Models.DataInfo;

/// <summary>
/// Базовый класс для информации о связанных данных
/// </summary>
public class RelatedDataInfo
{
    /// <summary>
    /// Словарь с количеством связанных данных по типам
    /// </summary>
    public Dictionary<string, int> RelatedDataCounts { get; set; } = new();

    /// <summary>
    /// Есть ли связанные данные
    /// </summary>
    public virtual bool HasRelatedData => false;

    /// <summary>
    /// Общее количество связанных записей
    /// </summary>
    public virtual int TotalRelatedCount => 0;

    /// <summary>
    /// Добавляет информацию о связанных данных
    /// </summary>
    public void AddRelatedData(string typeName, int count)
    {
        RelatedDataCounts[typeName] = count;
    }

    /// <summary>
    /// Получает количество связанных данных по типу
    /// </summary>
    public int GetCount(string typeName)
    {
        return RelatedDataCounts.TryGetValue(typeName, out var count) ? count : 0;
    }

    /// <summary>
    /// Сообщение о связанных данных
    /// </summary>
    public virtual string RelatedDataMessage => 
        HasRelatedData 
            ? $"Найдено {TotalRelatedCount} связанных записей. Удаление может повлиять на целостность данных."
            : "Связанных данных не найдено. Удаление безопасно.";

    /// <summary>
    /// Краткое сообщение о связанных данных
    /// </summary>
    public string GetShortMessage()
    {
        if (!HasRelatedData)
            return "Нет связанных данных";

        return $"Связанных записей: {TotalRelatedCount}";
    }
} 