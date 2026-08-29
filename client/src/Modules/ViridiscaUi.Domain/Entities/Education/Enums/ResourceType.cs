using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Тип библиотечного ресурса
/// </summary>
public enum ResourceType
{
    [Description("Книга")]
    Book = 1,

    [Description("Журнал")]
    Journal = 2,

    [Description("Статья")]
    Article = 3,

    [Description("Диссертация")]
    Thesis = 4,

    [Description("DVD/CD")]
    Media = 5,

    [Description("Электронный ресурс")]
    Digital = 6,

    [Description("Справочник")]
    Reference = 7,

    [Description("Учебник")]
    Textbook = 8
} 