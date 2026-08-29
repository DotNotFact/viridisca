using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using Microsoft.Extensions.Logging; 
using System.Text.RegularExpressions;
using System.Net.Mail;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual teacher with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
/// <remarks>
/// Was previously a distinct-path "/teachers" route with no ReactiveViewLocator mapping -
/// a phantom sidebar entry (DisplayName defaulted to "Teacher", dumped into "Основное"
/// since Group wasn't set either) that froze the screen when clicked. TeachersViewModel
/// ("teachers" route) is the real, wired page. This is a per-item wrapper, not a page -
/// no [Route] here.
/// </remarks>
public class TeacherViewModel : RoutableViewModelBase
{
    #region Services
    
    private readonly IDepartmentService _departmentService;
    private readonly ITeacherService _teacherService; 
    private readonly INotificationService _notificationService;
    private readonly ILogger<TeacherViewModel> _logger;

    #endregion

    #region Core Properties

    /// <summary>
    /// Связанная модель Teacher
    /// </summary>
    [Reactive] public Teacher Teacher { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public Guid PersonUid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public string AcademicDegree { get; set; } = string.Empty;
    [Reactive] public string AcademicTitle { get; set; } = string.Empty;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    #endregion

    #region Department Properties

    [Reactive] public Guid DepartmentUid { get; set; }
    [Reactive] public string DepartmentName { get; set; } = string.Empty;
    [Reactive] public string DepartmentCode { get; set; } = string.Empty;

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadTeachersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<Teacher, Unit> EditTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<Teacher, Unit> DeleteTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<Teacher, Unit> ViewTeacherDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full teacher name
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Teacher details
    /// </summary>
    public string TeacherDetails => $"{AcademicDegree}, {AcademicTitle}";

    /// <summary>
    /// Contact information
    /// </summary>
    public string ContactInfo => $"Email: {Email}, Телефон: {Phone}";

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a TeacherViewModel
    /// </summary>
    public TeacherViewModel(
        IScreen hostScreen,
        IDepartmentService departmentService,
        ITeacherService teacherService, 
        INotificationService notificationService,
        ILogger<TeacherViewModel> logger) 
        : base(hostScreen)
    {
        _departmentService = departmentService;
        _teacherService = teacherService;
        _notificationService = notificationService;
        _logger = logger;

        InitializeCommands();
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор с преподавателем
    /// </summary>
    public TeacherViewModel(
        IScreen hostScreen,
        Teacher teacher,
        IDepartmentService departmentService,
        ITeacherService teacherService, 
        INotificationService notificationService,
        ILogger<TeacherViewModel> logger) 
        : this(hostScreen, departmentService, teacherService, notificationService, logger)
    {
        UpdateFromTeacher(teacher);
    }

    /// <summary>
    /// Простой конструктор для использования в коллекциях (только с Teacher объектом)
    /// </summary>
    public TeacherViewModel(Teacher teacher) : base(hostScreen: null!)
    {
        _departmentService = null!; // Для простых ViewModels сервисы не нужны
        _teacherService = null!; 
        _notificationService = null!;
        _logger = null!;
        
        UpdateFromTeacher(teacher);
        SetupPropertyChangeNotifications();
    }

    #endregion

    #region Initialization

    private void InitializeCommands()
    {
        // Инициализация команд
        LoadTeachersCommand = ReactiveCommand.CreateFromTask(LoadTeachersAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        CreateTeacherCommand = ReactiveCommand.CreateFromTask(CreateTeacherAsync);
        EditTeacherCommand = ReactiveCommand.CreateFromTask<Teacher>(EditTeacherAsync);
        DeleteTeacherCommand = ReactiveCommand.CreateFromTask<Teacher>(DeleteTeacherAsync);
        ViewTeacherDetailsCommand = ReactiveCommand.CreateFromTask<Teacher>(ViewTeacherDetailsAsync);
        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
        GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
        NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync);
        PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync);
        FirstPageCommand = ReactiveCommand.CreateFromTask(FirstPageAsync);
        LastPageCommand = ReactiveCommand.CreateFromTask(LastPageAsync);
    }

    #endregion

    #region Command Implementations

    private async Task LoadTeachersAsync()
    {
        try
        {
            IsLoading = true;
            // Implementation for loading teachers
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке преподавателей", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadTeachersAsync();
    }

    private async Task CreateTeacherAsync()
    {
        try
        {
            // Implementation for creating teacher
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при создании преподавателя", ex);
        }
    }

    private async Task EditTeacherAsync(Teacher teacher)
    {
        try
        {
            // Implementation for editing teacher
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при редактировании преподавателя", ex);
        }
    }

    private async Task DeleteTeacherAsync(Teacher teacher)
    {
        try
        {
            // Implementation for deleting teacher
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при удалении преподавателя", ex);
        }
    }

    private async Task ViewTeacherDetailsAsync(Teacher teacher)
    {
        try
        {
            // Implementation for viewing teacher details
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при просмотре деталей преподавателя", ex);
        }
    }

    private async Task SearchAsync()
    {
        try
        {
            // Implementation for searching
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при поиске", ex);
        }
    }

    private async Task GoToPageAsync(int page)
    {
        try
        {
            // Implementation for going to page
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при переходе на страницу", ex);
        }
    }

    private async Task NextPageAsync()
    {
        try
        {
            // Implementation for next page
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при переходе на следующую страницу", ex);
        }
    }

    private async Task PreviousPageAsync()
    {
        try
        {
            // Implementation for previous page
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при переходе на предыдущую страницу", ex);
        }
    }

    private async Task FirstPageAsync()
    {
        try
        {
            // Implementation for first page
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при переходе на первую страницу", ex);
        }
    }

    private async Task LastPageAsync()
    {
        try
        {
            // Implementation for last page
            await Task.Delay(100); // Placeholder
        }
        catch (Exception ex)
        {
            SetError("Ошибка при переходе на последнюю страницу", ex);
        }
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
            
        this.WhenAnyValue(x => x.AcademicDegree, x => x.AcademicTitle)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TeacherDetails));
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
    /// Updates this ViewModel from a Teacher domain model
    /// </summary>
    public void UpdateFromTeacher(Teacher teacher)
    {
        if (teacher == null) return;

        Teacher = teacher;
        Uid = teacher.Uid;
        PersonUid = teacher.PersonUid;
        FirstName = teacher.Person?.FirstName ?? string.Empty;
        LastName = teacher.Person?.LastName ?? string.Empty;
        MiddleName = teacher.Person?.MiddleName ?? string.Empty;
        Email = teacher.Person?.Email ?? string.Empty;
        Phone = teacher.Person?.PhoneNumber ?? string.Empty;
        AcademicDegree = teacher.AcademicDegree ?? string.Empty;
        AcademicTitle = teacher.AcademicTitle ?? string.Empty;
        IsActive = teacher.IsActive;
        CreatedAt = teacher.CreatedAt;
        LastModifiedAt = teacher.LastModifiedAt;

        // Department properties
        DepartmentUid = teacher.DepartmentUid ?? Guid.Empty;
        DepartmentName = teacher.Department?.Name ?? string.Empty;
        DepartmentCode = teacher.Department?.Code ?? string.Empty;
    }

    /// <summary>
    /// Converts this ViewModel to a Teacher domain model
    /// </summary>
    public Teacher ToTeacher()
    {
        return new Teacher
        {
            Uid = Uid,
            PersonUid = Teacher.PersonUid,
            EmployeeCode = Teacher.EmployeeCode,
            DepartmentUid = DepartmentUid,
            AcademicDegree = AcademicDegree,
            AcademicTitle = AcademicTitle,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Creates a copy of this ViewModel
    /// </summary>
    public TeacherViewModel Clone()
    {
        return new TeacherViewModel(
            HostScreen,
            ToTeacher(),
            _departmentService,
            _teacherService, 
            _notificationService,
            _logger);
    }

    /// <summary>
    /// Validates the teacher data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("Имя обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Фамилия обязательна для заполнения");

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email обязателен для заполнения");
        else if (!IsValidEmail(Email))
            errors.Add("Некорректный формат email");

        if (!string.IsNullOrWhiteSpace(Phone) && !IsValidPhone(Phone))
            errors.Add("Некорректный формат телефона");

        if (DepartmentUid == Guid.Empty)
            errors.Add("Необходимо выбрать департамент");

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

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

    private bool IsValidPhone(string phone)
    {
        // Простая проверка телефона (можно расширить)
        var phoneRegex = new Regex(@"^[\+]?[1-9][\d]{0,15}$");
        return phoneRegex.IsMatch(phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
    }

    public override string ToString()
    {
        return $"TeacherViewModel: {FullName} ({Email})";
    }

    public override bool Equals(object? obj)
    {
        return obj is TeacherViewModel other && Uid == other.Uid;
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    #endregion
} 

