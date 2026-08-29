namespace ViridiscaUi.Models.DataInfo;

/// <summary>
/// Информация о связанных данных для предмета
/// </summary>
public class SubjectRelatedDataInfo : RelatedDataInfo
{
    /// <summary>
    /// Количество связанных курсов
    /// </summary>
    public int CoursesCount { get; set; }

    /// <summary>
    /// Количество связанных учебных планов
    /// </summary>
    public int CurriculumsCount { get; set; }

    /// <summary>
    /// Количество связанных заданий
    /// </summary>
    public int AssignmentsCount { get; set; }

    /// <summary>
    /// Количество связанных экземпляров курсов
    /// </summary>
    public int CourseInstancesCount { get; set; }

    /// <summary>
    /// Список описаний связанных данных
    /// </summary>
    public List<string> RelatedDataDescriptions { get; set; } = new();

    /// <summary>
    /// Есть ли связанные данные
    /// </summary>
    public override bool HasRelatedData => 
        CoursesCount > 0 || 
        CurriculumsCount > 0 || 
        AssignmentsCount > 0 || 
        CourseInstancesCount > 0;

    /// <summary>
    /// Общее количество связанных записей
    /// </summary>
    public override int TotalRelatedCount => 
        CoursesCount + CurriculumsCount + AssignmentsCount + CourseInstancesCount;

    /// <summary>
    /// Детальное сообщение о связанных данных
    /// </summary>
    public override string RelatedDataMessage
    {
        get
        {
            if (!HasRelatedData)
                return "Связанных данных не найдено. Удаление предмета безопасно.";

            var details = new List<string>();
            if (CoursesCount > 0) details.Add($"курсы ({CoursesCount})");
            if (CurriculumsCount > 0) details.Add($"учебные планы ({CurriculumsCount})");
            if (AssignmentsCount > 0) details.Add($"задания ({AssignmentsCount})");
            if (CourseInstancesCount > 0) details.Add($"экземпляры курсов ({CourseInstancesCount})");

            return $"Предмет используется в: {string.Join(", ", details)}. " +
                   $"Всего связанных записей: {TotalRelatedCount}. " +
                   "Удаление может нарушить целостность данных.";
        }
    }
} 