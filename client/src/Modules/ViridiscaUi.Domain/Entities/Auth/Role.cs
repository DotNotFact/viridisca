using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Роль в системе
/// </summary>
public class Role : AuditableEntity
{
    /// <summary>
    /// Название роли
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название роли
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Описание роли
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип роли (для совместимости)
    /// </summary>
    public string? RoleType { get; set; }

    /// <summary>
    /// Связи с разрешениями
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    /// <summary>
    /// Роли пользователей
    /// </summary>
    public ICollection<PersonRole> PersonRoles { get; set; } = [];
} 