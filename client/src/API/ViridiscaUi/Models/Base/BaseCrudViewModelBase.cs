namespace ViridiscaUi.Models.Base;

/// <summary>
/// Базовый класс для ViewModels с CRUD операциями
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
public abstract class BaseCrudViewModelBase<TEntity> : RoutableViewModelBase
    where TEntity : class
{
    [Reactive] public ObservableCollection<TEntity> Items { get; set; } = [];
    [Reactive] public TEntity? SelectedItem { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public new string? ErrorMessage { get; set; }
    [Reactive] public new bool HasError { get; set; }

    // Commands
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public new ReactiveCommand<Unit, Unit> CreateCommand { get; }
    public ReactiveCommand<TEntity, Unit> EditCommand { get; }
    public ReactiveCommand<TEntity, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    protected BaseCrudViewModelBase(IScreen hostScreen) : base(hostScreen)
    {
        // Initialize commands
        LoadCommand = ReactiveCommand.CreateFromTask(LoadItemsAsync);
        CreateCommand = ReactiveCommand.CreateFromTask(CreateItemAsync);
        EditCommand = ReactiveCommand.CreateFromTask<TEntity>(EditItemAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask<TEntity>(DeleteItemAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshItemsAsync);

        // Setup reactive subscriptions
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .DistinctUntilChanged()
            .Subscribe(_ => LoadCommand.Execute().Subscribe());

        // Handle errors
        this.WhenAnyObservable(
                x => x.LoadCommand.ThrownExceptions,
                x => x.CreateCommand.ThrownExceptions,
                x => x.EditCommand.ThrownExceptions,
                x => x.DeleteCommand.ThrownExceptions)
            .Subscribe(HandleError);
    }

    /// <summary>
    /// Загружает элементы из источника данных
    /// </summary>
    protected virtual async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var items = await GetItemsAsync();
            
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            StatusLogger.LogInfo($"Loaded {Items.Count} items");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка загрузки данных: {ex.Message}");
            SetError($"Ошибка загрузки данных: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Создает новый элемент
    /// </summary>
    protected virtual async Task CreateItemAsync()
    {
        try
        {
            var newItem = await CreateNewItemAsync();
            if (newItem != null)
            {
                Items.Add(newItem);
                SelectedItem = newItem;
                StatusLogger.LogInfo("Item created successfully");
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка создания элемента: {ex.Message}");
            SetError($"Ошибка создания: {ex.Message}");
        }
    }

    /// <summary>
    /// Редактирует элемент
    /// </summary>
    protected virtual async Task EditItemAsync(TEntity item)
    {
        try
        {
            var updatedItem = await UpdateItemAsync(item);
            if (updatedItem != null)
            {
                var index = Items.IndexOf(item);
                if (index >= 0)
                {
                    Items[index] = updatedItem;
                }
                StatusLogger.LogInfo("Item updated successfully");
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка обновления элемента: {ex.Message}");
            SetError($"Ошибка обновления: {ex.Message}");
        }
    }

    /// <summary>
    /// Удаляет элемент
    /// </summary>
    protected virtual async Task DeleteItemAsync(TEntity item)
    {
        try
        {
            var success = await RemoveItemAsync(item);
            if (success)
            {
                Items.Remove(item);
                if (SelectedItem == item)
                {
                    SelectedItem = null;
                }
                StatusLogger.LogInfo("Item deleted successfully");
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка удаления элемента: {ex.Message}");
            SetError($"Ошибка удаления: {ex.Message}");
        }
    }

    /// <summary>
    /// Обновляет список элементов
    /// </summary>
    protected virtual async Task RefreshItemsAsync()
    {
        await LoadItemsAsync();
    }

    /// <summary>
    /// Получает элементы из источника данных (должен быть переопределен в наследниках)
    /// </summary>
    protected abstract Task<IEnumerable<TEntity>> GetItemsAsync();

    /// <summary>
    /// Создает новый элемент (должен быть переопределен в наследниках)
    /// </summary>
    protected abstract Task<TEntity?> CreateNewItemAsync();

    /// <summary>
    /// Обновляет элемент (должен быть переопределен в наследниках)
    /// </summary>
    protected abstract Task<TEntity?> UpdateItemAsync(TEntity item);

    /// <summary>
    /// Удаляет элемент (должен быть переопределен в наследниках)
    /// </summary>
    protected abstract Task<bool> RemoveItemAsync(TEntity item);

    /// <summary>
    /// Устанавливает ошибку
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    /// <summary>
    /// Очищает ошибку
    /// </summary>
    protected new void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    /// <summary>
    /// Обрабатывает исключения
    /// </summary>
    protected virtual void HandleError(Exception ex)
    {
        StatusLogger.LogError($"Ошибка в операции: {ex.Message}");
        SetError(ex.Message);
    }
} 