using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Роль человека в системе
/// </summary>
public class PersonRole : AuditableEntity
{
    /// <summary>
    /// ID человека
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// ID роли
    /// </summary>
    public Guid RoleUid { get; set; }

    /// <summary>
    /// Когда назначена роль
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Когда истекает роль
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Активна ли роль
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// ID сущности, в контексте которой назначена роль (например, GroupId, CourseId)
    /// </summary>
    public Guid? ContextEntityUid { get; set; }

    /// <summary>
    /// Тип сущности, в контексте которой назначена роль (например, "Group", "Course")
    /// </summary>
    public string? ContextEntityType { get; set; }

    /// <summary>
    /// Контекст роли (алиас для совместимости)
    /// </summary>
    public string? Context 
    { 
        get => ContextEntityType; 
        set => ContextEntityType = value; 
    }

    /// <summary>
    /// Идентификатор пользователя, назначившего роль
    /// </summary>
    public Guid? AssignedBy { get; set; }

    /// <summary>
    /// Человек
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Истекла ли роль
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    public PersonRole()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        AssignedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Extend(DateTime newExpiryDate)
    {
        ExpiresAt = newExpiryDate;
        LastModifiedAt = DateTime.UtcNow;
    }
} 