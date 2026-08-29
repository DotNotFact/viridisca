using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Library.Enums;

namespace ViridiscaUi.Domain.Entities.Library;

/// <summary>
/// Библиотечный ресурс
/// </summary>
public class LibraryResource : AuditableEntity
{
    /// <summary>
    /// Название ресурса
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Автор
    /// </summary>
    public string? Author { get; set; }
    
    /// <summary>
    /// ISBN
    /// </summary>
    public string? ISBN { get; set; }
    
    /// <summary>
    /// Издательство
    /// </summary>
    public string? Publisher { get; set; }
    
    /// <summary>
    /// Дата публикации
    /// </summary>
    public DateTime? PublishedDate { get; set; }
    
    /// <summary>
    /// Год публикации
    /// </summary>
    public int? PublicationYear { get; set; }
    
    /// <summary>
    /// Тип ресурса
    /// </summary>
    public ResourceType ResourceType { get; set; } = ResourceType.Book;
    
    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Местоположение в библиотеке
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Общее количество экземпляров
    /// </summary>
    public int TotalCopies { get; set; } = 1;
    
    /// <summary>
    /// Доступное количество экземпляров
    /// </summary>
    public int AvailableCopies { get; set; } = 1;
    
    /// <summary>
    /// Цифровой ресурс
    /// </summary>
    public bool IsDigital { get; set; }
    
    /// <summary>
    /// URL цифрового ресурса
    /// </summary>
    public string? DigitalUrl { get; set; }
    
    /// <summary>
    /// Теги для поиска (хранятся как строка, разделенная запятыми)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Займы ресурса
    /// </summary>
    public ICollection<LibraryLoan> Loans { get; set; } = new List<LibraryLoan>();
} 