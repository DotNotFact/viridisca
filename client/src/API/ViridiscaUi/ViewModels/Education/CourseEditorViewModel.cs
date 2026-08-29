using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Navigations;
using ViridiscaUi.Navigations;
using ViridiscaUi.Models.Collections;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для создания и редактирования курсов
/// </summary>
[Route("course-editor", DisplayName = "Редактор курсов", IconKey = "📚", Order = 302, RequiredRoles = new[] { "Admin", "Teacher" })]
public class CourseEditorViewModel : RoutableViewModelBase
{
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly ITeacherService _teacherService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly ObservableCollection<Teacher> _teachers = [];
    private ReadOnlyObservableCollection<Teacher> _teachersReadOnly;

    public ReadOnlyObservableCollection<Teacher> Teachers => _teachersReadOnly;

    /// <summary>
    /// Флаг режима редактирования (true) или создания (false)
    /// </summary>
    [Reactive] public bool IsEditMode { get; set; }

    /// <summary>
    /// Текущий редактируемый курс
    /// </summary>
    [Reactive] public CourseInstance? CurrentCourseInstance { get; set; }

    /// <summary>
    /// Идентификатор курса для редактирования
    /// </summary>
    [Reactive] public Guid? CourseInstanceId { get; set; }

    // Поля для редактирования
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string? Category { get; set; } = string.Empty;
    [Reactive] public Teacher? SelectedTeacher { get; set; }
    [Reactive] public DateTime StartDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(4);
    [Reactive] public int Credits { get; set; } = 3;
    [Reactive] public CourseStatus SelectedStatus { get; set; } = CourseStatus.Draft;
    [Reactive] public string Prerequisites { get; set; } = string.Empty;
    [Reactive] public string LearningOutcomes { get; set; } = string.Empty;
    [Reactive] public int MaxEnrollments { get; set; } = 30;

    /// <summary>
    /// Доступные преподаватели для выбора
    /// </summary>
    [Reactive] public ObservableCollection<Teacher> AvailableTeachers { get; set; } = new();

    /// <summary>
    /// Доступные статусы курса
    /// </summary>
    [Reactive] public ObservableCollection<CourseStatus> AvailableStatuses { get; set; } = new();

    /// <summary>
    /// Предопределенные категории курсов
    /// </summary>
    [Reactive] public ObservableCollection<string> AvailableCategories { get; set; } = new();

    /// <summary>
    /// Флаг процесса сохранения
    /// </summary>
    [Reactive] public bool IsSaving { get; set; }

    /// <summary>
    /// Заголовок формы
    /// </summary>
    [Reactive] public string FormTitle { get; set; } = "Создание курса";

    [ObservableAsProperty] public bool IsLoading { get; }
    [ObservableAsProperty] public bool IsValid { get; }
    [ObservableAsProperty] public bool CanSave { get; }

    // Commands
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateNewCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> GenerateCodeCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> EditCommand { get; set; } = null!;
    public ReactiveCommand<Unit, Unit> CloseCommand { get; set; } = null!;

    public string Title => CurrentCourseInstance == null ? "Добавить курс" : "Редактировать курс";

    [Reactive] public ObservableCollection<Subject> AvailableSubjects { get; set; } = new();
    [Reactive] public ObservableCollection<Department> AvailableDepartments { get; set; } = new();
    [Reactive] public ObservableCollection<AcademicPeriod> AvailableAcademicPeriods { get; set; } = new();

    // Computed properties
    public int TeacherCount => AvailableTeachers?.Count ?? 0;
    public string CourseName => Name ?? string.Empty;

    public CourseEditorViewModel(
        ICourseInstanceService courseInstanceService,
        ITeacherService teacherService,
        IUnifiedNavigationService navigationService,
        IScreen hostScreen) : base(hostScreen)
    {
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        _teachersReadOnly = new ReadOnlyObservableCollection<Teacher>(_teachers);

        InitializeCommands();
        InitializePredefinedValues();
    }

    private void InitializeCommands()
    {
        // Проверка валидности формы
        var canSave = this.WhenAnyValue(
            x => x.Name,
            x => x.Code,
            x => x.Description,
            x => x.SelectedTeacher,
            x => x.StartDate,
            x => x.EndDate,
            x => x.IsSaving,
            (name, code, description, teacher, startDate, endDate, isSaving) =>
                !string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(code) &&
                !string.IsNullOrWhiteSpace(description) &&
                teacher != null &&
                startDate < endDate &&
                !isSaving);

        SaveCommand = CreateCommand(SaveAsync, canSave, "Ошибка при сохранении курса");
        CancelCommand = CreateCommand(CancelAsync, null, "Ошибка при отмене");
        
        var canDelete = this.WhenAnyValue(x => x.IsEditMode, x => x.IsSaving, 
            (isEdit, isSaving) => isEdit && !isSaving);
        DeleteCommand = CreateCommand(DeleteAsync, canDelete, "Ошибка при удалении курса");
        
        CreateNewCommand = CreateCommand(CreateNewAsync, null, "Ошибка при создании нового курса");
        GenerateCodeCommand = CreateCommand(GenerateCodeAsync, null, "Ошибка при генерации кода");
        
        EditCommand = CreateCommand(EditAsync, null, "Ошибка при редактировании курса");
        CloseCommand = CreateCommand(CloseAsync, null, "Ошибка при закрытии курса");
    }

    private void InitializePredefinedValues()
    {
        // Инициализируем статусы курса
        AvailableStatuses.Clear();
        foreach (var status in Enum.GetValues<CourseStatus>())
        {
            AvailableStatuses.Add(status);
        }

        // Инициализируем категории курсов
        AvailableCategories.Clear();
        var categories = new[]
        {
            "Программирование",
            "Веб-разработка",
            "Базы данных",
            "Системное администрирование",
            "Дизайн",
            "Математика",
            "Физика",
            "Общие дисциплины"
        };

        foreach (var category in categories)
        {
            AvailableCategories.Add(category);
        }
    }

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        
        await LoadTeachersAsync();
        
        if (CurrentCourseInstance != null)
        {
            await LoadCourseInstanceAsync(CurrentCourseInstance.Uid);
        }
        else
        {
            SetupForCreation();
        }
    }

    private async Task LoadTeachersAsync()
    {
        try
        {
            ShowInfo("Загрузка преподавателей...");
            var activeTeachers = await _teacherService.GetAllTeachersAsync();
            
            AvailableTeachers.Clear();
            foreach (var teacher in activeTeachers.Where(t => t.IsActive))
            {
                AvailableTeachers.Add(teacher);
            }
            
            LogInfo($"Loaded {TeacherCount} teachers");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке преподавателей", ex);
        }
    }

    private async Task LoadCourseInstanceAsync(Guid courseInstanceId)
    {
        try
        {
            ShowInfo("Загрузка данных курса...");
            
            var courseInstance = await _courseInstanceService.GetCourseInstanceAsync(courseInstanceId);
            if (courseInstance == null)
            {
                SetError("Курс не найден");
                await _navigationService.GoBackAsync();
                return;
            }

            CurrentCourseInstance = courseInstance;
            PopulateForm(courseInstance);
            
            ShowSuccess("Данные курса загружены");
            LogInfo($"Loaded course: {CourseName}");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке курса", ex);
        }
    }

    private void PopulateForm(CourseInstance courseInstance)
    {
        // Основные свойства
        Name = courseInstance.Name;
        Code = courseInstance.Code;
        Description = courseInstance.Description;
        StartDate = courseInstance.StartDate;
        EndDate = courseInstance.EndDate ?? DateTime.Now.AddMonths(4);
        MaxEnrollments = courseInstance.MaxEnrollments;
        SelectedStatus = courseInstance.Status;

        // Выбираем преподавателя из загруженного списка
        SelectedTeacher = AvailableTeachers.FirstOrDefault(t => t.Uid == courseInstance.TeacherUid);

        // Дополнительные свойства для диалога деталей
        CurrentCourseInstance = courseInstance;
    }

    private void SetupForCreation()
    {
        CurrentCourseInstance = null;
        ClearForm();
        GenerateCode();
    }

    private void ClearForm()
    {
        Name = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        SelectedTeacher = null;
        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddMonths(4);
        Credits = 3;
        SelectedStatus = CourseStatus.Draft;
        Prerequisites = string.Empty;
        LearningOutcomes = string.Empty;
        MaxEnrollments = 30;
    }

    private void GenerateCode()
    {
        var year = DateTime.Now.Year;
        var random = new Random();
        Code = $"COURSE{year % 100:D2}{random.Next(100, 999)}";
    }

    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ClearError();

            if (CurrentCourseInstance != null)
            {
                await UpdateCourseInstanceAsync();
            }
            else
            {
                await CreateCourseInstanceAsync();
            }

            ShowSuccess(CurrentCourseInstance != null ? "Курс обновлен" : "Курс создан");
            
            // Для диалогов не используем навигацию
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("courses");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при сохранении: {ex.Message}", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UpdateCourseInstanceAsync()
    {
        if (CurrentCourseInstance == null || SelectedTeacher == null) return;

        // Обновляем существующий экземпляр курса
        CurrentCourseInstance.Name = Name;
        CurrentCourseInstance.Code = Code;
        CurrentCourseInstance.Description = Description;
        CurrentCourseInstance.StartDate = StartDate;
        CurrentCourseInstance.EndDate = EndDate;
        CurrentCourseInstance.MaxEnrollments = MaxEnrollments;
        CurrentCourseInstance.Status = SelectedStatus;
        CurrentCourseInstance.TeacherUid = SelectedTeacher.Uid;

        var success = await _courseInstanceService.UpdateAsync(CurrentCourseInstance);
        if (success)
        {
            ShowSuccess($"Курс '{CurrentCourseInstance.Name}' обновлен");
            // Для диалогов не используем навигацию
        }
        else
        {
            SetError("Ошибка обновления экземпляра курса");
        }
    }

    private async Task CreateCourseInstanceAsync()
    {
        if (SelectedTeacher == null) return;

        // Создаем новый экземпляр курса
        var newCourseInstance = new CourseInstance
        {
            Name = Name,
            Code = Code,
            Description = Description,
            StartDate = StartDate,
            EndDate = EndDate,
            MaxEnrollments = MaxEnrollments,
            Status = SelectedStatus,
            TeacherUid = SelectedTeacher.Uid
        };

        var createdCourseInstance = await _courseInstanceService.CreateAsync(newCourseInstance);
        if (createdCourseInstance != null)
        {
            ShowSuccess($"Курс '{createdCourseInstance.Name}' создан");
            // Для диалогов не используем навигацию
        }
    }

    private async Task DeleteAsync()
    {
        if (CurrentCourseInstance == null) return;

        try
        {
            IsSaving = true;
            
            // Здесь можно добавить диалог подтверждения
            await _courseInstanceService.DeleteCourseInstanceAsync(CurrentCourseInstance.Uid);
            
            ShowSuccess("Курс удален");
            LogInfo($"Deleted course: {CourseName}");
            
            await _navigationService.NavigateToAsync("courses");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при удалении: {ex.Message}", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CancelAsync()
    {
        // Для диалогов не используем навигацию
        if (_navigationService != null)
        {
            await _navigationService.GoBackAsync();
        }
    }

    private async Task CreateNewAsync()
    {
        SetupForCreation();
        IsEditMode = false;
        FormTitle = "Создание курса";
        ClearError();
    }

    private async Task GenerateCodeAsync()
    {
        GenerateCode();
        ShowInfo("Код курса сгенерирован");
    }

    /// <summary>
    /// Получает текстовое представление статуса курса
    /// </summary>
    public string GetStatusText(CourseStatus status)
    {
        return status switch
        {
            CourseStatus.Draft => "Черновик",
            CourseStatus.Published => "Опубликован",
            CourseStatus.Active => "Активен",
            CourseStatus.Completed => "Завершен",
            CourseStatus.Archived => "В архиве",
            _ => "Неизвестно"
        };
    }

    /// <summary>
    /// Проверяет, можно ли изменить статус курса
    /// </summary>
    public bool CanChangeStatus(CourseStatus newStatus)
    {
        return SelectedStatus switch
        {
            CourseStatus.Draft => newStatus == CourseStatus.Published,
            CourseStatus.Published => newStatus == CourseStatus.Active || newStatus == CourseStatus.Draft,
            CourseStatus.Active => newStatus == CourseStatus.Completed || newStatus == CourseStatus.Archived,
            CourseStatus.Completed => newStatus == CourseStatus.Archived,
            CourseStatus.Archived => false,
            _ => false
        };
    }

    private async Task EditAsync()
    {
        // Команда редактирования - просто уведомляем о том, что нужно перейти к редактированию
        // Логика будет обрабатываться в диалоге
        await Task.CompletedTask;
    }

    private async Task CloseAsync()
    {
        // Команда закрытия - просто уведомляем о том, что нужно закрыть диалог
        // Логика будет обрабатываться в диалоге
        await Task.CompletedTask;
    }

    // Дополнительные свойства для диалога деталей
    [Reactive] public ObservableCollection<Lesson> Lessons { get; set; } = new();
    [Reactive] public ObservableCollection<Enrollment> Enrollments { get; set; } = new();
    [Reactive] public string CourseDuration { get; set; } = string.Empty;
    [Reactive] public bool HasErrors { get; set; }

    /// <summary>
    /// Конструктор для диалогов с упрощенным набором зависимостей
    /// </summary>
    public CourseEditorViewModel(ICourseInstanceService courseInstanceService, CourseInstance? courseInstance = null)
        : base(hostScreen: null!)  // Для диалогов hostScreen не нужен
    {
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _teacherService = null!; // Для диалогов teacherService не нужен
        _navigationService = null!; // Для диалогов навигация не нужна

        _teachersReadOnly = new ReadOnlyObservableCollection<Teacher>(_teachers);

        InitializeCommands();
        InitializePredefinedValues();

        if (courseInstance != null)
        {
            CurrentCourseInstance = courseInstance;
            IsEditMode = true;
            FormTitle = "Редактирование курса";
            PopulateForm(courseInstance);
            LoadModulesAndEnrollments(courseInstance);
        }
        else
        {
            SetupForCreation();
        }
    }

    private void LoadModulesAndEnrollments(CourseInstance courseInstance)
    {
        // Загружаем занятия и записи курса
        Lessons.Clear();
        if (courseInstance.Lessons != null)
        {
            foreach (var lesson in courseInstance.Lessons.OrderBy(l => l.OrderIndex))
            {
                Lessons.Add(lesson);
            }
        }

        Enrollments.Clear();
        if (courseInstance.Enrollments != null)
        {
            foreach (var enrollment in courseInstance.Enrollments.OrderBy(e => e.EnrollmentDate))
            {
                Enrollments.Add(enrollment);
            }
        }

        // Вычисляем продолжительность курса
        if (courseInstance.EndDate != null)
        {
            var duration = courseInstance.EndDate.Value - courseInstance.StartDate;
            CourseDuration = $"{duration.Days} дней ({Math.Round(duration.TotalDays / 7, 1)} недель)";
        }
        else
        {
            CourseDuration = "Не определена";
        }
    }

    [Reactive] public CourseInstance? SelectedCourseInstance { get; set; }

    private async Task<CourseInstance> CreateCourseInstanceFromInputAsync()
    {
        return new CourseInstance
        {
            Uid = SelectedCourseInstance?.Uid ?? Guid.NewGuid(),
            // Map other properties as needed
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    private async Task<bool> ValidateCourseInstanceAsync(CourseInstance courseInstance)
    {
        // TODO: Implement validation logic
        await Task.Delay(1);
        return true;
    }

    private CourseStatus ParseCourseStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return CourseStatus.Draft;

        // Попробуем прямое преобразование
        if (Enum.TryParse<CourseStatus>(status, true, out var parsedStatus))
        {
            return parsedStatus;
        }

        // Попробуем сопоставить по русским названиям
        return status.ToLowerInvariant() switch
        {
            "черновик" => CourseStatus.Draft,
            "активен" or "активный" => CourseStatus.Active,
            "опубликован" or "опубликованный" => CourseStatus.Published,
            "завершен" or "завершенный" => CourseStatus.Completed,
            "архив" or "архивированный" => CourseStatus.Archived,
            "приостановлен" or "приостановленный" => CourseStatus.Suspended,
            "запланирован" or "запланированный" => CourseStatus.Draft,
            "неактивен" or "неактивный" => CourseStatus.Suspended,
            _ => CourseStatus.Draft // Значение по умолчанию
        };
    }

    /// <summary>
    /// Загружает курс для редактирования (для диалогов)
    /// </summary>
    public async Task LoadCourseAsync(Course course)
    {
        if (course == null) return;

        IsEditMode = true;
        FormTitle = "Редактирование курса";

        // Заполняем форму данными курса
        Name = course.Name ?? string.Empty;
        Code = course.Code ?? string.Empty;
        Description = course.Description ?? string.Empty;
        Credits = course.Credits;
        
        // Загружаем преподавателей
        await LoadTeachersAsync();
        
        // Устанавливаем остальные поля
        Category = course.Type.ToString();
        SelectedStatus = CourseStatus.Active; // Устанавливаем по умолчанию
        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddMonths(4);
        MaxEnrollments = 30;
    }
}


