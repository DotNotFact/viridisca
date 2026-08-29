using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для отдельного предмета с полной реактивной поддержкой
/// </summary>
public class SubjectViewModel : ViewModelBase
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public int Credits { get; set; }
    [Reactive] public int LessonsPerWeek { get; set; }
    [Reactive] public SubjectType Type { get; set; }
    [Reactive] public SubjectCategory Category { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Department Properties

    [Reactive] public Guid? DepartmentUid { get; set; }
    [Reactive] public string? DepartmentName { get; set; }
    [Reactive] public string? DepartmentCode { get; set; }

    #endregion

    #region Statistics Properties

    [Reactive] public int CoursesCount { get; set; }
    [Reactive] public int TeachersCount { get; set; }
    [Reactive] public int StudentsCount { get; set; }
    [Reactive] public double AverageGrade { get; set; }
    [Reactive] public int TotalHours { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Отображаемое название типа
    /// </summary>
    public string TypeDisplayName => _typeDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _typeDisplayName;

    /// <summary>
    /// Цвет типа
    /// </summary>
    public string TypeColor => _typeColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _typeColor;

    /// <summary>
    /// Отображаемое название категории
    /// </summary>
    public string CategoryDisplayName => _categoryDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _categoryDisplayName;

    /// <summary>
    /// Цвет категории
    /// </summary>
    public string CategoryColor => _categoryColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _categoryColor;

    /// <summary>
    /// Краткое описание (первые 100 символов)
    /// </summary>
    public string ShortDescription => _shortDescription?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _shortDescription;

    /// <summary>
    /// Статус активности
    /// </summary>
    public string StatusDisplayName => _statusDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusDisplayName;

    /// <summary>
    /// Цвет статуса
    /// </summary>
    public string StatusColor => _statusColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusColor;

    /// <summary>
    /// Отображение департамента с кодом
    /// </summary>
    public string DepartmentDisplay => _departmentDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _departmentDisplay;

    /// <summary>
    /// Полное название предмета
    /// </summary>
    public string FullName => _fullName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _fullName;

    /// <summary>
    /// Количество кредитов в виде строки
    /// </summary>
    public string CreditsDisplay => _creditsDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _creditsDisplay;

    /// <summary>
    /// Количество занятий в неделю в виде строки
    /// </summary>
    public string LessonsPerWeekDisplay => _lessonsPerWeekDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _lessonsPerWeekDisplay;

    /// <summary>
    /// Общее количество часов в виде строки
    /// </summary>
    public string TotalHoursDisplay => _totalHoursDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _totalHoursDisplay;

    /// <summary>
    /// Средний балл в виде строки
    /// </summary>
    public string AverageGradeDisplay => _averageGradeDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _averageGradeDisplay;

    /// <summary>
    /// Статистика курсов
    /// </summary>
    public string CoursesStats => _coursesStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _coursesStats;

    /// <summary>
    /// Статистика преподавателей и студентов
    /// </summary>
    public string TeachingStats => _teachingStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _teachingStats;

    /// <summary>
    /// Есть ли департамент
    /// </summary>
    public bool HasDepartment => _hasDepartment?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasDepartment;

    /// <summary>
    /// Есть ли описание
    /// </summary>
    public bool HasDescription => _hasDescription?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasDescription;

    /// <summary>
    /// Можно ли редактировать предмет
    /// </summary>
    public bool CanEdit => _canEdit?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEdit;

    /// <summary>
    /// Можно ли удалить предмет
    /// </summary>
    public bool CanDelete => _canDelete?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDelete;

    /// <summary>
    /// Можно ли деактивировать предмет
    /// </summary>
    public bool CanDeactivate => _canDeactivate?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDeactivate;

    /// <summary>
    /// Можно ли активировать предмет
    /// </summary>
    public bool CanActivate => _canActivate?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canActivate;

    #endregion

    #region Validation Properties

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public string ValidationErrors => _validationErrors?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _validationErrors;

    /// <summary>
    /// Валиден ли предмет
    /// </summary>
    public bool IsValid => _isValid?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isValid;

    #endregion

    private readonly ISubjectService? _subjectService;
    private readonly IDepartmentService? _departmentService;

    #region Constructor

    public SubjectViewModel()
    {
        // Initialize computed properties
        InitializeComputedProperties();
    }

    public SubjectViewModel(
        ISubjectService? subjectService = null,
        IDepartmentService? departmentService = null)
    {
        _subjectService = subjectService;
        _departmentService = departmentService;
        
        // Initialize computed properties
        InitializeComputedProperties();
    }

    public SubjectViewModel(Subject subject) : this()
    {
        UpdateFromSubject(subject);
    }

    private void InitializeComputedProperties()
    {
        // Настройка computed properties
        _typeDisplayName = this.WhenAnyValue(x => x.Type)
            .Select(GetTypeDisplayName)
            .ToProperty(this, nameof(TypeDisplayName));

        _typeColor = this.WhenAnyValue(x => x.Type)
            .Select(GetTypeColor)
            .ToProperty(this, nameof(TypeColor));

        _categoryDisplayName = this.WhenAnyValue(x => x.Category)
            .Select(GetCategoryDisplayName)
            .ToProperty(this, nameof(CategoryDisplayName));

        _categoryColor = this.WhenAnyValue(x => x.Category)
            .Select(GetCategoryColor)
            .ToProperty(this, nameof(CategoryColor));

        _shortDescription = this.WhenAnyValue(x => x.Description)
            .Select(desc => string.IsNullOrEmpty(desc) ? string.Empty : 
                desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc)
            .ToProperty(this, nameof(ShortDescription));

        _statusDisplayName = this.WhenAnyValue(x => x.IsActive)
            .Select(active => active ? "Активный" : "Неактивный")
            .ToProperty(this, nameof(StatusDisplayName));

        _statusColor = this.WhenAnyValue(x => x.IsActive)
            .Select(active => active ? "#4CAF50" : "#9E9E9E")
            .ToProperty(this, nameof(StatusColor));

        _departmentDisplay = this.WhenAnyValue(x => x.DepartmentName, x => x.DepartmentCode)
            .Select(tuple => GetDepartmentDisplay(tuple.Item1, tuple.Item2))
            .ToProperty(this, nameof(DepartmentDisplay));

        _fullName = this.WhenAnyValue(x => x.Name, x => x.Code)
            .Select(tuple => string.IsNullOrEmpty(tuple.Item2) ? tuple.Item1 : $"{tuple.Item1} ({tuple.Item2})")
            .ToProperty(this, nameof(FullName));

        _creditsDisplay = this.WhenAnyValue(x => x.Credits)
            .Select(credits => $"{credits} кр.")
            .ToProperty(this, nameof(CreditsDisplay));

        _lessonsPerWeekDisplay = this.WhenAnyValue(x => x.LessonsPerWeek)
            .Select(lessons => $"{lessons} ч/нед")
            .ToProperty(this, nameof(LessonsPerWeekDisplay));

        _totalHoursDisplay = this.WhenAnyValue(x => x.TotalHours)
            .Select(hours => hours > 0 ? $"{hours} ч" : "—")
            .ToProperty(this, nameof(TotalHoursDisplay));

        _averageGradeDisplay = this.WhenAnyValue(x => x.AverageGrade)
            .Select(grade => grade > 0 ? $"{grade:F1}" : "—")
            .ToProperty(this, nameof(AverageGradeDisplay));

        _coursesStats = this.WhenAnyValue(x => x.CoursesCount)
            .Select(count => $"{count} курсов")
            .ToProperty(this, nameof(CoursesStats));

        _teachingStats = this.WhenAnyValue(
                x => x.TeachersCount,
                x => x.StudentsCount,
                (teachers, students) => $"{teachers} преп., {students} студ.")
            .ToProperty(this, nameof(TeachingStats));

        _hasDepartment = this.WhenAnyValue(x => x.DepartmentUid)
            .Select(departmentUid => departmentUid.HasValue && departmentUid != Guid.Empty)
            .ToProperty(this, nameof(HasDepartment));

        _hasDescription = this.WhenAnyValue(x => x.Description)
            .Select(desc => !string.IsNullOrEmpty(desc))
            .ToProperty(this, nameof(HasDescription));

        _canEdit = this.WhenAnyValue(x => x.IsActive)
            .Select(active => active)
            .ToProperty(this, nameof(CanEdit));

        _canDelete = this.WhenAnyValue(x => x.CoursesCount)
            .Select(coursesCount => coursesCount == 0)
            .ToProperty(this, nameof(CanDelete));

        _canDeactivate = this.WhenAnyValue(x => x.IsActive, x => x.CoursesCount)
            .Select(tuple => tuple.Item1 && tuple.Item2 == 0)
            .ToProperty(this, nameof(CanDeactivate));

        _canActivate = this.WhenAnyValue(x => x.IsActive)
            .Select(active => !active)
            .ToProperty(this, nameof(CanActivate));

        // Валидация
        _validationErrors = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.Credits,
                x => x.LessonsPerWeek)
            .Select(_ => GetValidationErrors())
            .ToProperty(this, nameof(ValidationErrors));

        _isValid = this.WhenAnyValue(x => x.ValidationErrors)
            .Select(errors => string.IsNullOrEmpty(errors))
            .ToProperty(this, nameof(IsValid));
    }

    #endregion

    #region Helper Methods

    private static string GetTypeDisplayName(SubjectType type)
    {
        return type switch
        {
            SubjectType.Required => "Обязательный",
            SubjectType.Elective => "Факультативный",
            SubjectType.Specialized => "Специализированный",
            SubjectType.Practicum => "Практикум",
            SubjectType.Seminar => "Семинар",
            _ => "Неизвестно"
        };
    }

    private static string GetTypeColor(SubjectType type)
    {
        return type switch
        {
            SubjectType.Required => "#2196F3",
            SubjectType.Elective => "#4CAF50",
            SubjectType.Specialized => "#FF9800",
            SubjectType.Practicum => "#9C27B0",
            SubjectType.Seminar => "#F44336",
            _ => "#9E9E9E"
        };
    }

    private static string GetCategoryDisplayName(SubjectCategory category)
    {
        return category switch
        {
            SubjectCategory.Mathematics => "Математика",
            SubjectCategory.NaturalSciences => "Естественные науки",
            SubjectCategory.Humanities => "Гуманитарные науки",
            SubjectCategory.ComputerScience => "Информатика",
            SubjectCategory.Languages => "Языки",
            SubjectCategory.Arts => "Искусство",
            SubjectCategory.Sports => "Спорт",
            SubjectCategory.Economics => "Экономика",
            SubjectCategory.Engineering => "Инженерия",
            SubjectCategory.Other => "Другое",
            _ => "Неизвестно"
        };
    }

    private static string GetCategoryColor(SubjectCategory category)
    {
        return category switch
        {
            SubjectCategory.Mathematics => "#4CAF50",
            SubjectCategory.NaturalSciences => "#2196F3",
            SubjectCategory.Humanities => "#FF9800",
            SubjectCategory.ComputerScience => "#9C27B0",
            SubjectCategory.Languages => "#607D8B",
            SubjectCategory.Arts => "#E91E63",
            SubjectCategory.Sports => "#FF5722",
            SubjectCategory.Economics => "#795548",
            SubjectCategory.Engineering => "#009688",
            SubjectCategory.Other => "#9E9E9E",
            _ => "#9E9E9E"
        };
    }

    private static string GetDepartmentDisplay(string? departmentName, string? departmentCode)
    {
        if (string.IsNullOrEmpty(departmentName))
            return "Не назначен";
            
        return string.IsNullOrEmpty(departmentCode) 
            ? departmentName 
            : $"{departmentName} ({departmentCode})";
    }

    private string GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название обязательно");

        if (Name?.Length > 200)
            errors.Add("Название не должно превышать 200 символов");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код предмета обязателен");

        if (Code?.Length > 20)
            errors.Add("Код предмета не должен превышать 20 символов");

        if (Credits <= 0)
            errors.Add("Количество кредитов должно быть больше 0");

        if (Credits > 10)
            errors.Add("Количество кредитов не должно превышать 10");

        if (LessonsPerWeek <= 0)
            errors.Add("Количество занятий в неделю должно быть больше 0");

        if (LessonsPerWeek > 20)
            errors.Add("Количество занятий в неделю не должно превышать 20");

        if (Description?.Length > 2000)
            errors.Add("Описание не должно превышать 2000 символов");

        return string.Join("; ", errors);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Создает Subject из ViewModel
    /// </summary>
    public Subject ToSubject()
    {
        return new Subject
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Credits = Credits,
            LessonsPerWeek = LessonsPerWeek,
            Type = Type,
            DepartmentUid = DepartmentUid,
            IsActive = IsActive
        };
    }

    /// <summary>
    /// Обновляет ViewModel из Subject
    /// </summary>
    public void UpdateFromSubject(Subject subject)
    {
        if (subject == null) return;

        Uid = subject.Uid;
        Name = subject.Name ?? string.Empty;
        Code = subject.Code ?? string.Empty;
        Description = subject.Description ?? string.Empty;
        Credits = subject.Credits;
        LessonsPerWeek = subject.LessonsPerWeek;
        Type = subject.Type;
        DepartmentUid = subject.DepartmentUid;
        IsActive = subject.IsActive;
        
        // Update department info if available
        if (subject.Department != null)
        {
            DepartmentName = subject.Department.Name;
            DepartmentCode = subject.Department.Code;
        }
    }

    /// <summary>
    /// Создает копию ViewModel
    /// </summary>
    public SubjectViewModel Clone()
    {
        return new SubjectViewModel
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            IsActive = IsActive,
            Credits = Credits,
            LessonsPerWeek = LessonsPerWeek,
            Type = Type,
            Category = Category,
            DepartmentUid = DepartmentUid,
            DepartmentName = DepartmentName,
            DepartmentCode = DepartmentCode,
            CoursesCount = CoursesCount,
            TeachersCount = TeachersCount,
            StudentsCount = StudentsCount,
            AverageGrade = AverageGrade,
            TotalHours = TotalHours,
            IsSelected = IsSelected,
            IsExpanded = IsExpanded,
            IsLoading = IsLoading
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Создает ViewModel из Subject
    /// </summary>
    public static SubjectViewModel FromSubject(Subject subject)
    {
        var viewModel = new SubjectViewModel();
        viewModel.UpdateFromSubject(subject);
        return viewModel;
    }

    /// <summary>
    /// Создает пустую ViewModel для нового предмета
    /// </summary>
    public static SubjectViewModel CreateNew(Guid? departmentUid = null)
    {
        return new SubjectViewModel
        {
            Uid = Guid.NewGuid(),
            DepartmentUid = departmentUid,
            IsActive = true,
            Credits = 3,
            LessonsPerWeek = 2,
            Type = SubjectType.Required,
            Category = SubjectCategory.ComputerScience
        };
    }

    #endregion

    #region Equality and Comparison

    public override bool Equals(object? obj)
    {
        return obj is SubjectViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{FullName} - {StatusDisplayName}";
    }

    #endregion
} 

