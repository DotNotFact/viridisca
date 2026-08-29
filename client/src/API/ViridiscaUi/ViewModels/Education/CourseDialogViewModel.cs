using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования курса
/// </summary>
public class CourseDialogViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;

    #region Properties

    [Reactive] public Guid CourseUid { get; set; }
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public int Credits { get; set; }
    [Reactive] public CourseType Type { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public Department? Department { get; set; }
    [Reactive] public Course? Entity { get; set; }

    #endregion

    #region Collections

    public ObservableCollection<Department> AvailableDepartments { get; } = new();

    #endregion

    #region Constructor

    public CourseDialogViewModel(
        ICourseService courseService,
        ILogger<CourseDialogViewModel> logger,
        IDialogService dialogService)
        : base(logger, dialogService)
    {
        _courseService = courseService;

        CourseUid = Guid.NewGuid();
        Credits = 3;
        Type = CourseType.Core;

        // Настройка валидации
        this.WhenAnyValue(
                x => x.CourseName,
                x => x.Code,
                x => x.Credits,
                x => x.Department)
            .Subscribe(_ => ValidateInternal())
            .DisposeWith(Disposables);
    }

    #endregion

    #region Overrides

    protected void InitializeFromEntity(Course entity)
    {
        Entity = entity;
        CourseName = entity.Name ?? string.Empty;
        Code = entity.Code ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        Credits = entity.Credits;
        Type = entity.Type;
        IsActive = entity.IsActive;

        Title = "Редактирование курса";
    }

    protected void InitializeNew()
    {
        Entity = null;
        CourseUid = Guid.NewGuid();
        CourseName = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        Credits = 3;
        Type = CourseType.Core;
        IsActive = true;
        Department = null;

        IsEditMode = false;
        Title = "Новый курс";
        LogDebug($"Initialized new course dialog: {CourseUid}");
    }

    protected async Task<Course?> SaveEntityAsync()
    {
        Entity ??= new Course();

        Entity.Name = CourseName;
        Entity.Code = Code;
        Entity.Description = Description;
        Entity.Credits = Credits;
        Entity.Type = Type;
        Entity.IsActive = IsActive;

        if (Department != null)
        {
            Entity.DepartmentUid = Department.Uid;
        }

        Entity.LastModifiedAt = DateTime.UtcNow;

        if (IsEditMode)
        {
            var updated = await _courseService.UpdateAsync(Entity);
            return updated ? Entity : null;
        }
        else
        {
            return await _courseService.CreateAsync(Entity);
        }
    }

    #endregion

    /// <summary>
    /// Загружает данные курса для редактирования
    /// </summary>
    public async Task LoadCourseAsync(Guid courseUid)
    {
        try
        {
            var course = await _courseService.GetByUidAsync(courseUid);
            if (course == null)
            {
                throw new InvalidOperationException($"Курс с ID {courseUid} не найден");
            }

            InitializeFromEntity(course);
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки курса {CourseUid}");
            throw;
        }
    }

    /// <summary>
    /// Загружает данные курса для редактирования (перегрузка для объекта Course)
    /// </summary>
    public void LoadCourseAsync(Course course)
    {
        if (course == null) return;

        Entity = course;
        CourseUid = course.Uid;
        CourseName = course.Name;
        Code = course.Code;
        Description = course.Description ?? string.Empty;
        Credits = course.Credits;
        Type = course.Type;
        IsActive = course.IsActive;
        Department = course.Department;

        IsEditMode = true;
        Title = "Редактирование курса";
        LogDebug($"Loaded course for editing: {CourseUid}");
    }

    /// <summary>
    /// Получает обновленный курс после сохранения
    /// </summary>
    public Course? GetUpdatedCourse()
    {
        if (!Validate()) return null;

        if (Entity == null)
        {
            return new Course
            {
                Uid = CourseUid,
                Name = CourseName,
                Code = Code,
                Description = Description,
                Credits = Credits,
                Type = Type,
                IsActive = IsActive,
                DepartmentUid = Department?.Uid
            };
        }
        else
        {
            Entity.Name = CourseName;
            Entity.Code = Code;
            Entity.Description = Description;
            Entity.Credits = Credits;
            Entity.Type = Type;
            Entity.IsActive = IsActive;
            Entity.DepartmentUid = Department?.Uid;
            return Entity;
        }
    }

    #region Methods

    public async Task LoadAvailableData()
    {
        try
        {
            SetLoading(true, "Загрузка данных...");

            // Загружаем департаменты - исправим это позже когда будет правильный сервис
            // var departments = await _departmentService.GetAllAsync();
            // AvailableDepartments.Clear();
            // foreach (var department in departments)
            // {
            //     AvailableDepartments.Add(department);
            // }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error loading available data for course dialog");
            _dialogService?.ShowErrorAsync("Ошибка загрузки данных", ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ValidateInternal()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(CourseName))
            errors.Add("Название обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код обязателен для заполнения");

        if (Credits <= 0)
            errors.Add("Количество кредитов должно быть больше 0");

        if (Department == null)
            errors.Add("Необходимо выбрать кафедру");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    #endregion
}

