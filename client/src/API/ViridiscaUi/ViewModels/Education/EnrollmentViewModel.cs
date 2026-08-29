namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual enrollment with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
/// <remarks>
/// [Route] would be dead here: UnifiedNavigationService.ScanAndRegisterRoutes only registers
/// types assignable to IRoutableViewModel, and this derives from plain ViewModelBase (not
/// RoutableViewModelBase), so it would never be scanned, never in the sidebar, and unreachable.
/// This is a per-item wrapper (CourseName/StudentFullName/Status fields, UpdateFrom/Clone/
/// Validate helpers), not a real list page - no Items/SelectedItem/Create-Edit-Delete commands
/// exist, so EnrollmentView.axaml's bindings to them would also be dead. Not a page - no
/// [Route] here.
/// </remarks>
public class EnrollmentViewModel : ViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель Enrollment
    /// </summary>
    [Reactive] public Enrollment Enrollment { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public DateTime EnrollmentDate { get; set; }
    [Reactive] public DateTime? CompletionDate { get; set; }
    [Reactive] public EnrollmentStatus Status { get; set; }
    [Reactive] public decimal? FinalGrade { get; set; }
    [Reactive] public string Notes { get; set; } = string.Empty;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Course Properties

    [Reactive] public Guid CourseUid { get; set; }
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string CourseCode { get; set; } = string.Empty;
    [Reactive] public int CourseCredits { get; set; }
    [Reactive] public CourseStatus CourseStatus { get; set; }

    #endregion

    #region Student Properties

    [Reactive] public Guid StudentUid { get; set; }
    [Reactive] public string StudentFullName { get; set; } = string.Empty;
    [Reactive] public string StudentEmail { get; set; } = string.Empty;

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full enrollment name with course and student
    /// </summary>
    public string FullName => $"{CourseName} - {StudentFullName}";

    /// <summary>
    /// Enrollment status display
    /// </summary>
    public string StatusText => Status switch
    {
        EnrollmentStatus.Pending => "Ожидает",
        EnrollmentStatus.Active => "Активно",
        EnrollmentStatus.Completed => "Завершено",
        EnrollmentStatus.Dropped => "Отчислено",
        EnrollmentStatus.OnLeave => "В академическом отпуске",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Status color
    /// </summary>
    public string StatusColor => Status switch
    {
        EnrollmentStatus.Pending => "#9E9E9E",
        EnrollmentStatus.Active => "#4CAF50",
        EnrollmentStatus.Completed => "#2196F3",
        EnrollmentStatus.Dropped => "#F44336",
        EnrollmentStatus.OnLeave => "#FF9800",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Enrollment details
    /// </summary>
    public string EnrollmentDetails => $"{EnrollmentDate:g} - {StatusText}";

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

    #region Constructor

    /// <summary>
    /// Creates an EnrollmentViewModel
    /// </summary>
    public EnrollmentViewModel()
        : base()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        EnrollmentDate = DateTime.UtcNow;
        
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор с записью
    /// </summary>
    public EnrollmentViewModel(Enrollment enrollment) : this()
    {
        // this() assigns a random Uid; UpdateFromEnrollment rejects any Uid that doesn't
        // already match enrollment.Uid, which a random Guid essentially never does - every
        // EnrollmentViewModel(Enrollment) call threw. Correct the Uid first.
        Uid = enrollment.Uid;
        UpdateFromEnrollment(enrollment);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.CourseName, x => x.StudentFullName)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(FullName));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
                this.RaisePropertyChanged(nameof(EnrollmentDetails));
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
    /// Updates this ViewModel from an Enrollment domain model
    /// </summary>
    public void UpdateFromEnrollment(Enrollment enrollment)
    {
        if (enrollment == null)
            throw new ArgumentNullException(nameof(enrollment));
            
        if (enrollment.Uid != Uid)
            throw new ArgumentException("Cannot update from enrollment with different UID", nameof(enrollment));
            
        Enrollment = enrollment;
        Uid = enrollment.Uid;
        Status = enrollment.Status;
        EnrollmentDate = enrollment.EnrollmentDate;
        CompletionDate = enrollment.CompletionDate;
        FinalGrade = enrollment.FinalGrade;
        Notes = enrollment.Notes;
        
        // Получаем данные из связанной модели Course
        if (enrollment.Course != null)
        {
            CourseUid = enrollment.Course.Uid;
            CourseName = enrollment.Course.Name;
            CourseCode = enrollment.Course.Code;
            CourseCredits = enrollment.Course.Credits;
            CourseStatus = enrollment.Course.Status;
        }
        
        // Получаем данные из связанной модели Student
        if (enrollment.Student != null)
        {
            StudentUid = enrollment.Student.Uid;
            StudentFullName = enrollment.Student.Person?.FirstName + " " + enrollment.Student.Person?.LastName;
            StudentEmail = enrollment.Student.Person?.Email ?? string.Empty;
        }
        
        CreatedAt = enrollment.CreatedAt;
        LastModifiedAt = enrollment.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to an Enrollment domain model
    /// </summary>
    public Enrollment ToEnrollment()
    {
        var enrollment = new Enrollment
        {
            Uid = Uid,
            Status = Status,
            EnrollmentDate = EnrollmentDate,
            CompletionDate = CompletionDate,
            FinalGrade = FinalGrade,
            Notes = Notes,
            CourseInstanceUid = CourseUid,
            StudentUid = StudentUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return enrollment;
    }

    /// <summary>
    /// Creates a copy of this EnrollmentViewModel
    /// </summary>
    public EnrollmentViewModel Clone()
    {
        return new EnrollmentViewModel()
        {
            Uid = Uid,
            EnrollmentDate = EnrollmentDate,
            CompletionDate = CompletionDate,
            Status = Status,
            FinalGrade = FinalGrade,
            Notes = Notes,
            CourseUid = CourseUid,
            CourseName = CourseName,
            CourseCode = CourseCode,
            CourseStatus = CourseStatus,
            StudentUid = StudentUid,
            StudentFullName = StudentFullName,
            StudentEmail = StudentEmail,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Validates the enrollment data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (EnrollmentDate > DateTime.Now)
            errors.Add("Дата зачисления не может быть в будущем");
            
        if (CompletionDate.HasValue && CompletionDate.Value < EnrollmentDate)
            errors.Add("Дата завершения не может быть раньше даты зачисления");
            
        // Business logic validation
        if (Status == EnrollmentStatus.Completed && !CompletionDate.HasValue)
            warnings.Add("Зачисление помечено как завершенное, но дата завершения не указана");
        
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
    /// Returns a string representation of the enrollment
    /// </summary>
    public override string ToString()
    {
        return $"{CourseName} - {StudentFullName}: {StatusText}";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is EnrollmentViewModel other && Uid.Equals(other.Uid);
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

