using System.Collections.ObjectModel;

namespace ViridiscaUi.Models.Collections;

/// <summary>
/// Кэш источника данных для реактивных коллекций
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <typeparam name="TKey">Тип ключа</typeparam>
public class SourceCache<TEntity, TKey> : ObservableCollection<TEntity>
    where TKey : notnull
{
    private readonly Func<TEntity, TKey> _keySelector;
    private readonly Dictionary<TKey, TEntity> _cache = new();

    public SourceCache(Func<TEntity, TKey> keySelector)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
    }

    /// <summary>
    /// Добавляет или обновляет элемент в кэше
    /// </summary>
    public void AddOrUpdate(TEntity item)
    {
        if (item == null) return;

        var key = _keySelector(item);
        
        if (_cache.ContainsKey(key))
        {
            var index = Items.ToList().FindIndex(x => _keySelector(x).Equals(key));
            if (index >= 0)
            {
                Items[index] = item;
                _cache[key] = item;
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Replace, item, Items[index], index));
            }
        }
        else
        {
            _cache[key] = item;
            Add(item);
        }
    }

    /// <summary>
    /// Удаляет элемент по ключу
    /// </summary>
    public bool RemoveByKey(TKey key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            _cache.Remove(key);
            return Remove(item);
        }
        return false;
    }

    /// <summary>
    /// Получает элемент по ключу
    /// </summary>
    public TEntity? GetByKey(TKey key)
    {
        _cache.TryGetValue(key, out var item);
        return item;
    }

    /// <summary>
    /// Очищает кэш
    /// </summary>
    public new void Clear()
    {
        _cache.Clear();
        base.Clear();
    }

    /// <summary>
    /// Обновляет коллекцию из источника данных
    /// </summary>
    public void RefreshFrom(IEnumerable<TEntity> source)
    {
        Clear();
        
        foreach (var item in source)
        {
            var key = _keySelector(item);
            _cache[key] = item;
            Items.Add(item);
        }
        
        OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
            System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
    }
} 