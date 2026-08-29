namespace ViridiscaUi.Domain.Models.Common;

/// <summary>
/// Результат с пагинацией
/// </summary>
/// <typeparam name="T">Тип элементов</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Элементы страницы
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Общее количество элементов
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Номер текущей страницы (начиная с 1)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Номер первого элемента на странице
    /// </summary>
    public int StartIndex => PageSize > 0 ? (Page - 1) * PageSize + 1 : 0;

    /// <summary>
    /// Номер последнего элемента на странице
    /// </summary>
    public int EndIndex => Math.Min(StartIndex + PageSize - 1, TotalCount);

    /// <summary>
    /// Создает пустой результат
    /// </summary>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
    {
        return new PagedResult<T>
        {
            Items = Enumerable.Empty<T>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Создает результат с данными
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
} 