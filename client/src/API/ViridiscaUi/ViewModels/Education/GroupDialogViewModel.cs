using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using DynamicData;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.System;
using ReactiveUI;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования группы
/// </summary>
public class GroupDialogViewModel : RoutableViewModelBase 
{
    #region Properties

    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public int Year { get; set; }
    [Reactive] public GroupStatus Status { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public Department? Department { get; set; }
    [Reactive] public int MaxStudents { get; set; }
    [Reactive] public Curriculum? Curriculum { get; set; }
    [Reactive] public AcademicPeriod? AcademicPeriod { get; set; }
    [Reactive] public ObservableCollection<Department> Departments { get; set; } = new();
    [Reactive] public ObservableCollection<AcademicPeriod> AcademicPeriods { get; set; } = new();
    [Reactive] public string? ValidationError { get; set; }
    [Reactive] public string Title { get; set; } = "Группа";
    [Reactive] public bool IsEditMode { get; set; }
    [Reactive] public Group Group { get; set; } = new();
    [Reactive] public ObservableCollection<Person> Curators { get; set; } = new();
    [Reactive] public Department? SelectedDepartment { get; set; }
    [Reactive] public Person? SelectedCurator { get; set; }

    /// <summary>
    /// UID группы
    /// </summary>
    [Reactive] public Guid GroupUid { get; set; }

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<Department> AvailableDepartments { get; set; } = new();
    [Reactive] public ObservableCollection<Curriculum> AvailableCurricula { get; set; } = new();
    [Reactive] public ObservableCollection<AcademicPeriod> AvailableAcademicPeriods { get; set; } = new();

    #endregion

    private readonly IGroupService _groupService;
    private readonly IDepartmentService _departmentService;
    private readonly ICurriculumService _curriculumService;
    private readonly IAcademicPeriodService _academicPeriodService;
    private readonly IPersonService _personService;
    private readonly IDialogService _dialogService;
    
    protected ViridiscaUi.Domain.Entities.Education.Group? Entity { get; set; } 

    #region Constructor

    public GroupDialogViewModel(
        IScreen? hostScreen,
        IGroupService groupService,
        IDepartmentService departmentService,
        ICurriculumService curriculumService,
        IAcademicPeriodService academicPeriodService,
        IPersonService personService,
        IDialogService dialogService) : base(hostScreen)
    {
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
        _academicPeriodService = academicPeriodService ?? throw new ArgumentNullException(nameof(academicPeriodService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Entity = null;
        Title = "Создание группы";

        // Инициализация значений по умолчанию
        Name = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        IsActive = true;
        MaxStudents = 30;
        Year = 1;

        // Загрузка доступных данных
        LoadAvailableDataAsync().ConfigureAwait(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Инициализирует ViewModel для создания новой группы
    /// </summary>
    public void Initialize()
    {
        Title = "Создание группы";
        IsEditMode = false;
        Group = new Group();
        ValidationError = string.Empty;
    }

    /// <summary>
    /// Инициализирует ViewModel для редактирования группы
    /// </summary>
    /// <param name="group">Группа для редактирования</param>
    public void Initialize(Group group)
    {
        Title = $"Редактирование группы: {group.Name}";
        IsEditMode = true;
        Group = group;
        ValidationError = string.Empty;
        
        // Устанавливаем выбранные значения
        SelectedDepartment = Departments.FirstOrDefault(d => d.Uid == group.DepartmentUid);
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующей группы
    /// </summary>
    public async Task InitializeAsync(ViridiscaUi.Domain.Entities.Education.Group group)
    {
        Initialize(group);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Загружает данные группы для редактирования
    /// </summary>
    public async Task LoadGroupAsync(Guid groupUid)
    {
        try
        {
            var group = await _groupService.GetByUidAsync(groupUid);
            if (group == null)
            {
                throw new InvalidOperationException($"Группа с ID {groupUid} не найдена");
            }

            Entity = group;
            InitializeFromEntity(group);
            
            Title = $"Редактирование группы: {group.Name}";
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки группы {GroupUid}");
            throw;
        }
    }

    /// <summary>
    /// Загружает данные группы для редактирования (перегрузка для объекта Group)
    /// </summary>
    public async Task LoadGroupAsync(ViridiscaUi.Domain.Entities.Education.Group group)
    {
        try
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            // Если нужна полная загрузка связанных данных, загружаем из сервиса.
            // A blank "new group" template has a fresh random Uid that was never persisted,
            // so GetByUidAsync legitimately returns null - that's the expected "create" case,
            // not an error. Populate from the passed-in template directly instead of throwing
            // (same fix already applied to Student/Teacher/Subject dialogs).
            var fullGroup = await _groupService.GetByUidAsync(group.Uid);
            if (fullGroup == null)
            {
                Entity = group;
                InitializeFromEntity(group);
                Title = "Создание группы";
                IsEditMode = false;
                return;
            }

            Entity = fullGroup;
            InitializeFromEntity(fullGroup);

            Title = $"Редактирование группы: {fullGroup.Name}";
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки группы {GroupUid}");
            throw;
        }
    }

    /// <summary>
    /// Получает обновленную группу после сохранения
    /// </summary>
    public ViridiscaUi.Domain.Entities.Education.Group? GetUpdatedGroup()
    {
        return Entity;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Инициализирует ViewModel из сущности Group
    /// </summary>
    protected void InitializeFromEntity(ViridiscaUi.Domain.Entities.Education.Group entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Name = entity.Name;
        Code = entity.Code;
        Description = entity.Description;
        Year = entity.Year;
        Status = entity.Status;
        IsActive = entity.IsActive;
        MaxStudents = entity.MaxStudents;
        
        // Загружаем связанные сущности
        if (entity.Curriculum != null)
            Curriculum = AvailableCurricula.FirstOrDefault(c => c.Uid == entity.Curriculum.Uid);
        if (entity.AcademicPeriod != null)
            AcademicPeriod = AvailableAcademicPeriods.FirstOrDefault(ap => ap.Uid == entity.AcademicPeriod.Uid);
        
        // Загружаем связанную сущность
        if (entity.Department != null)
        {
            Department = AvailableDepartments.FirstOrDefault(d => d.Uid == entity.Department.Uid);
        }
    }

    /// <summary>
    /// Валидирует данные группы
    /// </summary>
    protected bool Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название группы обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код группы обязателен для заполнения");

        if (MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше 0");

        if (Curriculum == null)
            errors.Add("Необходимо выбрать учебный план");

        if (AcademicPeriod == null)
            errors.Add("Необходимо выбрать академический период");

        if (Department == null)
            errors.Add("Необходимо выбрать кафедру");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
        return errors.Count == 0;
    }

    /// <summary>
    /// Сохраняет данные в сущность Group
    /// </summary>
    protected async Task<ViridiscaUi.Domain.Entities.Education.Group?> SaveEntityAsync()
    {
        try
        {
            if (!Validate())
            {
                ValidationError = "Проверьте правильность заполнения полей";
                return null!;
            }

            var entity = Entity ?? new ViridiscaUi.Domain.Entities.Education.Group();
            
            entity.Name = Name;
            entity.Code = Code;
            entity.Description = Description;
            entity.Year = Year;
            entity.Status = Status;
            entity.IsActive = IsActive;
            entity.MaxStudents = MaxStudents;
            entity.CurriculumUid = Curriculum?.Uid ?? Guid.Empty;
            entity.AcademicPeriodUid = AcademicPeriod?.Uid ?? Guid.Empty;
            
            if (Department != null)
            {
                entity.DepartmentUid = Department.Uid;
            }
            
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
            StatusLogger.LogError($"Ошибка сохранения группы: {ex.Message}");
            ValidationError = ex.Message;
            throw;
        }
    }

    /// <summary>
    /// Загружает доступные департаменты
    /// </summary>
    private async Task LoadAvailableDataAsync()
    {
        try
        {
            // Загружаем департаменты
            var departments = await _departmentService.GetAllAsync();
            AvailableDepartments.Clear();
            AvailableDepartments.AddRange(departments);

            // Загружаем учебные планы
            var curricula = await _curriculumService.GetAllAsync();
            AvailableCurricula.Clear();
            AvailableCurricula.AddRange(curricula);

            // Загружаем академические периоды
            var academicPeriods = await _academicPeriodService.GetAllAsync();
            AvailableAcademicPeriods.Clear();
            AvailableAcademicPeriods.AddRange(academicPeriods);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки данных для группы");
            StatusLogger.LogError($"Ошибка загрузки данных для группы: {ex.Message}");
        }
    }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion
} 

