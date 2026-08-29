namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual submission with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
/// <remarks>
/// [Route] would be dead here: UnifiedNavigationService.ScanAndRegisterRoutes only registers
/// types assignable to IRoutableViewModel, and this derives from plain ViewModelBase (not
/// RoutableViewModelBase), so it would never be scanned, never in the sidebar, and unreachable.
/// This is a per-item wrapper (CourseName/StudentFullName/Status fields, UpdateFrom/Clone/
/// Validate helpers), not a real list page - no Items/SelectedItem/Create-Edit-Delete commands
/// exist, so SubmissionView.axaml's bindings to them would also be dead. Not a page - no
/// [Route] here.
/// </remarks>
public class SubmissionViewModel : ViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель Submission
    /// </summary>
    [Reactive] public Submission Submission { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Content { get; set; } = string.Empty;
    [Reactive] public string FilePath { get; set; } = string.Empty;
    [Reactive] public DateTime? SubmittedAt { get; set; }
    [Reactive] public SubmissionStatus Status { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Assignment Properties

    [Reactive] public Guid AssignmentUid { get; set; }
    [Reactive] public string AssignmentTitle { get; set; } = string.Empty;
    [Reactive] public string AssignmentDescription { get; set; } = string.Empty;
    [Reactive] public AssignmentStatus AssignmentStatus { get; set; }
    [Reactive] public DateTime? AssignmentDueDate { get; set; }

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
    /// Full submission name with assignment and student
    /// </summary>
    public string FullName => $"{AssignmentTitle} - {StudentFullName}";

    /// <summary>
    /// Submission status display
    /// </summary>
    public string StatusText => Status switch
    {
        SubmissionStatus.Draft => "Черновик",
        SubmissionStatus.Submitted => "Сдано",
        SubmissionStatus.UnderReview => "На проверке",
        SubmissionStatus.Graded => "Оценено",
        SubmissionStatus.Returned => "Возвращено",
        SubmissionStatus.Overdue => "Просрочено",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Status color
    /// </summary>
    public string StatusColor => Status switch
    {
        SubmissionStatus.Draft => "#9E9E9E",
        SubmissionStatus.Submitted => "#2196F3",
        SubmissionStatus.UnderReview => "#FF9800",
        SubmissionStatus.Graded => "#4CAF50",
        SubmissionStatus.Returned => "#F44336",
        SubmissionStatus.Overdue => "#F44336",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Assignment status display
    /// </summary>
    public string AssignmentStatusText => AssignmentStatus switch
    {
        AssignmentStatus.Draft => "Черновик",
        AssignmentStatus.Published => "Опубликовано",
        AssignmentStatus.InProgress => "В процессе",
        AssignmentStatus.Submitted => "Сдано",
        AssignmentStatus.Graded => "Оценено",
        AssignmentStatus.Overdue => "Просрочено",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Assignment status color
    /// </summary>
    public string AssignmentStatusColor => AssignmentStatus switch
    {
        AssignmentStatus.Draft => "#9E9E9E",
        AssignmentStatus.Published => "#2196F3",
        AssignmentStatus.InProgress => "#FF9800",
        AssignmentStatus.Submitted => "#4CAF50",
        AssignmentStatus.Graded => "#4CAF50",
        AssignmentStatus.Overdue => "#F44336",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Submission details
    /// </summary>
    public string SubmissionDetails => SubmittedAt.HasValue ? $"Сдано: {SubmittedAt.Value:d}" : "Не сдано";

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a SubmissionViewModel
    /// </summary>
    public SubmissionViewModel()
        : base()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        SubmittedAt = null; // Изначально не сдано
        
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор с заданием
    /// </summary>
    public SubmissionViewModel(Submission submission) : this()
    {
        // this() assigns a random Uid; UpdateFromSubmission rejects any Uid that doesn't
        // already match submission.Uid, which a random Guid essentially never does - every
        // SubmissionViewModel(Submission) call threw. Correct the Uid first.
        Uid = submission.Uid;
        UpdateFromSubmission(submission);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.AssignmentTitle, x => x.StudentFullName)
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
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.AssignmentStatus)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(AssignmentStatusText));
                this.RaisePropertyChanged(nameof(AssignmentStatusColor));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.SubmittedAt)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(SubmissionDetails));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from a Submission domain model
    /// </summary>
    public void UpdateFromSubmission(Submission submission)
    {
        if (submission == null)
            throw new ArgumentNullException(nameof(submission));
            
        if (submission.Uid != Uid)
            throw new ArgumentException("Cannot update from submission with different UID", nameof(submission));
            
        Submission = submission;
        Uid = submission.Uid;
        Content = submission.Content;
        FilePath = submission.FilePath;
        Status = submission.Status;
        SubmittedAt = submission.SubmittedAt;
        IsActive = submission.IsActive;
        
        // Получаем данные из связанной модели Assignment
        if (submission.Assignment != null)
        {
            AssignmentUid = submission.Assignment.Uid;
            AssignmentTitle = submission.Assignment.Title;
            AssignmentDescription = submission.Assignment.Description;
            AssignmentStatus = submission.Assignment.Status;
            AssignmentDueDate = submission.Assignment.DueDate;
        }
        
        // Получаем данные из связанной модели Student
        if (submission.Student != null)
        {
            StudentUid = submission.Student.Uid;
            StudentFullName = submission.Student.Person?.FirstName + " " + submission.Student.Person?.LastName;
            StudentEmail = submission.Student.Person?.Email ?? string.Empty;
        }
        
        CreatedAt = submission.CreatedAt;
        LastModifiedAt = submission.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to a Submission domain model
    /// </summary>
    public Submission ToSubmission()
    {
        var submission = new Submission
        {
            Uid = Uid,
            Content = Content,
            FilePath = FilePath,
            Status = Status,
            SubmittedAt = SubmittedAt,
            IsActive = IsActive,
            AssignmentUid = AssignmentUid,
            StudentUid = StudentUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return submission;
    }

    /// <summary>
    /// Creates a copy of this SubmissionViewModel
    /// </summary>
    public SubmissionViewModel Clone()
    {
        return new SubmissionViewModel()
        {
            Uid = Uid,
            Content = Content,
            FilePath = FilePath,
            SubmittedAt = SubmittedAt,
            Status = Status,
            IsActive = IsActive,
            AssignmentUid = AssignmentUid,
            AssignmentTitle = AssignmentTitle,
            AssignmentDescription = AssignmentDescription,
            StudentUid = StudentUid,
            StudentFullName = StudentFullName,
            StudentEmail = StudentEmail,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Validates the submission data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(FilePath))
            errors.Add("Необходимо указать содержание или прикрепить файл");
            
        if (SubmittedAt.HasValue && SubmittedAt.Value > DateTime.Now)
            errors.Add("Дата сдачи не может быть в будущем");
            
        // Business logic validation
        if (Status == SubmissionStatus.Submitted && !SubmittedAt.HasValue)
            warnings.Add("Работа помечена как сданная, но дата сдачи не указана");
        
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
    /// Returns a string representation of the submission
    /// </summary>
    public override string ToString()
    {
        return $"{AssignmentTitle} - {StudentFullName}: {StatusText}";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is SubmissionViewModel other && Uid.Equals(other.Uid);
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

