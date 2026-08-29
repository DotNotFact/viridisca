using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Разрешение в системе
/// </summary>
public class Permission : AuditableEntity
{
    /// <summary>
    /// Название разрешения
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название разрешения
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Описание разрешения
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Связи с ролями
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
