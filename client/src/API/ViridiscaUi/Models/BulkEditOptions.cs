namespace ViridiscaUi.Models;

/// <summary>
/// Опции для массового редактирования
/// </summary>
public class BulkEditOptions
{
    /// <summary>
    /// Можно ли изменять группу
    /// </summary>
    public bool CanChangeGroup { get; set; }

    /// <summary>
    /// Можно ли изменять статус
    /// </summary>
    public bool CanChangeStatus { get; set; }

    /// <summary>
    /// Можно ли изменять академический год
    /// </summary>
    public bool CanChangeAcademicYear { get; set; }

    /// <summary>
    /// Новый идентификатор группы
    /// </summary>
    public Guid? NewGroupUid { get; set; }

    /// <summary>
    /// Новый статус
    /// </summary>
    public StudentStatus? NewStatus { get; set; }

    /// <summary>
    /// Новый академический год
    /// </summary>
    public int? NewAcademicYear { get; set; }
} 