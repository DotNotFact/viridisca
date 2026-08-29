using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Domain.Models.Common;
using ViridiscaUi.Domain.Services.File;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Models.File;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с файлами
/// </summary>
public class FileService : IFileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public FileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        
        // Настройки файлового хранилища
        _uploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ViridiscaUi", "Files");
        _maxFileSize = 50 * 1024 * 1024; // 50 MB
        _allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
        
        // Создаем директорию если не существует
        Directory.CreateDirectory(_uploadPath);
    }

    #region IFileService Interface Implementation

    /// <summary>
    /// Загружает файл по пути
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(string filePath, string fileName, string[]? allowedExtensions = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return FileUploadResult.Failure("Файл не найден");
            }

            var contentType = GetContentType(fileName);
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await UploadFileAsync(fileStream, fileName, contentType, allowedExtensions);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file from path {filePath}: {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    /// <summary>
    /// Загружает файл из потока
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(Stream stream, string fileName, string contentType, string[]? allowedExtensions = null)
    {
        try
        {
            // Валидация расширений
            if (allowedExtensions != null && allowedExtensions.Length > 0)
            {
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return FileUploadResult.Failure($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", allowedExtensions)}");
                }
            }

            return await UploadFileAsync(stream, fileName, contentType, null, null);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file from stream: {fileName} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    /// <summary>
    /// Загружает файл из массива байтов
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(byte[] fileData, string fileName, string contentType, string[]? allowedExtensions = null)
    {
        try
        {
            using var memoryStream = new MemoryStream(fileData);
            return await UploadFileAsync(memoryStream, fileName, contentType, allowedExtensions);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file from byte array: {fileName} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    /// <summary>
    /// Скачивает файл
    /// </summary>
    public async Task<FileDownloadResult> DownloadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return FileDownloadResult.Failure("Файл не найден");
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            return FileDownloadResult.Success(fileName, fileBytes, contentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error downloading file {filePath}: {ex.Message}", nameof(FileService));
            return FileDownloadResult.Failure("Произошла ошибка при скачивании файла");
        }
    }

    /// <summary>
    /// Удаляет файл
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                StatusLogger.LogInfo($"File deleted successfully: {filePath}", nameof(FileService));
            }
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error deleting file {filePath}: {ex.Message}", nameof(FileService));
            return false;
        }
    }

    /// <summary>
    /// Проверяет существование файла по UID
    /// </summary>
    public async Task<bool> FileExistsAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            return fileRecord != null && File.Exists(fileRecord.FilePath);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error checking file existence: {fileUid} - {ex.Message}", nameof(FileService));
            return false;
        }
    }

    /// <summary>
    /// Проверяет существование файла по пути
    /// </summary>
    public async Task<bool> FileExistsAsync(string filePath)
    {
        return await Task.FromResult(File.Exists(filePath));
    }

    /// <summary>
    /// Валидирует файл по пути
    /// </summary>
    public async Task<FileValidationResult> ValidateFileAsync(string filePath, string[]? allowedExtensions = null, long? maxSizeBytes = null)
    {
        var result = new FileValidationResult();

        try
        {
            if (!File.Exists(filePath))
            {
                result.Errors.Add("Файл не найден");
                return result;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            var fileName = fileInfo.Name;
            var fileExtension = fileInfo.Extension.ToLowerInvariant();

            // Проверка размера файла
            var maxSize = maxSizeBytes ?? _maxFileSize;
            if (fileInfo.Length > maxSize)
            {
                result.Errors.Add($"Размер файла превышает максимально допустимый ({maxSize / (1024 * 1024)} MB)");
            }

            // Проверка расширения файла
            var extensions = allowedExtensions ?? _allowedExtensions;
            if (!extensions.Contains(fileExtension))
            {
                result.Errors.Add($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", extensions)}");
            }

            result.FileSize = fileInfo.Length;
            result.ContentType = GetContentType(fileName);

            return result;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error validating file: {filePath} - {ex.Message}", nameof(FileService));
            return FileValidationResult.Invalid(filePath, "Произошла ошибка при валидации файла");
        }
    }

    /// <summary>
    /// Валидирует файл из потока
    /// </summary>
    public async Task<FileValidationResult> ValidateFileAsync(Stream stream, string fileName, string[]? allowedExtensions = null, long? maxSizeBytes = null)
    {
        var result = new FileValidationResult
        {
            ContentType = GetContentType(fileName),
            FileSize = stream.Length
        };

        // Проверка размера файла
        var maxSize = maxSizeBytes ?? _maxFileSize;
        if (stream.Length > maxSize)
        {
            result.Errors.Add($"Размер файла превышает максимально допустимый ({maxSize / (1024 * 1024)} MB)");
        }

        // Проверка расширения файла
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var extensions = allowedExtensions ?? _allowedExtensions;
        if (!extensions.Contains(fileExtension))
        {
            result.Errors.Add($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", extensions)}");
        }

        // Проверка имени файла
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            result.Errors.Add("Недопустимое имя файла");
        }

        return result;
    }

    /// <summary>
    /// Получает информацию о файле
    /// </summary>
    public async Task<ViridiscaUi.Domain.Models.File.FileRecord?> GetFileInfoAsync(Guid fileUid)
    {
        try
        {
            var entityRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (entityRecord == null)
                return null;

            return new ViridiscaUi.Domain.Models.File.FileRecord
            {
                FileName = entityRecord.OriginalFileName,
                FilePath = entityRecord.FilePath,
                ContentType = entityRecord.ContentType,
                FileSize = entityRecord.FileSize
            };
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting file info: {fileUid} - {ex.Message}", nameof(FileService));
            return null;
        }
    }

    /// <summary>
    /// Получает файлы в директории
    /// </summary>
    public async Task<IEnumerable<ViridiscaUi.Domain.Models.File.FileRecord>> GetFilesInDirectoryAsync(string directoryPath, string searchPattern = "*")
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return Enumerable.Empty<ViridiscaUi.Domain.Models.File.FileRecord>();
            }

            var files = Directory.GetFiles(directoryPath, searchPattern);
            var fileRecords = new List<ViridiscaUi.Domain.Models.File.FileRecord>();

            foreach (var filePath in files)
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                var fileRecord = new ViridiscaUi.Domain.Models.File.FileRecord
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    ContentType = GetContentType(fileInfo.Name),
                    FileSize = fileInfo.Length
                };
                fileRecords.Add(fileRecord);
            }

            return fileRecords;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting files in directory: {directoryPath} - {ex.Message}", nameof(FileService));
            return Enumerable.Empty<ViridiscaUi.Domain.Models.File.FileRecord>();
        }
    }

    /// <summary>
    /// Создает директорию
    /// </summary>
    public async Task<bool> CreateDirectoryAsync(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                StatusLogger.LogInfo($"Directory created successfully: {directoryPath}", nameof(FileService));
            }
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error creating directory {directoryPath}: {ex.Message}", nameof(FileService));
            return false;
        }
    }

    /// <summary>
    /// Удаляет директорию
    /// </summary>
    public async Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = false)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive);
                StatusLogger.LogInfo($"Directory deleted successfully: {directoryPath}", nameof(FileService));
            }
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error deleting directory {directoryPath}: {ex.Message}", nameof(FileService));
            return false;
        }
    }

    /// <summary>
    /// Копирует файл
    /// </summary>
    public async Task<FileUploadResult> CopyFileAsync(Guid sourceFileUid, string destinationPath, string? newFileName = null)
    {
        try
        {
            var sourceFileRecord = await _dbContext.FileRecords.FindAsync(sourceFileUid);
            if (sourceFileRecord == null)
            {
                return FileUploadResult.Failure("Исходный файл не найден");
            }

            if (!File.Exists(sourceFileRecord.FilePath))
            {
                return FileUploadResult.Failure("Исходный файл не найден на диске");
            }

            var fileName = newFileName ?? sourceFileRecord.OriginalFileName;
            var destinationFilePath = Path.Combine(destinationPath, fileName);

            Directory.CreateDirectory(destinationPath);
            File.Copy(sourceFileRecord.FilePath, destinationFilePath, true);

            // Создаем новую запись в базе данных
            var newFileRecord = new ViridiscaUi.Domain.Entities.System.FileRecord
            {
                Uid = Guid.NewGuid(),
                OriginalFileName = fileName,
                StoredFileName = Path.GetFileName(destinationFilePath),
                FilePath = destinationFilePath,
                ContentType = sourceFileRecord.ContentType,
                FileSize = sourceFileRecord.FileSize,
                EntityType = sourceFileRecord.EntityType,
                EntityUid = sourceFileRecord.EntityUid,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.FileRecords.AddAsync(newFileRecord);
            await _dbContext.SaveChangesAsync();

            return FileUploadResult.Success(newFileRecord.Uid, fileName, destinationFilePath, newFileRecord.FileSize, newFileRecord.ContentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error copying file: {sourceFileUid} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при копировании файла");
        }
    }

    /// <summary>
    /// Перемещает файл
    /// </summary>
    public async Task<FileUploadResult> MoveFileAsync(Guid sourceFileUid, string destinationPath, string? newFileName = null)
    {
        try
        {
            var sourceFileRecord = await _dbContext.FileRecords.FindAsync(sourceFileUid);
            if (sourceFileRecord == null)
            {
                return FileUploadResult.Failure("Исходный файл не найден");
            }

            if (!File.Exists(sourceFileRecord.FilePath))
            {
                return FileUploadResult.Failure("Исходный файл не найден на диске");
            }

            var fileName = newFileName ?? sourceFileRecord.OriginalFileName;
            var destinationFilePath = Path.Combine(destinationPath, fileName);

            Directory.CreateDirectory(destinationPath);
            File.Move(sourceFileRecord.FilePath, destinationFilePath);

            // Обновляем запись в базе данных
            sourceFileRecord.OriginalFileName = fileName;
            sourceFileRecord.StoredFileName = Path.GetFileName(destinationFilePath);
            sourceFileRecord.FilePath = destinationFilePath;

            _dbContext.FileRecords.Update(sourceFileRecord);
            await _dbContext.SaveChangesAsync();

            return FileUploadResult.Success(sourceFileRecord.Uid, fileName, destinationFilePath, sourceFileRecord.FileSize, sourceFileRecord.ContentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error moving file: {sourceFileUid} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при перемещении файла");
        }
    }

    /// <summary>
    /// Получает статистику хранилища
    /// </summary>
    public async Task<(long TotalSizeBytes, long UsedSizeBytes, long AvailableSizeBytes, int TotalFiles, Dictionary<string, int> FileTypeDistribution)> GetStorageStatisticsAsync()
    {
        try
        {
            var totalFiles = await _dbContext.FileRecords.CountAsync();
            var totalSize = await _dbContext.FileRecords.SumAsync(f => f.FileSize);
            
            var filesByType = await _dbContext.FileRecords
                .GroupBy(f => f.ContentType)
                .Select(g => new { ContentType = g.Key, Count = g.Count() })
                .ToListAsync();

            // Получаем информацию о доступном месте на диске
            var driveInfo = new DriveInfo(Path.GetPathRoot(_uploadPath) ?? "C:");
            var availableSpace = driveInfo.AvailableFreeSpace;
            var usedSpace = totalSize;

            var fileTypeDistribution = filesByType.ToDictionary(x => x.ContentType ?? "unknown", x => x.Count);

            return (totalSize, usedSpace, availableSpace, totalFiles, fileTypeDistribution);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting storage statistics: {ex.Message}", nameof(FileService));
            return (0, 0, 0, 0, new Dictionary<string, int>());
        }
    }

    /// <summary>
    /// Выбирает файл через диалог
    /// NOTE: This method should be implemented in UI layer, not Infrastructure
    /// </summary>
    public async Task<string?> SelectFileAsync(string[]? allowedExtensions = null)
    {
        // TODO: Move to UI layer - Infrastructure should not have UI dependencies
        StatusLogger.LogWarning("SelectFileAsync called in Infrastructure layer - should be moved to UI layer");
        await Task.CompletedTask;
        throw new NotImplementedException("File selection should be implemented in UI layer");
    }

    /// <summary>
    /// Выбирает несколько файлов через диалог
    /// NOTE: This method should be implemented in UI layer, not Infrastructure
    /// </summary>
    public async Task<string[]?> SelectMultipleFilesAsync(string[]? allowedExtensions = null)
    {
        // TODO: Move to UI layer - Infrastructure should not have UI dependencies
        StatusLogger.LogWarning("SelectMultipleFilesAsync called in Infrastructure layer - should be moved to UI layer");
        await Task.CompletedTask;
        throw new NotImplementedException("File selection should be implemented in UI layer");
    }

    #endregion

    public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            // Валидация файла
            var validationResult = await ValidateFileAsync(fileStream, fileName, contentType);
            if (!validationResult.IsValid)
            {
                return FileUploadResult.Failure(string.Join("; ", validationResult.Errors));
            }

            // Генерируем уникальное имя файла
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Сохраняем файл на диск
            using (var fileStreamDisk = new FileStream(filePath, FileMode.Create))
            {
                fileStream.Position = 0; // Сбрасываем позицию потока
                await fileStream.CopyToAsync(fileStreamDisk);
            }

            // Создаем запись в базе данных
            var fileRecord = new Domain.Entities.System.FileRecord
            {
                Uid = Guid.NewGuid(),
                OriginalFileName = fileName,
                StoredFileName = uniqueFileName,
                FilePath = filePath,
                ContentType = contentType,
                FileSize = fileStream.Length,
                EntityType = entityType, // ParseEntityType(entityType),
                EntityUid = entityUid,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.FileRecords.AddAsync(fileRecord);
            await _dbContext.SaveChangesAsync();

            StatusLogger.LogInfo($"File uploaded successfully: {fileName}", nameof(FileService));

            return FileUploadResult.Success(fileRecord.Uid, fileName, filePath, fileRecord.FileSize, fileRecord.ContentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file: {fileName} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(string filePath, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return FileUploadResult.Failure("Файл не найден");
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await UploadFileAsync(fileStream, fileName, contentType, entityUid, entityType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file from path {filePath}: {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(byte[] fileData, string fileName, string contentType, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            using var memoryStream = new MemoryStream(fileData);
            return await UploadFileAsync(memoryStream, fileName, contentType, entityUid, entityType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading file from byte array: {fileName} - {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке файла");
        }
    }

    public async Task<FileDownloadResult?> DownloadFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null)
            {
                return FileDownloadResult.Failure("Файл не найден");
            }

            if (!File.Exists(fileRecord.FilePath))
            {
                StatusLogger.LogWarning("File not found on disk: {FilePath}", fileRecord.FilePath);
                return FileDownloadResult.Failure("Файл не найден на диске");
            }

            var fileContent = await File.ReadAllBytesAsync(fileRecord.FilePath);
            return FileDownloadResult.Success(fileRecord.OriginalFileName, fileContent, fileRecord.ContentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error downloading file: {fileUid} - {ex.Message}", nameof(FileService));
            return FileDownloadResult.Failure("Произошла ошибка при скачивании файла");
        }
    }

    public async Task<FileDownloadResult?> DownloadFileAsync(string fileName, Guid? entityUid = null)
    {
        try
        {
            var query = _dbContext.FileRecords.AsQueryable();
            
            if (entityUid.HasValue)
            {
                query = query.Where(f => f.EntityUid == entityUid.Value && f.OriginalFileName == fileName);
            }
            else
            {
                query = query.Where(f => f.OriginalFileName == fileName);
            }

            var fileRecord = await query.FirstOrDefaultAsync();
            if (fileRecord == null)
            {
                return FileDownloadResult.Failure("Файл не найден");
            }

            return await DownloadFileAsync(fileRecord.Uid);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error downloading file by name: {fileName} - {ex.Message}", nameof(FileService));
            return FileDownloadResult.Failure("Произошла ошибка при скачивании файла");
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null)
                return false;

            // Удаляем файл с диска
            if (File.Exists(fileRecord.FilePath))
            {
                File.Delete(fileRecord.FilePath);
            }

            // Удаляем запись из базы данных
            _dbContext.FileRecords.Remove(fileRecord);
            await _dbContext.SaveChangesAsync();

            StatusLogger.LogInfo($"File deleted successfully: {fileUid}", nameof(FileService));
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error deleting file: {fileUid} - {ex.Message}", nameof(FileService));
            return false;
        }
    }

    public async Task<IEnumerable<Domain.Models.File.FileInfo>> GetFilesByEntityAsync(Guid entityUid, string? entityType = null)
    {
        var query = _dbContext.FileRecords
            .Where(f => f.EntityUid == entityUid);

        if (!string.IsNullOrEmpty(entityType))
        {
            var parsedEntityType = entityType; // ParseEntityType(entityType);
            query = query.Where(f => f.EntityType == parsedEntityType);
        }

        var fileRecords = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fileRecords.Select(f => new Domain.Models.File.FileInfo
        {
            Uid = f.Uid,
            FileName = f.OriginalFileName,
            ContentType = f.ContentType,
            Size = f.FileSize,
            CreatedAt = f.CreatedAt,
            LastModifiedAt = f.LastModifiedAt,
            EntityUid = f.EntityUid,
            EntityType = f.EntityType?.ToString() ?? string.Empty,
            FilePath = f.FilePath
        });
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType, long? maxSizeBytes = null)
    {
        var result = new FileValidationResult
        {
            ContentType = contentType,
            FileSize = fileStream.Length
        };

        // Проверка размера файла
        var maxSize = maxSizeBytes ?? _maxFileSize;
        if (fileStream.Length > maxSize)
        {
            result.Errors.Add($"Размер файла превышает максимально допустимый ({maxSize / (1024 * 1024)} MB)");
        }

        // Проверка расширения файла
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            result.Errors.Add($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", _allowedExtensions)}");
        }

        // Проверка имени файла
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            result.Errors.Add("Недопустимое имя файла");
        }

        // Проверка на вредоносное содержимое (базовая)
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            result.Errors.Add("Имя файла содержит недопустимые символы");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    public async Task<FileValidationResult> ValidateFileAsync(string filePath, long? maxSizeBytes = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    Errors = { "Файл не найден" }
                };
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await ValidateFileAsync(fileStream, fileName, contentType, maxSizeBytes);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error validating file: {filePath} - {ex.Message}", nameof(FileService));
            return FileValidationResult.Invalid(filePath, "Произошла ошибка при валидации файла");
        }
    }

    public async Task<IEnumerable<string>> GetAllowedFileTypesAsync()
    {
        return await Task.FromResult(_allowedExtensions.AsEnumerable());
    }

    public async Task<long> GetMaxFileSizeAsync()
    {
        return await Task.FromResult(_maxFileSize);
    }

    public async Task<string?> GetFilePathAsync(Guid fileUid)
    {
        var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
        return fileRecord?.FilePath;
    }

    public async Task<bool> BackupFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null || !File.Exists(fileRecord.FilePath))
                return false;

            var backupPath = Path.Combine(_uploadPath, "Backups");
            Directory.CreateDirectory(backupPath);

            var backupFileName = $"{Path.GetFileNameWithoutExtension(fileRecord.StoredFileName)}_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(fileRecord.StoredFileName)}";
            var backupFilePath = Path.Combine(backupPath, backupFileName);

            File.Copy(fileRecord.FilePath, backupFilePath, true);

            StatusLogger.LogInfo($"File backup created: {fileUid} -> {backupFilePath}", nameof(FileService));
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error creating backup for file: {fileUid} - {ex.Message}", nameof(FileService));
            return false;
        }
    }

    public async Task CleanupTemporaryFilesAsync(TimeSpan olderThan)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow - olderThan;
            
            var orphanedFiles = await _dbContext.FileRecords
                .Where(f => f.EntityUid == null && f.CreatedAt < cutoffDate)
                .ToListAsync();

            foreach (var file in orphanedFiles)
            {
                await DeleteFileAsync(file.Uid);
            }

            StatusLogger.LogInfo($"Cleaned up {orphanedFiles.Count} temporary files older than {cutoffDate}", nameof(FileService));
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error cleaning up temporary files: {ex.Message}", nameof(FileService));
        }
    }

    /// <summary>
    /// Выбирает изображение через диалог
    /// NOTE: This method should be implemented in UI layer, not Infrastructure
    /// </summary>
    public async Task<SelectedFile?> SelectImageFileAsync()
    {
        // TODO: Move to UI layer - Infrastructure should not have UI dependencies
        StatusLogger.LogWarning("SelectImageFileAsync called in Infrastructure layer - should be moved to UI layer");
        await Task.CompletedTask;
        throw new NotImplementedException("Image selection should be implemented in UI layer");
    }

    public async Task<FileUploadResult> UploadProfileImageAsync(SelectedFile selectedFile, Guid personUid)
    {
        try
        {
            // Валидация изображения
            if (selectedFile.Stream == null)
            {
                return FileUploadResult.Failure("Поток файла недоступен");
            }

            // Проверяем, что это изображение
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(selectedFile.FileName).ToLowerInvariant();
            if (!imageExtensions.Contains(fileExtension))
            {
                return FileUploadResult.Failure($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", imageExtensions)}");
            }

            var fileName = selectedFile.FileName;
            var filePath = Path.GetFullPath(Path.Combine(_uploadPath, Guid.NewGuid().ToString() + Path.GetExtension(fileName)));

            // Создаем директорию если не существует
            Directory.CreateDirectory(_uploadPath);

            // Сохраняем файл
            await global::System.IO.File.WriteAllBytesAsync(filePath, selectedFile.Content);

            // Создаем запись в базе данных
            var fileRecord = new ViridiscaUi.Domain.Entities.System.FileRecord
            {
                Uid = Guid.NewGuid(),
                OriginalFileName = fileName,
                StoredFileName = Path.GetFileName(filePath),
                FilePath = filePath,
                ContentType = selectedFile.ContentType,
                FileSize = selectedFile.Size,
                EntityType = "ProfileImage",
                EntityUid = personUid,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.FileRecords.AddAsync(fileRecord);
            await _dbContext.SaveChangesAsync();

            StatusLogger.LogInfo($"Profile image uploaded for person {personUid}: {selectedFile.FileName}", nameof(FileService));

            return FileUploadResult.Success(fileRecord.Uid, fileName, filePath, fileRecord.FileSize, fileRecord.ContentType);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error uploading profile image for person {personUid}: {ex.Message}", nameof(FileService));
            return FileUploadResult.Failure("Произошла ошибка при загрузке изображения профиля");
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    } 
} 