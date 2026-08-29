using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.File;

/// <summary>
/// Интерфейс сервиса для экспорта данных
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Экспортирует список студентов в Excel файл
    /// </summary>
    /// <param name="students">Список студентов для экспорта</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportStudentsToExcelAsync(IEnumerable<Student> students, string fileName = "Students");
    
    /// <summary>
    /// Экспортирует список студентов в CSV файл
    /// </summary>
    /// <param name="students">Список студентов для экспорта</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportStudentsToCsvAsync(IEnumerable<Student> students, string fileName = "Students");
    
    /// <summary>
    /// Экспортирует список студентов в PDF файл
    /// </summary>
    /// <param name="students">Список студентов для экспорта</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportStudentsToPdfAsync(IEnumerable<Student> students, string fileName = "Students");
    
    /// <summary>
    /// Экспортирует подробный отчет по студенту
    /// </summary>
    /// <param name="student">Студент для экспорта</param>
    /// <param name="includeGrades">Включать ли оценки</param>
    /// <param name="includeCourses">Включать ли курсы</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportStudentDetailAsync(Student student, bool includeGrades = true, bool includeCourses = true, string? fileName = null);
    
    /// <summary>
    /// Экспортирует список групп в Excel файл
    /// </summary>
    /// <param name="groups">Список групп для экспорта</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportGroupsToExcelAsync(IEnumerable<Group> groups, string fileName = "Groups");
    
    /// <summary>
    /// Экспортирует статистику по студентам
    /// </summary>
    /// <param name="statistics">Данные статистики</param>
    /// <param name="fileName">Имя файла (без расширения)</param>
    /// <returns>Путь к созданному файлу или null в случае ошибки</returns>
    Task<string?> ExportStudentStatisticsAsync(object statistics, string fileName = "StudentStatistics");
    
    /// <summary>
    /// Получает поддерживаемые форматы экспорта
    /// </summary>
    /// <returns>Список поддерживаемых форматов</returns>
    IEnumerable<string> GetSupportedExportFormats();
    
    /// <summary>
    /// Проверяет, поддерживается ли указанный формат
    /// </summary>
    /// <param name="format">Формат файла (например, "xlsx", "csv", "pdf")</param>
    /// <returns>True если формат поддерживается</returns>
    bool IsFormatSupported(string format);
}
