using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Models.File;

namespace ViridiscaUi.Domain.Services.File;

/// <summary>
/// Интерфейс сервиса для работы с файлами
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Загружает файл по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    Task<FileUploadResult> UploadFileAsync(string filePath, string fileName, string[]? allowedExtensions = null);

    /// <summary>
    /// Загружает файл из потока
    /// </summary>
    /// <param name="stream">Поток с данными файла</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="contentType">Тип содержимого</param>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    Task<FileUploadResult> UploadFileAsync(Stream stream, string fileName, string contentType, string[]? allowedExtensions = null);

    /// <summary>
    /// Загружает файл из массива байтов
    /// </summary>
    /// <param name="fileData">Данные файла</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="contentType">Тип содержимого</param>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    Task<FileUploadResult> UploadFileAsync(byte[] fileData, string fileName, string contentType, string[]? allowedExtensions = null);

    /// <summary>
    /// Скачивает файл по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    Task<FileDownloadResult> DownloadFileAsync(string filePath);

    /// <summary>
    /// Удаляет файл по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Проверяет существование файла по UID
    /// </summary>
    /// <param name="fileUid">Идентификатор файла</param>
    Task<bool> FileExistsAsync(Guid fileUid);

    /// <summary>
    /// Проверяет существование файла по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Валидирует файл по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    /// <param name="maxSizeBytes">Максимальный размер в байтах</param>
    Task<FileValidationResult> ValidateFileAsync(string filePath, string[]? allowedExtensions = null, long? maxSizeBytes = null);

    /// <summary>
    /// Валидирует файл из потока
    /// </summary>
    /// <param name="stream">Поток с данными файла</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    /// <param name="maxSizeBytes">Максимальный размер в байтах</param>
    Task<FileValidationResult> ValidateFileAsync(Stream stream, string fileName, string[]? allowedExtensions = null, long? maxSizeBytes = null);

    /// <summary>
    /// Получает информацию о файле
    /// </summary>
    /// <param name="fileUid">Идентификатор файла</param>
    Task<Models.File.FileRecord?> GetFileInfoAsync(Guid fileUid);

    /// <summary>
    /// Получает файлы в директории
    /// </summary>
    /// <param name="directoryPath">Путь к директории</param>
    /// <param name="searchPattern">Шаблон поиска</param>
    Task<IEnumerable<Models.File.FileRecord>> GetFilesInDirectoryAsync(string directoryPath, string searchPattern = "*");

    /// <summary>
    /// Создает директорию
    /// </summary>
    /// <param name="directoryPath">Путь к директории</param>
    Task<bool> CreateDirectoryAsync(string directoryPath);

    /// <summary>
    /// Удаляет директорию
    /// </summary>
    /// <param name="directoryPath">Путь к директории</param>
    /// <param name="recursive">Рекурсивное удаление</param>
    Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = false);

    /// <summary>
    /// Копирует файл
    /// </summary>
    /// <param name="sourceFileUid">Идентификатор исходного файла</param>
    /// <param name="destinationPath">Путь назначения</param>
    /// <param name="newFileName">Новое имя файла</param>
    Task<FileUploadResult> CopyFileAsync(Guid sourceFileUid, string destinationPath, string? newFileName = null);

    /// <summary>
    /// Перемещает файл
    /// </summary>
    /// <param name="sourceFileUid">Идентификатор исходного файла</param>
    /// <param name="destinationPath">Путь назначения</param>
    /// <param name="newFileName">Новое имя файла</param>
    Task<FileUploadResult> MoveFileAsync(Guid sourceFileUid, string destinationPath, string? newFileName = null);

    /// <summary>
    /// Получает статистику хранилища
    /// </summary>
    Task<(long TotalSizeBytes, long UsedSizeBytes, long AvailableSizeBytes, int TotalFiles, Dictionary<string, int> FileTypeDistribution)> GetStorageStatisticsAsync();

    /// <summary>
    /// Выбирает файл через диалог
    /// </summary>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    Task<string?> SelectFileAsync(string[]? allowedExtensions = null);

    /// <summary>
    /// Выбирает несколько файлов через диалог
    /// </summary>
    /// <param name="allowedExtensions">Разрешенные расширения</param>
    Task<string[]?> SelectMultipleFilesAsync(string[]? allowedExtensions = null);
}
