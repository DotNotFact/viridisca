using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Navigations;
using ViridiscaUi.Infrastructure.Logger;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления данными Department
/// </summary>
/// <remarks>
/// Was previously duplicating DepartmentsViewModel's "departments" route (same path
/// string, different DisplayName) - both derive from RoutableViewModelBase, so
/// registration order (non-deterministic across rebuilds) decided which one the sidebar
/// item actually pointed at; only DepartmentsViewModel has a ReactiveViewLocator mapping.
/// Not a page - no [Route] here.
/// </remarks>
public class DepartmentViewModel : RoutableViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    // Коллекции
    [Reactive] public ObservableCollection<Department> Departments { get; set; } = new();
    [Reactive] public Department? SelectedItem { get; set; }
    
    // Пагинация и поиск
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalItems { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    // Команды
    public ReactiveCommand<Unit, Unit> LoadItemsCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateItemCommand { get; }
    public ReactiveCommand<Department, Unit> EditItemCommand { get; }
    public ReactiveCommand<Department, Unit> DeleteItemCommand { get; }

    public DepartmentViewModel(
        IScreen hostScreen,
        IDepartmentService departmentService,
        INotificationService notificationService,
        IDialogService dialogService) : base(hostScreen)
    {
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        LoadItemsCommand = ReactiveCommand.CreateFromTask(LoadItemsAsync);
        CreateItemCommand = ReactiveCommand.CreateFromTask(CreateItemAsync);
        EditItemCommand = ReactiveCommand.CreateFromTask<Department>(EditItemAsync);
        DeleteItemCommand = ReactiveCommand.CreateFromTask<Department>(DeleteItemAsync);

        // Автозагрузка при изменении поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .InvokeCommand(LoadItemsCommand);
    }

    /// <summary>
    /// Простой конструктор для использования в коллекциях
    /// </summary>
    public DepartmentViewModel(Department department) : base(null!)
    {
        if (department == null)
        {
            throw new ArgumentNullException(nameof(department));
        }

        SelectedItem = department;

        // Инициализируем базовые свойства
        Uid = department.Uid;
        Name = department.Name ?? string.Empty;
        Code = department.Code ?? string.Empty;
        Description = department.Description;
        IsActive = department.IsActive;
        CreatedAt = department.CreatedAt;
        LastModifiedAt = department.LastModifiedAt;
    }

    #region Properties for simple usage

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; }
    [Reactive] public bool IsActive { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    /// <summary>
    /// Загрузка элементов
    /// </summary>
    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            var departments = await _departmentService.GetAllAsync();
            
            Departments.Clear();
            foreach (var department in departments)
            {
                Departments.Add(department);
            }
        }
        catch (Exception ex)
        {
            await _notificationService.CreateNotificationAsync(
                Guid.Empty,
                "Ошибка загрузки",
                $"Не удалось загрузить департаменты: {ex.Message}",
                NotificationType.Error,
                NotificationPriority.High);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task<int> GetTotalItemsAsync()
    {
        try
        {
            var (_, totalCount) = await _departmentService.GetPagedAsync(1, 1, SearchText);
            return totalCount;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting total departments count: {ex.Message}", "DepartmentViewModel");
            return 0;
        }
    }

    protected async Task OnCreateAsync()
    {
        // Логика для подготовки создания нового департамента
        SelectedItem = new Department
        {
            Uid = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    protected async Task OnEditAsync()
    {
        // Логика для подготовки редактирования департамента
        if (SelectedItem != null)
        {
            StatusLogger.LogInfo($"Editing department: {SelectedItem.Name}", "DepartmentViewModel");
        }
    }

    protected async Task OnDeleteAsync()
    {
        // Логика для подготовки удаления департамента
        if (SelectedItem != null)
        {
            StatusLogger.LogInfo($"Preparing to delete department: {SelectedItem.Name}", "DepartmentViewModel");
        }
    }

    protected async Task OnSaveEditAsync()
    {
        try
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.Uid == Guid.Empty)
                {
                    // Создание нового департамента
                    var created = await _departmentService.CreateAsync(SelectedItem);
                    _notificationService.ShowSuccess("Кафедра успешно создана");
                }
                else
                {
                    // Обновление существующего департамента
                    var updated = await _departmentService.UpdateAsync(SelectedItem);
                    _notificationService.ShowSuccess("Кафедра успешно обновлена");
                }

                await LoadItemsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error saving department: {ex.Message}", "DepartmentViewModel");
            _notificationService.ShowError($"Ошибка сохранения: {ex.Message}");
            throw;
        }
    }

    protected async Task OnSaveDeleteAsync()
    {
        try
        {
            if (SelectedItem != null)
            {
                await _departmentService.DeleteAsync(SelectedItem.Uid);
                _notificationService.ShowSuccess("Кафедра успешно удалена");
                await LoadItemsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error deleting department: {ex.Message}", "DepartmentViewModel");
            _notificationService.ShowError($"Ошибка удаления: {ex.Message}");
            throw;
        }
    }

    protected async Task OnCancelEditAsync()
    {
        // Отмена редактирования
        SelectedItem = null;
        StatusLogger.LogInfo($"Edit cancelled");
    }

    protected async Task OnCancelDeleteAsync()
    {
        // Отмена удаления
        StatusLogger.LogInfo($"Delete cancelled");
    }

    public Department ToDepartment()
    {
        if (SelectedItem == null)
            return new Department();

        return new Department
        {
            Uid = SelectedItem.Uid,
            Name = SelectedItem.Name ?? string.Empty,
            Code = SelectedItem.Code ?? string.Empty,
            Description = SelectedItem.Description,
            IsActive = SelectedItem.IsActive,
            HeadOfDepartmentUid = SelectedItem.HeadOfDepartmentUid,
            CreatedAt = SelectedItem.CreatedAt,
            LastModifiedAt = SelectedItem.LastModifiedAt
        };
    }

    /// <summary>
    /// Обновляет данные из модели департамента
    /// </summary>
    public void UpdateFromModel(Department department)
    {
        SelectedItem = new Department
        {
            Uid = department.Uid,
            Name = department.Name ?? string.Empty,
            Code = department.Code ?? string.Empty,
            Description = department.Description,
            IsActive = department.IsActive,
            HeadOfDepartmentUid = department.HeadOfDepartmentUid,
            CreatedAt = department.CreatedAt,
            LastModifiedAt = department.LastModifiedAt
        };
    }

    protected virtual async Task<Department?> ShowCreateDialogAsync()
    {
        try
        {
            var dialogViewModel = new DepartmentDialogViewModel(
                HostScreen,
                _departmentService,
                _dialogService,
                Logger as ILogger<DepartmentDialogViewModel>);
            dialogViewModel.InitializeNew();

            // Используем правильный метод диалога
            var result = await _dialogService.ShowDepartmentEditDialogAsync(new Department());
            return result;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing create dialog: {ex.Message}", "DepartmentViewModel");
            return null;
        }
    }

    protected virtual async Task<Department?> ShowEditDialogAsync(Department item)
    {
        try
        {
            var dialogViewModel = new DepartmentDialogViewModel(
                HostScreen,
                _departmentService,
                _dialogService,
                Logger as ILogger<DepartmentDialogViewModel>);
            // Используем публичный метод для инициализации
            dialogViewModel.Name = item.Name ?? string.Empty;
            dialogViewModel.Code = item.Code ?? string.Empty;
            dialogViewModel.Description = item.Description ?? string.Empty;
            dialogViewModel.IsActive = item.IsActive;

            // Используем правильный метод диалога
            var result = await _dialogService.ShowDepartmentEditDialogAsync(item);
            return result;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing edit dialog: {ex.Message}", "DepartmentViewModel");
            return null;
        }
    }

    /// <summary>
    /// Создает новый департамент
    /// </summary>
    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        try
        {
            var result = await _departmentService.CreateDepartmentAsync(department);
            _notificationService.ShowSuccess("Департамент успешно создан");
            return result;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Не удалось создать департамент: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Обновляет департамент
    /// </summary>
    public async Task<bool> UpdateDepartmentAsync(Department department)
    {
        try
        {
            var result = await _departmentService.UpdateDepartmentAsync(department);
            _notificationService.ShowSuccess("Департамент успешно обновлен");
            return result;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Не удалось обновить департамент: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Удаляет департамент
    /// </summary>
    public async Task<bool> DeleteDepartmentAsync(Guid uid)
    {
        try
        {
            var result = await _departmentService.DeleteDepartmentAsync(uid);
            _notificationService.ShowSuccess("Департамент успешно удален");
            return result;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Не удалось удалить департамент: {ex.Message}");
            return false;
        }
    }

    private async Task CreateItemAsync()
    {
        try
        {
            await _dialogService.ShowDepartmentEditDialogAsync(new Department());
        }
        catch (Exception ex)
        {
            LogError($"Error creating department: {ex.Message}");
        }
    }

    private async Task EditItemAsync(Department department)
    {
        try
        {
            await _dialogService.ShowDepartmentEditDialogAsync(department);
        }
        catch (Exception ex)
        {
            LogError($"Error editing department: {ex.Message}");
        }
    }

    private async Task DeleteItemAsync(Department department)
    {
        try
        {
            await _departmentService.DeleteAsync(department.Uid);
        }
        catch (Exception ex)
        {
            LogError($"Error deleting department: {ex.Message}");
        }
    }
}

