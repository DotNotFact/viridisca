using System;

namespace ViridiscaUi.Navigations;
 
/// <summary>
/// Атрибут для определения маршрута навигации для ViewModel
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RouteAttribute(string path) : Attribute
{
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
    public string? DisplayName { get; set; }
    public string? IconKey { get; set; }
    public int Order { get; set; } = 0;
    public string? Group { get; set; }
    public string[]? RequiredRoles { get; set; }
    public bool ShowInMenu { get; set; } = true;
    public string? ParentRoute { get; set; }
    public string? Description { get; set; }
    public string? Shortcut { get; set; }
    public string[]? Tags { get; set; }
    public bool IsBeta { get; set; } = false;
    public bool RequiresConfirmation { get; set; } = false;
}
 