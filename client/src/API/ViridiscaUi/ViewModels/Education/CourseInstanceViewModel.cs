using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Education;
using System.ComponentModel;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для экземпляра курса с полной реактивной поддержкой
/// </summary>
public class CourseInstanceViewModel : ReactiveObject
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public CourseStatus Status { get; set; }
    [Reactive] public DateTime StartDate { get; set; }
    [Reactive] public DateTime EndDate { get; set; }
    [Reactive] public int MaxEnrollments { get; set; }

    #endregion

    #region Subject Properties

    [Reactive] public Guid SubjectUid { get; set; }
    [Reactive] public string SubjectName { get; set; } = string.Empty;
    [Reactive] public string SubjectCode { get; set; } = string.Empty;
    [Reactive] public int Credits { get; set; }

    #endregion

    #region Teacher Properties

    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherName { get; set; } = string.Empty;
    [Reactive] public string TeacherFirstName { get; set; } = string.Empty;
    [Reactive] public string TeacherLastName { get; set; } = string.Empty;
    [Reactive] public string TeacherEmail { get; set; } = string.Empty;

    #endregion

    #region Group Properties

    [Reactive] public Guid GroupUid { get; set; }
    [Reactive] public string GroupName { get; set; } = string.Empty;
    [Reactive] public string GroupCode { get; set; } = string.Empty;

    #endregion

    #region Academic Period Properties

    [Reactive] public Guid AcademicPeriodUid { get; set; }
    [Reactive] public string AcademicPeriodName { get; set; } = string.Empty;
    [Reactive] public AcademicPeriodType PeriodType { get; set; }

    #endregion

    #region Statistics Properties

    [Reactive] public int EnrollmentsCount { get; set; }
    [Reactive] public int AssignmentsCount { get; set; }
    [Reactive] public int CompletedAssignments { get; set; }
    [Reactive] public double AverageGrade { get; set; }
    [Reactive] public int ActiveStudents { get; set; }
    [Reactive] public int CompletedStudents { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Инициалы преподавателя
    /// </summary>
    public string TeacherInitials => _teacherInitials?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _teacherInitials;

    /// <summary>
    /// Отображаемое название статуса
    /// </summary>
    public string StatusDisplayName => _statusDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusDisplayName;

    /// <summary>
    /// Цвет статуса
    /// </summary>
    public string StatusColor => _statusColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusColor;

    /// <summary>
    /// Краткое описание (первые 100 символов)
    /// </summary>
    public string ShortDescription => _shortDescription?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _shortDescription;

    /// <summary>
    /// Форматированная дата начала
    /// </summary>
    public string FormattedStartDate => _formattedStartDate?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedStartDate;

    /// <summary>
    /// Форматированная дата окончания
    /// </summary>
    public string FormattedEndDate => _formattedEndDate?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedEndDate;

    /// <summary>
    /// Продолжительность курса
    /// </summary>
    public string Duration => _duration?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _duration;

    /// <summary>
    /// Активен ли курс
    /// </summary>
    public bool IsActive => _isActive?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isActive;

    /// <summary>
    /// Завершен ли курс
    /// </summary>
    public bool IsCompleted => _isCompleted?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isCompleted;

    /// <summary>
    /// Процент заполненности
    /// </summary>
    public double FillPercentage => _fillPercentage?.Value ?? 0.0;
    private ObservableAsPropertyHelper<double>? _fillPercentage;

    /// <summary>
    /// Процент завершения
    /// </summary>
    public double CompletionRate => _completionRate?.Value ?? 0.0;
    private ObservableAsPropertyHelper<double>? _completionRate;

    /// <summary>
    /// Средний балл в виде строки
    /// </summary>
    public string AverageGradeDisplay => _averageGradeDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _averageGradeDisplay;

    /// <summary>
    /// Статистика студентов
    /// </summary>
    public string StudentsStats => _studentsStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _studentsStats;

    /// <summary>
    /// Статистика заданий
    /// </summary>
    public string AssignmentsStats => _assignmentsStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _assignmentsStats;

    /// <summary>
    /// Можно ли редактировать курс
    /// </summary>
    public bool CanEdit => _canEdit?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEdit;

    /// <summary>
    /// Можно ли удалить курс
    /// </summary>
    public bool CanDelete => _canDelete?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDelete;

    /// <summary>
    /// Валидация
    /// </summary>
    public bool IsValid => _isValid?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isValid;

    #endregion

    #region Constructors

    public CourseInstanceViewModel()
    {
        InitializeComputedProperties();
    }

    public CourseInstanceViewModel(CourseInstance courseInstance) : this()
    {
        UpdateFromCourseInstance(courseInstance);
    }

    #endregion

    #region Private Methods

    private void InitializeComputedProperties()
    {
        // Teacher initials
        _teacherInitials = this.WhenAnyValue(x => x.TeacherFirstName, x => x.TeacherLastName, GetInitials)
            .ToProperty(this, x => x.TeacherInitials);

        // Status display
        _statusDisplayName = this.WhenAnyValue(x => x.Status, GetStatusDisplayName)
            .ToProperty(this, x => x.StatusDisplayName);

        _statusColor = this.WhenAnyValue(x => x.Status, GetStatusColor)
            .ToProperty(this, x => x.StatusColor);

        // Description
        _shortDescription = this.WhenAnyValue(x => x.Description, desc => 
            string.IsNullOrEmpty(desc) ? string.Empty : 
            desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc)
            .ToProperty(this, x => x.ShortDescription);

        // Dates
        _formattedStartDate = this.WhenAnyValue(x => x.StartDate, date => date.ToString("dd.MM.yyyy"))
            .ToProperty(this, x => x.FormattedStartDate);

        _formattedEndDate = this.WhenAnyValue(x => x.EndDate, date => date.ToString("dd.MM.yyyy"))
            .ToProperty(this, x => x.FormattedEndDate);

        _duration = this.WhenAnyValue(x => x.StartDate, x => x.EndDate, GetDuration)
            .ToProperty(this, x => x.Duration);

        // Status checks
        _isActive = this.WhenAnyValue(x => x.Status, status => status == CourseStatus.Active)
            .ToProperty(this, x => x.IsActive);

        _isCompleted = this.WhenAnyValue(x => x.Status, status => status == CourseStatus.Completed)
            .ToProperty(this, x => x.IsCompleted);

        // Statistics
        _fillPercentage = this.WhenAnyValue(x => x.EnrollmentsCount, x => x.MaxEnrollments, 
            (enrolled, max) => max > 0 ? (double)enrolled / max * 100 : 0)
            .ToProperty(this, x => x.FillPercentage);

        _completionRate = this.WhenAnyValue(x => x.CompletedStudents, x => x.EnrollmentsCount,
            (completed, total) => total > 0 ? (double)completed / total * 100 : 0)
            .ToProperty(this, x => x.CompletionRate);

        _averageGradeDisplay = this.WhenAnyValue(x => x.AverageGrade, grade => 
            grade > 0 ? grade.ToString("F1") : "—")
            .ToProperty(this, x => x.AverageGradeDisplay);

        _studentsStats = this.WhenAnyValue(x => x.ActiveStudents, x => x.EnrollmentsCount,
            (active, total) => $"{active}/{total}")
            .ToProperty(this, x => x.StudentsStats);

        _assignmentsStats = this.WhenAnyValue(x => x.CompletedAssignments, x => x.AssignmentsCount,
            (completed, total) => $"{completed}/{total}")
            .ToProperty(this, x => x.AssignmentsStats);

        // Permissions
        _canEdit = this.WhenAnyValue(x => x.Status, status => 
            status != CourseStatus.Completed && status != CourseStatus.Archived)
            .ToProperty(this, x => x.CanEdit);

        _canDelete = this.WhenAnyValue(x => x.Status, x => x.EnrollmentsCount,
            (status, enrollments) => status == CourseStatus.Draft && enrollments == 0)
            .ToProperty(this, x => x.CanDelete);

        // Validation
        _isValid = this.WhenAnyValue(
            x => x.Name, x => x.SubjectUid, x => x.TeacherUid, x => x.GroupUid, x => x.AcademicPeriodUid,
            (name, subjectUid, teacherUid, groupUid, periodUid) =>
                !string.IsNullOrWhiteSpace(name) &&
                subjectUid != Guid.Empty &&
                teacherUid != Guid.Empty &&
                groupUid != Guid.Empty &&
                periodUid != Guid.Empty)
            .ToProperty(this, x => x.IsValid);
    }

    private static string GetInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? string.Empty : firstName.Substring(0, 1).ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? string.Empty : lastName.Substring(0, 1).ToUpper();
        return $"{first}{last}";
    }

    private static string GetStatusDisplayName(CourseStatus status)
    {
        return status switch
        {
            CourseStatus.Draft => "Черновик",
            CourseStatus.Published => "Опубликован",
            CourseStatus.Active => "Активный",
            CourseStatus.Completed => "Завершен",
            CourseStatus.Archived => "Архивирован",
            CourseStatus.Suspended => "Приостановлен",
            _ => "Неизвестно"
        };
    }

    private static string GetStatusColor(CourseStatus status)
    {
        return status switch
        {
            CourseStatus.Draft => "#FFA500",
            CourseStatus.Published => "#4CAF50",
            CourseStatus.Active => "#2196F3",
            CourseStatus.Completed => "#9C27B0",
            CourseStatus.Archived => "#757575",
            CourseStatus.Suspended => "#FFA500",
            _ => "#757575"
        };
    }

    private static string GetDuration(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate) return "—";
        
        var duration = endDate - startDate;
        var weeks = (int)Math.Ceiling(duration.TotalDays / 7);
        
        return weeks switch
        {
            1 => "1 неделя",
            < 5 => $"{weeks} недели",
            _ => $"{weeks} недель"
        };
    }

    private static CourseStatus ParseCourseStatus(string status)
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
            "архив" or "архивирован" => CourseStatus.Archived,
            "приостановлен" => CourseStatus.Suspended,
            _ => CourseStatus.Draft
        };
    }

    #endregion

    #region Public Methods

    public CourseInstance ToCourseInstance()
    {
        return new CourseInstance
        {
            Uid = Uid,
            SubjectUid = SubjectUid,
            TeacherUid = TeacherUid,
            GroupUid = GroupUid,
            AcademicPeriodUid = AcademicPeriodUid,
            StartDate = StartDate,
            EndDate = EndDate,
            MaxEnrollments = MaxEnrollments,
            Status = Status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    public void UpdateFromCourseInstance(CourseInstance courseInstance)
    {
        Uid = courseInstance.Uid;
        SubjectUid = courseInstance.SubjectUid;
        TeacherUid = courseInstance.TeacherUid.HasValue ? courseInstance.TeacherUid.Value : Guid.Empty;
        GroupUid = courseInstance.GroupUid;
        AcademicPeriodUid = courseInstance.AcademicPeriodUid;
        StartDate = courseInstance.StartDate;
        EndDate = courseInstance.EndDate ?? DateTime.Today.AddMonths(4);
        MaxEnrollments = courseInstance.MaxEnrollments;
        Status = courseInstance.Status;
        EnrollmentsCount = courseInstance.Enrollments?.Count ?? 0;

        // Set related entity names
        SubjectName = courseInstance.Subject?.Name ?? "Неизвестный предмет";
        SubjectCode = courseInstance.Subject?.Code ?? string.Empty;
        TeacherName = courseInstance.Teacher?.Person?.FullName ?? "Неизвестный преподаватель";
        GroupName = courseInstance.Group?.Name ?? "Неизвестная группа";
        AcademicPeriodName = courseInstance.AcademicPeriod?.Name ?? "Неизвестный период";

        // Name/Code are the display identity of the row (bound directly in CoursesView's
        // list template) - a CourseInstance has no name of its own, so derive it from the
        // subject + group it links, the way the UI already labels these rows everywhere else.
        Name = $"{SubjectName} — {GroupName}";
        Code = !string.IsNullOrEmpty(courseInstance.Code) ? courseInstance.Code : SubjectCode;
    }

    public CourseInstanceViewModel Clone()
    {
        return new CourseInstanceViewModel
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Status = Status,
            StartDate = StartDate,
            EndDate = EndDate,
            MaxEnrollments = MaxEnrollments,
            SubjectUid = SubjectUid,
            SubjectName = SubjectName,
            SubjectCode = SubjectCode,
            Credits = Credits,
            TeacherUid = TeacherUid,
            TeacherName = TeacherName,
            TeacherFirstName = TeacherFirstName,
            TeacherLastName = TeacherLastName,
            TeacherEmail = TeacherEmail,
            GroupUid = GroupUid,
            GroupName = GroupName,
            GroupCode = GroupCode,
            AcademicPeriodUid = AcademicPeriodUid,
            AcademicPeriodName = AcademicPeriodName,
            PeriodType = PeriodType,
            EnrollmentsCount = EnrollmentsCount,
            AssignmentsCount = AssignmentsCount,
            CompletedAssignments = CompletedAssignments,
            AverageGrade = AverageGrade,
            ActiveStudents = ActiveStudents,
            CompletedStudents = CompletedStudents
        };
    }

    public static CourseInstanceViewModel FromCourseInstance(CourseInstance courseInstance)
    {
        return new CourseInstanceViewModel(courseInstance);
    }

    public static CourseInstanceViewModel CreateNew(Guid subjectUid, Guid teacherUid, Guid groupUid, Guid academicPeriodUid)
    {
        return new CourseInstanceViewModel
        {
            Uid = Guid.NewGuid(),
            SubjectUid = subjectUid,
            TeacherUid = teacherUid,
            GroupUid = groupUid,
            AcademicPeriodUid = academicPeriodUid,
            Status = CourseStatus.Draft,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(4),
            MaxEnrollments = 30
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is CourseInstanceViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} ({GroupCode}) - {TeacherName}";
    }

    #endregion
} 

