using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Library;

/// <summary>
/// Заем библиотечного ресурса
/// </summary>
public class LibraryLoan : AuditableEntity
{
    /// <summary>
    /// ID ресурса
    /// </summary>
    public Guid ResourceUid { get; set; }

    /// <summary>
    /// ID заемщика (Person)
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Дата займа
    /// </summary>
    public DateTime LoanedAt { get; set; }

    /// <summary>
    /// Дата возврата (планируемая)
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Дата возврата (фактическая)
    /// </summary>
    public DateTime? ReturnedAt { get; set; }

    /// <summary>
    /// Сумма штрафа
    /// </summary>
    public decimal FineAmount { get; set; }

    /// <summary>
    /// Флаг, возвращен ли ресурс
    /// </summary>
    public bool IsReturned => ReturnedAt.HasValue;

    /// <summary>
    /// Заметки
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Ресурс
    /// </summary>
    public LibraryResource? Resource { get; set; }

    /// <summary>
    /// Заемщик
    /// </summary>
    public Person? Person { get; set; }
}