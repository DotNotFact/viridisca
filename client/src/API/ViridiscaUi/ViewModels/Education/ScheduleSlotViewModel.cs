using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Navigations;
using ViridiscaUi.Navigations; 

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual schedule slot with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
/// <remarks>
/// Was previously a distinct-path "/schedule-slots" route with no ReactiveViewLocator
/// mapping - a phantom sidebar entry (DisplayName defaulted to "ScheduleSlot", dumped into
/// "Основное" since Group wasn't set either) that froze the screen when clicked.
/// ScheduleViewModel ("schedule" route) is the real, wired page. This is a per-item
/// wrapper, not a page - no [Route] here.
/// </remarks>
public class ScheduleSlotViewModel : RoutableViewModelBase
{
    private readonly IScheduleSlotService _scheduleSlotService;
    private readonly ICourseInstanceService _courseInstanceService; 
    private readonly INotificationService _notificationService;

    #region Core Properties

    /// <summary>
    /// Связанная модель ScheduleSlot
    /// </summary>
    [Reactive] public ScheduleSlot ScheduleSlot { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public DayOfWeek DayOfWeek { get; set; }
    [Reactive] public TimeSpan StartTime { get; set; }
    [Reactive] public TimeSpan EndTime { get; set; }
    [Reactive] public string Room { get; set; } = string.Empty;
    [Reactive] public ScheduleSlotType Type { get; set; } = ScheduleSlotType.Lecture;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime? ValidFrom { get; set; }
    [Reactive] public DateTime? ValidTo { get; set; }
    [Reactive] public string Location { get; set; } = string.Empty;
    [Reactive] public Guid CourseInstanceUid { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Course Properties

    [Reactive] public Guid CourseUid { get; set; }
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string CourseCode { get; set; } = string.Empty;
    [Reactive] public CourseStatus CourseStatus { get; set; }

    #endregion

    #region Teacher Properties

    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherFullName { get; set; } = string.Empty;
    [Reactive] public string TeacherEmail { get; set; } = string.Empty;
    [Reactive] public string TeacherName { get; set; } = string.Empty;

    #endregion

    #region Group Properties

    [Reactive] public Guid GroupUid { get; set; }
    [Reactive] public string GroupName { get; set; } = string.Empty;
    [Reactive] public string GroupCode { get; set; } = string.Empty;
    [Reactive] public string CourseInstanceName { get; set; } = string.Empty;

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full slot name with course and group
    /// </summary>
    public string FullName => $"{CourseName} - {GroupName}";

    /// <summary>
    /// Day of week display
    /// </summary>
    public string DayOfWeekText => DayOfWeek switch
    {
        DayOfWeek.Monday => "Понедельник",
        DayOfWeek.Tuesday => "Вторник",
        DayOfWeek.Wednesday => "Среда",
        DayOfWeek.Thursday => "Четверг",
        DayOfWeek.Friday => "Пятница",
        DayOfWeek.Saturday => "Суббота",
        DayOfWeek.Sunday => "Воскресенье",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Slot type display
    /// </summary>
    public string TypeText => Type switch
    {
        ScheduleSlotType.Lecture => "Лекция",
        ScheduleSlotType.Seminar => "Семинар",
        ScheduleSlotType.Laboratory => "Лабораторная",
        ScheduleSlotType.Practice => "Практика",
        ScheduleSlotType.Consultation => "Консультация",
        ScheduleSlotType.Exam => "Экзамен",
        ScheduleSlotType.Test => "Зачет",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Type color
    /// </summary>
    public string TypeColor => Type switch
    {
        ScheduleSlotType.Lecture => "#2196F3",
        ScheduleSlotType.Seminar => "#FF9800",
        ScheduleSlotType.Laboratory => "#9C27B0",
        ScheduleSlotType.Practice => "#4CAF50",
        ScheduleSlotType.Consultation => "#607D8B",
        ScheduleSlotType.Exam => "#F44336",
        ScheduleSlotType.Test => "#795548",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Time slot display
    /// </summary>
    public string TimeSlot => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

    /// <summary>
    /// Course status display
    /// </summary>
    public string CourseStatusText => CourseStatus switch
    {
        CourseStatus.Draft => "Черновик",
        CourseStatus.Published => "Опубликовано",
        CourseStatus.InProgress => "В процессе",
        CourseStatus.Completed => "Завершено",
        CourseStatus.Cancelled => "Отменено",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Course status color
    /// </summary>
    public string CourseStatusColor => CourseStatus switch
    {
        CourseStatus.Draft => "#9E9E9E",
        CourseStatus.Published => "#2196F3",
        CourseStatus.InProgress => "#FF9800",
        CourseStatus.Completed => "#4CAF50",
        CourseStatus.Cancelled => "#F44336",
        _ => "#9E9E9E"
    };

    #endregion

    #region Initialization

    private void InitializeComputedProperties()
    {
        // Initialize computed properties for reactive UI
        this.WhenAnyValue(x => x.DayOfWeek, x => x.StartTime, x => x.EndTime)
            .Select(tuple => $"{tuple.Item1} {tuple.Item2:HH:mm}-{tuple.Item3:HH:mm}")
            .ToPropertyEx(this, x => x.TimeSlotText);

        this.WhenAnyValue(x => x.CourseInstanceName, x => x.TeacherName)
            .Select(tuple => $"{tuple.Item1} - {tuple.Item2}")
            .ToPropertyEx(this, x => x.DisplayText);
    }

    #endregion

    #region Reactive Properties

    public extern string TimeSlotText { [ObservableAsProperty] get; }
    public extern string DisplayText { [ObservableAsProperty] get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a ScheduleSlotViewModel
    /// </summary>
    public ScheduleSlotViewModel(
        IScheduleSlotService scheduleSlotService,
        ICourseInstanceService courseInstanceService, 
        INotificationService notificationService)
        : base(null)
    {
        _scheduleSlotService = scheduleSlotService ?? throw new ArgumentNullException(nameof(scheduleSlotService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService)); 
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        
        // Initialize computed properties
        InitializeComputedProperties();
    }

    /// <summary>
    /// Конструктор с слотом расписания
    /// </summary>
    public ScheduleSlotViewModel(ScheduleSlot slot) : this(null!, null!, null!)
    {
        // Uid must be set before UpdateFromScheduleSlot - it guards on slot.Uid == Uid to
        // avoid overwriting the wrong tracked instance, and defaults to Guid.Empty otherwise.
        Uid = slot.Uid;
        UpdateFromScheduleSlot(slot);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.CourseName, x => x.GroupName)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(FullName));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.DayOfWeek)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(DayOfWeekText));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Type)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TypeText));
                this.RaisePropertyChanged(nameof(TypeColor));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.StartTime, x => x.EndTime)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TimeSlot));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.CourseStatus)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CourseStatusText));
                this.RaisePropertyChanged(nameof(CourseStatusColor));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from a ScheduleSlot domain model
    /// </summary>
    public void UpdateFromScheduleSlot(ScheduleSlot slot)
    {
        if (slot == null)
            throw new ArgumentNullException(nameof(slot));
            
        if (slot.Uid != Uid)
            throw new ArgumentException("Cannot update from slot with different UID", nameof(slot));
            
        ScheduleSlot = slot;
        Uid = slot.Uid;
        DayOfWeek = slot.DayOfWeek;
        StartTime = slot.StartTime;
        EndTime = slot.EndTime;
        Room = slot.Room;
        Type = slot.Type;
        IsActive = slot.IsActive;
        ValidFrom = slot.ValidFrom;
        ValidTo = slot.ValidTo;
        Location = slot.Location;
        CourseInstanceUid = slot.CourseInstanceUid;
        CreatedAt = slot.CreatedAt;
        LastModifiedAt = slot.LastModifiedAt;
        
        // Получаем данные из связанной модели Course
        if (slot.Course != null)
        {
            CourseUid = slot.Course.Uid;
            CourseName = slot.Course.Name;
            CourseCode = slot.Course.Code;
            CourseStatus = slot.Course.Status;
        }
        
        // Получаем данные из связанной модели CourseInstance
        if (slot.CourseInstance != null)
        {
            CourseInstanceUid = slot.CourseInstance.Uid;
            CourseInstanceName = $"{slot.CourseInstance.Subject?.Name} - {slot.CourseInstance.Group?.Name}";
            
            // Note: GroupUid is read-only and computed from CourseInstance.Group.Uid
            // GroupUid = slot.CourseInstance.Group?.Uid;
            
            if (slot.CourseInstance.Teacher != null)
            {
                TeacherUid = slot.CourseInstance.Teacher.Uid;
                TeacherName = $"{slot.CourseInstance.Teacher.Person?.FirstName} {slot.CourseInstance.Teacher.Person?.LastName}";
            }
        }
    }

    /// <summary>
    /// Converts this ViewModel back to a ScheduleSlot domain model
    /// </summary>
    public ScheduleSlot ToScheduleSlot()
    {
        var scheduleSlot = new ScheduleSlot
        {
            Uid = Uid,
            CourseInstanceUid = CourseInstanceUid,
            DayOfWeek = DayOfWeek,
            StartTime = StartTime,
            EndTime = EndTime,
            Room = Room,
            Type = Type,
            IsActive = IsActive,
            CourseUid = CourseUid,
            TeacherUid = TeacherUid
        };

        return scheduleSlot;
    }

    /// <summary>
    /// Клонирует ScheduleSlotViewModel
    /// </summary>
    public ScheduleSlotViewModel Clone()
    {
        return new ScheduleSlotViewModel(null!, null!, null!)
        {
            Uid = Uid,
            DayOfWeek = DayOfWeek,
            StartTime = StartTime,
            EndTime = EndTime,
            Room = Room,
            Type = Type,
            IsActive = IsActive,
            ValidFrom = ValidFrom,
            ValidTo = ValidTo,
            Location = Location,
            CourseInstanceUid = CourseInstanceUid,
            CourseUid = CourseUid,
            CourseName = CourseName,
            CourseCode = CourseCode,
            CourseStatus = CourseStatus,
            TeacherUid = TeacherUid,
            TeacherFullName = TeacherFullName,
            TeacherEmail = TeacherEmail,
            GroupUid = GroupUid,
            GroupName = GroupName,
            GroupCode = GroupCode,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Validates the schedule slot data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Room))
            errors.Add("Необходимо указать аудиторию");
            
        if (EndTime <= StartTime)
            errors.Add("Время окончания должно быть позже времени начала");
            
        // Business logic validation
        if (CourseStatus != CourseStatus.InProgress)
            warnings.Add("Курс не находится в процессе");
        
        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Returns a string representation of the schedule slot
    /// </summary>
    public override string ToString()
    {
        return $"{DayOfWeekText} {TimeSlot}: {CourseName} - {GroupName}";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is ScheduleSlotViewModel other && Uid.Equals(other.Uid);
    }

    /// <summary>
    /// Returns hash code based on UID
    /// </summary>
    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    #endregion
} 

