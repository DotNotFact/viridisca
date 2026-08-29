using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual student with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
[Route("/students")]
public class StudentViewModel : ViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель Student
    /// </summary>
    [Reactive] public Student Student { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public string StudentId { get; set; } = string.Empty;
    [Reactive] public DateTime DateOfBirth { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Group Properties

    [Reactive] public Guid GroupUid { get; set; }
    [Reactive] public string GroupName { get; set; } = string.Empty;
    [Reactive] public string GroupCode { get; set; } = string.Empty;

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full student name
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpperInvariant();

    public string StatusDisplayName => IsActive ? "Активен" : "Неактивен";

    public string StatusColor => IsActive ? "#4CAF50" : "#FF9800";

    /// <summary>
    /// Student details
    /// </summary>
    public string StudentDetails => $"ID: {StudentId}, Группа: {GroupName}";

    /// <summary>
    /// Contact information
    /// </summary>
    public string ContactInfo => $"Email: {Email}, Телефон: {Phone}";

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a StudentViewModel from a Student domain model
    /// </summary>
    public StudentViewModel()
        : base()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        DateOfBirth = DateTime.Today.AddYears(-18); // Default to 18 years ago
        
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор со студентом
    /// </summary>
    public StudentViewModel(Student student) : this()
    {
        // this() assigns a random Uid (for the "new blank student" case); UpdateFromStudent
        // then rejects any Uid that doesn't already match student.Uid, which a random Guid
        // essentially never does - every StudentViewModel(Student) call threw. Correct the
        // Uid first so the guard sees a match.
        Uid = student.Uid;
        UpdateFromStudent(student);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.FirstName, x => x.LastName, x => x.MiddleName)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(FullName));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.StudentId, x => x.GroupName)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StudentDetails));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Email, x => x.Phone)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ContactInfo));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from a Student domain model
    /// </summary>
    public void UpdateFromStudent(Student student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));
            
        if (student.Uid != Uid)
            throw new ArgumentException("Cannot update from student with different UID", nameof(student));
            
        Student = student;
        Uid = student.Uid;
        
        // Получаем данные из связанной модели Person
        if (student.Person != null)
        {
            FirstName = student.Person.FirstName;
            LastName = student.Person.LastName;
            MiddleName = student.Person.MiddleName ?? string.Empty;
            Email = student.Person.Email;
            Phone = student.Person.Phone ?? string.Empty;
            DateOfBirth = student.Person.DateOfBirth ?? DateTime.Today.AddYears(-18);
        }
        
        // Данные из модели Student
        StudentId = student.StudentCode;
        IsActive = student.IsActive;
        
        // Получаем данные из связанной модели Group
        if (student.Group != null)
        {
            GroupUid = student.Group.Uid;
            GroupName = student.Group.Name;
            GroupCode = student.Group.Code;
        }
        
        CreatedAt = student.CreatedAt;
        LastModifiedAt = student.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to a Student domain model
    /// </summary>
    public Student ToStudent()
    {
        var student = new Student
        {
            Uid = Uid,
            StudentCode = StudentId,
            IsActive = IsActive,
            GroupUid = GroupUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        // Person данные не устанавливаем здесь, так как это отдельная сущность
        // Они должны обрабатываться через PersonService

        return student;
    }

    /// <summary>
    /// Creates a copy of this StudentViewModel
    /// </summary>
    public StudentViewModel Clone()
    {
        return new StudentViewModel()
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            Phone = Phone,
            StudentId = StudentId,
            DateOfBirth = DateOfBirth,
            IsActive = IsActive,
            GroupUid = GroupUid,
            GroupName = GroupName,
            GroupCode = GroupCode,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Validates the student data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("Необходимо указать имя студента");
            
        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Необходимо указать фамилию студента");
            
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Необходимо указать email студента");
            
        if (string.IsNullOrWhiteSpace(StudentId))
            errors.Add("Необходимо указать номер студенческого билета");
            
        // Business logic validation
        if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            errors.Add("Некорректный формат email");
            
        if (!string.IsNullOrWhiteSpace(Phone) && !IsValidPhone(Phone))
            warnings.Add("Некорректный формат телефона");
            
        if (DateOfBirth > DateTime.Today.AddYears(-16))
            errors.Add("Студент должен быть старше 16 лет");
        
        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates phone format
    /// </summary>
    private bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^\+?[0-9\s\-\(\)]{10,}$");
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Returns a string representation of the student
    /// </summary>
    public override string ToString()
    {
        return $"{FullName} ({GroupName})";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is StudentViewModel other && Uid.Equals(other.Uid);
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

