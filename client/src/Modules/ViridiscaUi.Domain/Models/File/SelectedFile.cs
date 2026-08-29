using System.IO;

namespace ViridiscaUi.Domain.Models.File;

/// <summary>
/// Выбранный файл для операций
/// </summary>
public class SelectedFile
{
    /// <summary>
    /// Имя файла
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Содержимое файла
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Поток файла
    /// </summary>
    public Stream? Stream { get; set; }

    /// <summary>
    /// MIME тип файла
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Путь к файлу (если файл из файловой системы)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Создает SelectedFile из байтового массива
    /// </summary>
    public static SelectedFile FromBytes(string fileName, byte[] content, string contentType)
    {
        return new SelectedFile
        {
            FileName = fileName,
            Content = content,
            ContentType = contentType,
            Size = content.Length
        };
    }

    /// <summary>
    /// Создает SelectedFile из потока
    /// </summary>
    public static SelectedFile FromStream(string fileName, Stream stream, string contentType)
    {
        return new SelectedFile
        {
            FileName = fileName,
            Stream = stream,
            ContentType = contentType,
            Size = stream.Length
        };
    }

    /// <summary>
    /// Создает SelectedFile из файла
    /// </summary>
    public static SelectedFile FromFile(string filePath)
    {
        var fileName = global::System.IO.Path.GetFileName(filePath);
        var content = global::System.IO.File.ReadAllBytes(filePath);
        var contentType = GetContentType(global::System.IO.Path.GetExtension(filePath));

        return new SelectedFile
        {
            FileName = fileName,
            Content = content,
            ContentType = contentType,
            Size = content.Length,
            FilePath = filePath
        };
    }

    /// <summary>
    /// Получает MIME тип по расширению файла
    /// </summary>
    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    public void Dispose()
    {
        Stream?.Dispose();
    }
} 