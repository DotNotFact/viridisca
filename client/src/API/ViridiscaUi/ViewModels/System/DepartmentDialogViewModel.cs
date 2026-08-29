using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.System;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для диалога создания/редактирования отделения
/// </summary>
public class DepartmentDialogViewModel : ViewModelBase
{
    #region Properties

    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; } = string.Empty;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public string ValidationError { get; set; } = string.Empty;
    [Reactive] public Guid? ParentDepartmentUid { get; set; }

    /// <summary>
    /// Текущая редактируемая сущность
    /// </summary>
    [Reactive] public Department? Entity { get; set; }

    [Reactive] public Department? SelectedParentDepartment { get; set; }

    public ObservableCollection<Department> ParentDepartments { get; } = [];
    public ObservableCollection<Department> AvailableDepartments 
    { 
        get => _availableDepartments; 
        set => this.RaiseAndSetIfChanged(ref _availableDepartments, value); 
    }

    /// <summary>
    /// UID департамента
    /// </summary>
    [Reactive] public Guid DepartmentUid { get; set; }

    /// <summary>
    /// Команды
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateParentDepartmentCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadParentDepartmentsCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion

    #region Fields

    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DepartmentDialogViewModel> _logger;
    private ObservableCollection<Department> _availableDepartments = new();

    #endregion

    public DepartmentDialogViewModel(
        IScreen? hostScreen,
        IDepartmentService departmentService,
        IDialogService dialogService,
        ILogger<DepartmentDialogViewModel> logger) : base(logger, dialogService)
    {
        _departmentService = departmentService;
        _dialogService = dialogService;
        _logger = logger;

        CreateParentDepartmentCommand = ReactiveCommand.CreateFromTask(CreateParentDepartmentAsync);
        LoadParentDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadParentDepartmentsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);

        LoadParentDepartmentsAsync();
    }

    #region Methods

    /// <summary>
    /// Инициализирует ViewModel из сущности Department
    /// </summary>
    protected void InitializeFromEntity(Department entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        
        Entity = entity;
        Name = entity.Name ?? string.Empty;
        Code = entity.Code ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        IsActive = entity.IsActive;

        Title = "Редактирование отделения";
    }

    /// <summary>
    /// Валидирует данные отделения
    /// </summary>
    protected void Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название отделения обязательно для заполнения");
            
        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код отделения обязателен для заполнения");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    /// <summary>
    /// Сохраняет данные в сущность Department
    /// </summary>
    protected async Task<Department?> SaveEntityAsync()
    {
        try
        {
            var entity = Entity ?? new Department();
            
            entity.Name = Name;
            entity.Code = Code;
            entity.Description = Description;
            entity.IsActive = IsActive;
            
            entity.LastModifiedAt = DateTime.UtcNow;
        
        if (entity.Uid == Guid.Empty)
        {
            entity.Uid = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            }

            return entity;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при сохранении отделения: {ex.Message}", "DepartmentDialog");
            return null;
        }
    }

    private async Task CreateParentAsync()
    {
        try
        {
            var dialog = new DepartmentDialogViewModel(null, _departmentService, _dialogService, _logger);
            // Note: This would need to be implemented in the calling code
            // var result = await _dialogService.ShowDialogAsync(dialog);
            
            // For now, just create a placeholder
            var newDepartment = new Department
            {
                Name = "Новый департамент",
                Code = "NEW",
                Description = "Новый департамент"
            };
            
            var result = await _departmentService.CreateAsync(newDepartment);
            if (result != null)
            {
                AvailableDepartments.Add(result);
                SelectedParentDepartment = result;
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Ошибка создания родительской кафедры");
        }
    }

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания нового департамента
    /// </summary>
    public async Task InitializeAsync()
    {
        Initialize();
        await LoadParentDepartmentsAsync();
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующего департамента
    /// </summary>
    public async Task InitializeAsync(Department department)
    {
        Initialize(department);
        await LoadParentDepartmentsAsync();
    }

    #endregion

    private void InitializeCommands()
    {
        // Команды уже инициализированы в конструкторе
    }

    public void LoadDepartment(Department department)
    {
        Entity = department;
        DepartmentUid = department.Uid;
        Name = department.Name;
        Description = department.Description;
        SelectedParentDepartment = ParentDepartments.FirstOrDefault(d => d.Uid == department.ParentDepartmentUid);
        IsEditMode = true;
        Title = "Редактирование департамента";
    }

    public void InitializeNew()
    {
        Entity = null;
        DepartmentUid = Guid.NewGuid();
        Name = string.Empty;
        Description = string.Empty;
        SelectedParentDepartment = null;
        IsEditMode = false;
        Title = "Новый департамент";
    }

    private async Task SaveAsync()
    {
        try
        {
            if (Entity == null)
            {
                // Создание нового департамента
                var newDepartment = new Department
                {
                    Uid = DepartmentUid == Guid.Empty ? Guid.NewGuid() : DepartmentUid,
                    Name = Name ?? string.Empty,
                    Description = Description,
                    ParentDepartmentUid = SelectedParentDepartment?.Uid,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _departmentService.CreateAsync(newDepartment);
                Entity = newDepartment;
            }
            else
            {
                // Обновление существующего департамента
                Entity.Name = Name ?? string.Empty;
                Entity.Description = Description;
                Entity.ParentDepartmentUid = SelectedParentDepartment?.Uid;
                Entity.LastModifiedAt = DateTime.UtcNow;
                
                await _departmentService.UpdateAsync(Entity);
            }
            
            // Закрыть диалог с результатом OK
            // TODO: Implement dialog result
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения департамента");
            ValidationError = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private async Task CancelAsync()
    {
        // Логика отмены
    }

    private void Initialize(Department? department = null)
    {
        if (department == null)
        {
            // Инициализация для нового департамента
            Entity = null;
            DepartmentUid = Guid.NewGuid();
            Name = string.Empty;
            Code = string.Empty;
            Description = string.Empty;
            IsActive = true;
            ParentDepartmentUid = null;
            SelectedParentDepartment = null;

            IsEditMode = false;
            Title = "Новый департамент";
        }
        else
        {
            // Инициализация для редактирования
            Entity = department;
            DepartmentUid = department.Uid;
            Name = department.Name;
            Code = department.Code ?? string.Empty;
            Description = department.Description;
            IsActive = department.IsActive;
            ParentDepartmentUid = department.ParentDepartmentUid;

            IsEditMode = true;
            Title = "Редактирование департамента";
        }
    }

    private async Task CreateParentDepartmentAsync()
    {
        // TODO: Implement parent department creation
        await Task.CompletedTask;
    }

    private async Task LoadParentDepartmentsAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllAsync();
            ParentDepartments.Clear();
            foreach (var dept in departments)
            {
                ParentDepartments.Add(dept);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки родительских департаментов");
        }
    }

    private void Cancel()
    {
        // Закрыть диалог с результатом Cancel
        // TODO: Implement dialog result
    }

    #endregion
}

