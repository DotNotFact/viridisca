using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

public class SubjectDialogViewModel : RoutableViewModelBase
{
    private readonly ISubjectService _subjectService; 
    private readonly IDepartmentService _departmentService;
    
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public Department? Department { get; set; }
    [Reactive] public int Credits { get; set; }
    [Reactive] public int Hours { get; set; }
    [Reactive] public Subject? Entity { get; set; }
    [Reactive] public string Title { get; set; } = "Новый предмет";
    [Reactive] public bool IsEditMode { get; set; }
    [Reactive] public string? ValidationError { get; set; }

    public ObservableCollection<Department> AvailableDepartments { get; } = new();
    public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SubjectDialogViewModel(
        IScreen? hostScreen,
        ISubjectService subjectService,
        IDepartmentService departmentService,
        ILogger<SubjectDialogViewModel> logger)
        : base(hostScreen)
    {
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService)); 
        
        // Инициализация команд
        LoadDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);
        
        // Загрузка данных
        LoadDepartmentsAsync().ConfigureAwait(false);
        
        Credits = 3;
        Hours = 48;
        
        // Настройка валидации
        this.WhenAnyValue(
                x => x.Code,
                x => x.Name,
                x => x.Department,
                x => x.Credits,
                x => x.Hours)
            .Subscribe(_ => ValidateInternal())
            .DisposeWith(Disposables);
    }

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания нового предмета
    /// </summary>
    public async Task InitializeAsync()
    {
        Initialize(null);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующего предмета
    /// </summary>
    public async Task InitializeAsync(Subject subject)
    {
        Initialize(subject);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Загружает данные предмета для редактирования
    /// </summary>
    public async Task LoadSubjectAsync(Guid subjectUid)
    {
        try
        {
            var subject = await _subjectService.GetByUidAsync(subjectUid);
            if (subject == null)
            {
                throw new InvalidOperationException($"Предмет с ID {subjectUid} не найден");
            }

            InitializeFromEntity(subject);
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки предмета");
            throw;
        }
    }

    /// <summary>
    /// Загружает данные предмета для редактирования (перегрузка для объекта Subject)
    /// </summary>
    public async Task LoadSubjectAsync(Subject subject)
    {
        try
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            // Если нужна полная загрузка связанных данных, загружаем из сервиса.
            // A blank "new subject" template (from the "Создать предмет" button) has a fresh
            // random Uid that was never persisted, so GetByUidAsync legitimately returns null -
            // that's the expected "create" case, not an error. Populate from the passed-in
            // template directly instead of throwing.
            var fullSubject = await _subjectService.GetByUidAsync(subject.Uid);
            if (fullSubject == null)
            {
                InitializeFromEntity(subject);
                IsEditMode = false;
                return;
            }

            InitializeFromEntity(fullSubject);
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки предмета");
            throw;
        }
    }

    /// <summary>
    /// Получает обновленный предмет после сохранения
    /// </summary>
    public Subject? GetUpdatedSubject()
    {
        return Entity;
    }

    #endregion

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            SetLoading(true, "Загрузка департаментов...");
            
            var departments = await _departmentService.GetAllAsync();
            
            AvailableDepartments.Clear();
            foreach (var department in departments)
            {
                AvailableDepartments.Add(department);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error loading departments for subject dialog");
            ShowError("Ошибка загрузки данных");
        }
        finally
        {
            SetLoading(false);
        }
    }

    protected void InitializeFromEntity(Subject entity)
    {
        Entity = entity;
        Code = entity.Code ?? string.Empty;
        Name = entity.Name ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        Department = entity.Department;
        Credits = entity.Credits;
        Hours = entity.LessonsPerWeek * 16; // Примерный расчет часов
        
        Title = "Редактирование предмета";
    }

    protected void InitializeNew()
    {
        Entity = new Subject();
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Department = null;
        Credits = 3;
        Hours = 48;
        
        Title = "Создание предмета";
    }

    protected async Task<Subject?> SaveEntityAsync()
    {
        if (Entity == null)
        {
            Entity = new Subject();
        }

        Entity.Code = Code;
        Entity.Name = Name;
        Entity.Description = Description;
        Entity.DepartmentUid = Department?.Uid;
        Entity.Credits = Credits;
        Entity.LessonsPerWeek = Hours / 16; // Примерный расчет занятий в неделю
        Entity.LastModifiedAt = DateTime.UtcNow;

        if (IsEditMode)
        {
            return await _subjectService.UpdateAsync(Entity);
        }
        else
        {
            return await _subjectService.CreateAsync(Entity);
        }
    }

    private void ValidateInternal()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Введите код предмета");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Введите название предмета");

        if (Department == null)
            errors.Add("Выберите кафедру");

        if (Credits <= 0)
            errors.Add("Количество кредитов должно быть больше 0");

        if (Hours <= 0)
            errors.Add("Количество часов должно быть больше 0");

        if (Description?.Length > 1000)
            errors.Add("Описание не должно превышать 1000 символов");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    private void Initialize(Subject? subject)
    {
        if (subject == null)
        {
            InitializeNew();
            IsEditMode = false;
        }
        else
        {
            InitializeFromEntity(subject);
            IsEditMode = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(ValidationError))
            {
                return;
            }

            SetLoading(true, "Сохранение предмета...");
            Entity = await SaveEntityAsync();
            
            if (HostScreen is { } screen)
            {
                await screen.Router.NavigateBack.Execute();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка сохранения предмета");
            ShowError("Не удалось сохранить предмет");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void Cancel()
    {
        if (HostScreen is { } screen)
        {
            screen.Router.NavigateBack.Execute().Subscribe();
        }
    }
} 

