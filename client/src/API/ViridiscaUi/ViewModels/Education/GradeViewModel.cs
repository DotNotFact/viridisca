namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual grade with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
[Route("grade-details", 
    DisplayName = "Детали оценки", 
    IconKey = "StarBox", 
    Order = 999,
    Group = "Образование",
    ShowInMenu = false,
    Description = "Детальная информация об оценке")]
public class GradeViewModel : ViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель Grade
    /// </summary>
    [Reactive] public Grade Grade { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public decimal Value { get; set; }
    [Reactive] public string Comment { get; set; } = string.Empty;
    [Reactive] public DateTime? GradedAt { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Assignment Properties

    [Reactive] public Guid AssignmentUid { get; set; }
    [Reactive] public string AssignmentTitle { get; set; } = string.Empty;
    [Reactive] public string AssignmentDescription { get; set; } = string.Empty;
    [Reactive] public AssignmentStatus AssignmentStatus { get; set; }
    [Reactive] public AssignmentType Type { get; set; } = AssignmentType.Homework;
    [Reactive] public bool IsPublished { get; set; } = false;
    [Reactive] public DateTime? AssignmentDueDate { get; set; }

    #endregion

    #region Student Properties

    [Reactive] public Guid StudentUid { get; set; }
    [Reactive] public string StudentFullName { get; set; } = string.Empty;
    [Reactive] public string StudentEmail { get; set; } = string.Empty;

    #endregion

    #region Teacher Properties

    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherFullName { get; set; } = string.Empty;
    [Reactive] public string TeacherEmail { get; set; } = string.Empty;

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full grade name with assignment and student
    /// </summary>
    public string FullName => $"{AssignmentTitle} - {StudentFullName}";

    /// <summary>
    /// Grade value display
    /// </summary>
    public string ValueText => Value.ToString("F1");

    /// <summary>
    /// Grade value color
    /// </summary>
    public string ValueColor => Value switch
    {
        var v when v >= 4.5m => "#4CAF50", // Отлично
        var v when v >= 3.5m => "#2196F3", // Хорошо
        var v when v >= 2.5m => "#FF9800", // Удовлетворительно
        _ => "#F44336" // Неудовлетворительно
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
    /// Grade details
    /// </summary>
    public string GradeDetails => GradedAt.HasValue ? $"Оценка: {ValueText} ({GradedAt.Value:d})" : $"Оценка: {ValueText} (не оценено)";

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a GradeViewModel
    /// </summary>
    public GradeViewModel()
        : base()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        GradedAt = DateTime.UtcNow;
        
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор с оценкой
    /// </summary>
    public GradeViewModel(Grade grade) : this()
    {
        // this() assigns a random Uid; UpdateFromGrade rejects any Uid that doesn't already
        // match grade.Uid, which a random Guid essentially never does - every
        // GradeViewModel(Grade) call threw. Correct the Uid first.
        Uid = grade.Uid;
        UpdateFromGrade(grade);
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
            
        this.WhenAnyValue(x => x.Value)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ValueText));
                this.RaisePropertyChanged(nameof(ValueColor));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.AssignmentStatus)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(AssignmentStatusText));
                this.RaisePropertyChanged(nameof(AssignmentStatusColor));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Value, x => x.GradedAt)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(GradeDetails));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from a Grade domain model
    /// </summary>
    public void UpdateFromGrade(Grade grade)
    {
        if (grade == null)
            throw new ArgumentNullException(nameof(grade));
            
        if (grade.Uid != Uid)
            throw new ArgumentException("Cannot update from grade with different UID", nameof(grade));
            
        Grade = grade;
        Uid = grade.Uid;
        Value = grade.Value;
        Comment = grade.Comment;
        GradedAt = grade.GradedAt;
        IsActive = grade.IsActive;
        
        // Получаем данные из связанной модели Assignment
        if (grade.Assignment != null)
        {
            AssignmentUid = grade.Assignment.Uid;
            AssignmentTitle = grade.Assignment.Title;
            AssignmentDescription = grade.Assignment.Description;
            AssignmentStatus = grade.Assignment.Status;
            Type = grade.Assignment.Type;
            IsPublished = grade.Assignment.IsPublished;
            AssignmentDueDate = grade.Assignment.DueDate;
        }
        
        // Получаем данные из связанной модели Student
        if (grade.Student != null)
        {
            StudentUid = grade.Student.Uid;
            if (grade.Student.Person != null)
            {
                StudentFullName = $"{grade.Student.Person.LastName} {grade.Student.Person.FirstName} {grade.Student.Person.MiddleName}".Trim();
                StudentEmail = grade.Student.Person.Email;
            }
        }
        
        // Получаем данные из связанной модели Teacher
        if (grade.Teacher != null)
        {
            TeacherUid = grade.Teacher.Uid;
            if (grade.Teacher.Person != null)
            {
                TeacherFullName = $"{grade.Teacher.Person.LastName} {grade.Teacher.Person.FirstName} {grade.Teacher.Person.MiddleName}".Trim();
                TeacherEmail = grade.Teacher.Person.Email;
            }
        }
        
        CreatedAt = grade.CreatedAt;
        LastModifiedAt = grade.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to a Grade domain model
    /// </summary>
    public Grade ToGrade()
    {
        var grade = new Grade
        {
            Uid = Uid,
            Value = Value,
            Comment = Comment,
            GradedAt = GradedAt,
            IsActive = IsActive,
            AssignmentUid = AssignmentUid,
            StudentUid = StudentUid,
            TeacherUid = TeacherUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return grade;
    }

    /// <summary>
    /// Creates a copy of this GradeViewModel
    /// </summary>
    public GradeViewModel Clone()
    {
        return new GradeViewModel()
        {
            Uid = Uid,
            Value = Value,
            Comment = Comment,
            GradedAt = GradedAt,
            IsActive = IsActive,
            AssignmentUid = AssignmentUid,
            AssignmentTitle = AssignmentTitle,
            AssignmentDescription = AssignmentDescription,
            AssignmentStatus = AssignmentStatus,
            AssignmentDueDate = AssignmentDueDate,
            StudentUid = StudentUid,
            StudentFullName = StudentFullName,
            StudentEmail = StudentEmail,
            TeacherUid = TeacherUid,
            TeacherFullName = TeacherFullName,
            TeacherEmail = TeacherEmail,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Validates the grade data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (Value < 0 || Value > 5)
            errors.Add("Оценка должна быть от 0 до 5");
            
        if (GradedAt.HasValue && GradedAt.Value > DateTime.Now)
            errors.Add("Дата оценки не может быть в будущем");
            
        // Business logic validation
        if (AssignmentStatus != AssignmentStatus.Submitted)
            warnings.Add("Задание не сдано");
        
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
    /// Returns a string representation of the grade
    /// </summary>
    public override string ToString()
    {
        return $"{AssignmentTitle} - {StudentFullName}: {ValueText}";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is GradeViewModel other && Uid.Equals(other.Uid);
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

