using System.Collections.ObjectModel;

namespace ViridiscaUi.Extensions;

/// <summary>
/// Расширения для ObservableCollection
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Добавляет диапазон элементов в коллекцию
    /// </summary>
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Очищает коллекцию и добавляет новые элементы
    /// </summary>
    public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (items == null) throw new ArgumentNullException(nameof(items));

        collection.Clear();
        collection.AddRange(items);
    }
} 