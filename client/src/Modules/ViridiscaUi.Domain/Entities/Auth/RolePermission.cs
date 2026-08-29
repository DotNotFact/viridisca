using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Связь между ролью и разрешением
/// </summary>
public class RolePermission : AuditableEntity
{
    /// <summary>
    /// Идентификатор роли
    /// </summary>
    public Guid RoleUid { get; set; }

    /// <summary>
    /// Идентификатор разрешения
    /// </summary>
    public Guid PermissionUid { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Разрешение
    /// </summary>
    public Permission? Permission { get; set; }
}
