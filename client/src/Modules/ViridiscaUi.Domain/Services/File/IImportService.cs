using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Domain.Services.File;

/// <summary>
/// Интерфейс сервиса импорта данных
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Импортирует данные из файла
    /// </summary>
    /// <typeparam name="T">Тип импортируемых объектов</typeparam>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="importType">Тип импорта</param>
    /// <returns>Результат импорта</returns>
    Task<ImportResult<T>> ImportFromFileAsync<T>(string filePath, string importType);

    /// <summary>
    /// Импортирует данные из потока
    /// </summary>
    /// <typeparam name="T">Тип импортируемых объектов</typeparam>
    /// <param name="stream">Поток данных</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="importType">Тип импорта</param>
    /// <returns>Результат импорта</returns>
    Task<ImportResult<T>> ImportFromStreamAsync<T>(Stream stream, string fileName, string importType);

    /// <summary>
    /// Валидирует файл для импорта
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="importType">Тип импорта</param>
    /// <returns>Результат валидации</returns>
    Task<ValidationResult> ValidateImportFileAsync(string filePath, string importType);

    /// <summary>
    /// Получает поддерживаемые типы импорта
    /// </summary>
    /// <returns>Список типов импорта</returns>
    Task<IEnumerable<string>> GetSupportedImportTypesAsync();

    /// <summary>
    /// Импортирует студентов из файла
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Количество импортированных студентов</returns>
    Task<int> ImportStudentsAsync(string filePath);
}
