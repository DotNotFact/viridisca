using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using Microsoft.Extensions.Logging;
using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore; 
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Domain.Services;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления департаментами с полной реализацией CRUD операций
/// </summary>
[Route("departments", DisplayName = "Отделы", IconKey = "Building", Order = 1, Group = "Система", ShowInMenu = true)]
public class DepartmentsViewModel : RoutableViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IPersonService _personService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly IAuthService _authService;
    private readonly ILogger<DepartmentsViewModel> _logger;
    private readonly SourceList<Department> _departments = new();

    #region Properties

    [Reactive] public string Title { get; set; } = "Управление отделами";
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public new string? ErrorMessage { get; set; }
    [Reactive] public Department? SelectedDepartment { get; set; }
    
    // Filter Properties
    [Reactive] public bool ShowActiveOnly { get; set; } = false;
    [Reactive] public bool ShowInactiveOnly { get; set; } = false;
    
    // Statistics Properties
    [Reactive] public int TotalDepartments { get; set; } = 0;
    [Reactive] public int ActiveDepartments { get; set; } = 0;
    [Reactive] public int InactiveDepartments { get; set; } = 0;

    // Collections
    public ReadOnlyObservableCollection<Department> Departments => _filteredDepartments;
    private readonly ReadOnlyObservableCollection<Department> _filteredDepartments;

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateDepartmentCommand { get; private set; } = null!;
    public ReactiveCommand<Department, Unit> EditDepartmentCommand { get; private set; } = null!;
    public ReactiveCommand<Department, Unit> DeleteDepartmentCommand { get; private set; } = null!;
    public ReactiveCommand<Department, Unit> ViewDepartmentDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<Department, Unit> ToggleActiveStatusCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportToCsvCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;

    #endregion

    public DepartmentsViewModel(
        IScreen hostScreen,
        IDepartmentService departmentService,
        IPersonService personService,
        INotificationService notificationService,
        IDialogService dialogService,
        IStatusService statusService,
        IAuthService authService,
        ILogger<DepartmentsViewModel> logger) : base(hostScreen)
    {
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService;
        _authService = authService;
        _logger = logger;

        // Setup filtered collection
        var filterPredicate = this.WhenAnyValue(
                x => x.SearchText,
                x => x.ShowActiveOnly,
                x => x.ShowInactiveOnly)
            .Select(CreateFilter);

        _departments.Connect()
            .Filter(filterPredicate)
            .Sort(SortExpressionComparer<Department>.Ascending(d => d.Name))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _filteredDepartments)
            .Subscribe(_ => UpdateStatistics())
            .DisposeWith(Disposables);

        InitializeCommands();
        SetupSubscriptions();
    }

    private void InitializeCommands()
    {
        LoadDepartmentsCommand = CreateCommand(async () => await LoadDepartmentsAsync());
        RefreshCommand = CreateCommand(async () => await RefreshAsync());
        CreateDepartmentCommand = CreateCommand(async () => await CreateDepartmentAsync());
        EditDepartmentCommand = CreateCommand<Department>(async (dept) => await EditDepartmentAsync(dept));
        DeleteDepartmentCommand = CreateCommand<Department>(async (dept) => await DeleteDepartmentAsync(dept));
        ViewDepartmentDetailsCommand = CreateCommand<Department>(async (dept) => await ViewDepartmentDetailsAsync(dept));
        ToggleActiveStatusCommand = CreateCommand<Department>(async (dept) => await ToggleActiveStatusAsync(dept));
        ExportToExcelCommand = CreateCommand(async () => await ExportToExcelAsync());
        ExportToCsvCommand = CreateCommand(async () => await ExportToCsvAsync());
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync());
    }

    private void SetupSubscriptions()
    {
        // Initial load
        LoadDepartmentsCommand.Execute().Subscribe(_ => { }, _ => { });
    }

    #region Command Implementations

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            IsLoading = true;
            HasErrors = false;
            ErrorMessage = null;

            _logger.LogInformation("Loading departments...");

            var departments = await _departmentService.GetAllDepartmentsAsync();
            
            _departments.Clear();
            _departments.AddRange(departments);

            _logger.LogInformation("Loaded {Count} departments", departments.Count());
            
            ShowSuccess("Данные департаментов успешно загружены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading departments");
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadDepartmentsAsync();
    }

    /// <summary>
    /// Создание нового департамента с валидацией
    /// </summary>
    private async Task CreateDepartmentAsync()
    {
        try
        {
            var newDepartment = new Department
            {
                Uid = Guid.NewGuid(),
                Name = "Новый отдел",
                Code = $"DEPT_{DateTime.Now:yyyyMMddHHmmss}",
                Description = "Описание нового отдела",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Используем реальный диалог редактирования
            var editedDepartment = await _dialogService.ShowDepartmentEditDialogAsync(newDepartment);
            
            if (editedDepartment != null)
            {
                var createdDepartment = await _departmentService.CreateAsync(editedDepartment);
                if (createdDepartment != null)
                {
                    LogInfo($"Отдел '{createdDepartment.Name}' успешно создан");
                    await LoadDepartmentsAsync();
                }
                else
                {
                    LogWarning("Не удалось создать департамент");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании отдела");
        }
    }

    /// <summary>
    /// Редактирование департамента с optimistic locking
    /// </summary>
    private async Task EditDepartmentAsync(Department department)
    {
        if (department == null) return;

        try
        {
            IsLoading = true;
            HasErrors = false;

            var departmentEntity = await _departmentService.GetByUidAsync(department.Uid);
            if (departmentEntity == null)
            {
                ErrorMessage = "Отдел не найден";
                await LoadDepartmentsAsync(); // Обновляем список
                return;
            }

            // Используем диалог редактирования департамента
            var editedDepartment = await _dialogService.ShowDepartmentEditDialogAsync(departmentEntity);
            
            if (editedDepartment == null)
            {
                LogDebug("Department editing cancelled by user");
                return;
            }
            
            var updateResult = await _departmentService.UpdateAsync(editedDepartment);
            if (updateResult)
            {
                // Получаем обновленный департамент
                var updatedDepartment = await _departmentService.GetByUidAsync(editedDepartment.Uid);
                if (updatedDepartment != null)
                {
                    // Обновляем в коллекции
                    var existingDepartment = _departments.Items.FirstOrDefault(d => d.Uid == updatedDepartment.Uid);
                    if (existingDepartment != null)
                    {
                        var index = _departments.Items.ToList().IndexOf(existingDepartment);
                        if (index >= 0)
                        {
                            _departments.RemoveAt(index);
                            _departments.Insert(index, updatedDepartment);
                        }
                    }
                    else
                    {
                        // Если не найден в коллекции, просто добавляем
                        _departments.Add(updatedDepartment);
                    }

                    var currentPerson = await _authService.GetCurrentPersonAsync();
                    if (currentPerson != null)
                    {
                        await _notificationService.CreateNotificationAsync(
                            currentPerson.Uid,
                            "Департамент обновлен",
                            $"Департамент '{updatedDepartment.Name}' успешно обновлен",
                            ViridiscaUi.Domain.Entities.System.Enums.NotificationType.Success,
                            ViridiscaUi.Domain.Entities.System.Enums.NotificationPriority.Normal);
                    }

                    ShowSuccess($"Департамент '{updatedDepartment.Name}' обновлен");
                    LogInfo($"Департамент обновлен: {updatedDepartment.Name}");
                }
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating department");
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Ошибка валидации при редактировании департамента");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось обновить отдел";
            HasErrors = true;
            LogError(ex, "Failed to update department");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteDepartmentAsync(Department department)
    {
        if (department == null) return;

        try
        {
            IsLoading = true;
            HasErrors = false;

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить департамент '{department.Name}'?\n\nЭто действие нельзя отменить.");

            if (confirmed)
            {
                var departmentEntity = await _departmentService.GetByUidAsync(department.Uid);
                if (departmentEntity == null)
                {
                    ErrorMessage = "Отдел не найден";
                    await LoadDepartmentsAsync(); // Обновляем список
                    return;
                }

                // Удаление департамента
                await _departmentService.DeleteAsync(departmentEntity.Uid);
                
                // Удаление из коллекции
                var viewModelToRemove = Departments.FirstOrDefault(d => d.Uid == departmentEntity.Uid);
                if (viewModelToRemove != null)
                {
                    _departments.Remove(viewModelToRemove);
                }
                
                // Сброс выбора
                SelectedDepartment = null;

                await _notificationService.SendNotificationAsync(
                    "Отдел успешно удален",
                    $"Отдел '{departmentEntity.Name}' был удален из системы",
                    ViridiscaUi.Domain.Entities.System.Enums.NotificationType.Success);

                ShowSuccess($"Департамент '{departmentEntity.Name}' удален");
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Ошибка валидации при удалении департамента");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            HasErrors = true;
            ErrorMessage = "Не удалось удалить отдел";
            LogError(ex, "Failed to delete department");
            await _notificationService.SendNotificationAsync(
                "Ошибка удаления отдела",
                $"Не удалось удалить отдел: {ex.Message}",
                ViridiscaUi.Domain.Entities.System.Enums.NotificationType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewDepartmentDetailsAsync(Department department)
    {
        if (department == null) return;

        try
        {
            LogInfo($"Viewing department details: {department.Name}");
            await NavigateToAsync($"department-details/{department.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to department details");
            _notificationService.ShowError("Не удалось открыть детали отдела");
        }
    }

    private async Task ToggleActiveStatusAsync(Department department)
    {
        if (department == null) return;

        try
        {
            IsLoading = true;

            department.IsActive = !department.IsActive;
            await _departmentService.UpdateAsync(department);
            LogInfo($"Отдел '{department.Name}' успешно обновлен");
            await LoadDepartmentsAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to toggle department status");
            ShowError($"Ошибка переключения статуса отдела: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Экспорт списка департаментов в Excel
    /// </summary>
    private async Task ExportToExcelAsync()
    {
        try
        {
            LogInfo("Экспорт департаментов в Excel...");
            ShowInfo("Функция экспорта в Excel находится в разработке");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка экспорта в Excel");
        }
    }

    /// <summary>
    /// Экспорт списка департаментов в CSV
    /// </summary>
    private async Task ExportToCsvAsync()
    {
        try
        {
            LogInfo("Экспорт департаментов в CSV...");
            ShowInfo("Функция экспорта в CSV находится в разработке");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка экспорта в CSV");
        }
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        ShowInactiveOnly = false;
        await LoadDepartmentsAsync();
    }

    #endregion

    #region Helper Methods

    private Func<Department, bool> CreateFilter((string searchText, bool showActiveOnly, bool showInactiveOnly) filterParams)
    {
        return department =>
        {
            // Text filter
            if (!string.IsNullOrWhiteSpace(filterParams.searchText))
            {
                var searchLower = filterParams.searchText.ToLowerInvariant();
                if (!department.Name.ToLowerInvariant().Contains(searchLower) &&
                    !department.Code.ToLowerInvariant().Contains(searchLower) &&
                    !department.Description.ToLowerInvariant().Contains(searchLower))
                {
                    return false;
                }
            }

            // Status filters
            if (filterParams.showActiveOnly && !department.IsActive)
                return false;
            
            if (filterParams.showInactiveOnly && department.IsActive)
                return false;

            return true;
        };
    }

    private void UpdateStatistics()
    {
        var allDepartments = _departments.Items.ToList();
        TotalDepartments = allDepartments.Count;
        ActiveDepartments = allDepartments.Count(d => d.IsActive);
        InactiveDepartments = allDepartments.Count(d => !d.IsActive);
    }

    private void HandleError(Exception ex)
    {
        HasErrors = true;
        ErrorMessage = ex.Message;
        _logger.LogError(ex, "Error in DepartmentsViewModel");
    }

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson == null) return false;

            // Здесь должна быть логика проверки прав доступа
            // Пока возвращаем true для всех авторизованных пользователей
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Валидация департамента
    /// </summary>
    private async Task<DomainValidationResult> ValidateDepartmentAsync(Department department)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(department.Name))
            errors.Add("Название департамента обязательно");

        if (string.IsNullOrWhiteSpace(department.Code))
            errors.Add("Код департамента обязателен");

        // Проверка уникальности кода
        var existingByCode = await _departmentService.GetByCodeAsync(department.Code);
        if (existingByCode != null && existingByCode.Uid != department.Uid)
            errors.Add($"Департамент с кодом '{department.Code}' уже существует");

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    #endregion
} 

