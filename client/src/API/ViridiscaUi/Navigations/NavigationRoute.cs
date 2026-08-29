using System;
using System.Reactive;
using ReactiveUI;

namespace ViridiscaUi.Navigations;

/// <summary>
/// Модель маршрута навигации
/// </summary>
public class NavigationRoute(
    string path,
    Type viewModelType,
    string displayName,
    string? iconKey = null,
    int order = 0,
    string? group = null,
    string[]? requiredRoles = null,
    bool showInMenu = true,
    string? parentRoute = null,
    string? description = null,
    string? shortcut = null,
    string[]? tags = null,
    bool isBeta = false,
    bool requiresConfirmation = false)
{
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
    public Type ViewModelType { get; } = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
    public string DisplayName { get; } = displayName ?? throw new ArgumentNullException(nameof(displayName));
    public string? IconKey { get; } = iconKey;
    public int Order { get; } = order;
    public string? Group { get; } = group;
    public string[] RequiredRoles { get; } = requiredRoles ?? [];
    public bool ShowInMenu { get; } = showInMenu;
    public string? ParentRoute { get; } = parentRoute;
    public string? Description { get; } = description;
    public string? Shortcut { get; } = shortcut;
    public string[] Tags { get; } = tags ?? [];
    public bool IsBeta { get; } = isBeta;
    public bool RequiresConfirmation { get; } = requiresConfirmation;

    /// <summary>
    /// Команда навигации для данного маршрута
    /// </summary>
    public ReactiveCommand<Unit, Unit>? NavigateCommand { get; set; }
    
    /// <summary>
    /// Отображаемое имя для UI (alias для DisplayName)
    /// </summary>
    public string Label => DisplayName;

    /// <summary>
    /// Есть ли бейдж уведомлений
    /// </summary>
    public bool HasBadge { get; set; } = false;

    /// <summary>
    /// Текст бейджа уведомлений
    /// </summary>
    public string? BadgeText { get; set; }
} 