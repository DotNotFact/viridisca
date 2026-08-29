using System.Collections.ObjectModel;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.Models;

/// <summary>
/// Группа элементов меню
/// </summary>
public class MenuGroup
{
    /// <summary>
    /// Название группы (например, "Основное", "Образование", "Система")
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Элементы меню в этой группе
    /// </summary>
    public ObservableCollection<NavigationRoute> Items { get; set; } = [];

    /// <summary>
    /// Порядок отображения группы
    /// </summary>
    public int Order { get; set; }
}

