using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования записи на курс
/// </summary>
public class EnrollmentDialogViewModel : RoutableViewModelBase 
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IStudentService _studentService;
    private readonly ICourseInstanceService _courseInstanceService;

    #region Properties

    [Reactive] public Guid EnrollmentUid { get; set; }
    [Reactive] public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime? CompletionDate { get; set; }
    [Reactive] public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    [Reactive] public decimal? FinalGrade { get; set; }
    [Reactive] public string Notes { get; set; } = string.Empty;
    [Reactive] public bool IsActive { get; set; } = true;

    [Reactive] public Student? Student { get; set; }
    [Reactive] public CourseInstance? Course { get; set; }

    [Reactive] public string StudentSearchText { get; set; } = string.Empty;
    [Reactive] public string CourseSearchText { get; set; } = string.Empty;

    [Reactive] public bool IsLoadingStudents { get; set; }
    [Reactive] public bool IsLoadingCourses { get; set; }

    [Reactive] public ObservableCollection<Student> Students { get; set; } = [];
    [Reactive] public ObservableCollection<CourseInstance> CourseInstances { get; set; } = [];
    [Reactive] public bool IsEditMode { get; set; }

    #endregion

    #region Collections

    public ObservableCollection<Student> AvailableStudents { get; } = new();
    public ObservableCollection<CourseInstance> AvailableCourses { get; } = new();

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadCoursesCommand { get; }

    #endregion

    #region Computed Properties

    public string DialogTitle => IsEditMode ? "Редактирование записи" : "Новая запись на курс";

    #endregion

    #region Constructor

    public EnrollmentDialogViewModel(
        IEnrollmentService enrollmentService,
        IStudentService studentService,
        ICourseInstanceService courseInstanceService, 
        ILogger<EnrollmentDialogViewModel> logger)
        : base(null, logger, null)
    {
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService)); 

        // Commands
        LoadStudentsCommand = CreateCommand(LoadStudentsAsync, null, "Ошибка загрузки студентов");
        LoadCoursesCommand = CreateCommand(LoadCoursesAsync, null, "Ошибка загрузки курсов");

        // Setup validation
        SetupValidation();
        
        // Setup reactive subscriptions
        SetupReactiveSubscriptions();

        // Load initial data
        LoadStudentsCommand.Execute().Subscribe(_ => { }, _ => { });
        LoadCoursesCommand.Execute().Subscribe(_ => { }, _ => { });
    }

    #endregion

    #region Initialization

    protected void InitializeFromEntity(Enrollment enrollment)
    {
        EnrollmentUid = enrollment.Uid;
        EnrollmentDate = enrollment.EnrollmentDate;
        CompletionDate = enrollment.CompletionDate;
        Status = enrollment.Status;
        FinalGrade = enrollment.FinalGrade;
        Notes = enrollment.Notes ?? string.Empty;
        IsActive = enrollment.IsActive;

        // Set related entities if available
        Student = enrollment.Student;
        Course = enrollment.CourseInstance;

        Title = "Редактирование записи";
        LogDebug($"Initialized enrollment dialog from entity {EnrollmentUid}");
    }

    protected void InitializeNew()
    {
        EnrollmentUid = Guid.NewGuid();
        EnrollmentDate = DateTime.Now;
        CompletionDate = null;
        Status = EnrollmentStatus.Active;
        FinalGrade = null;
        Notes = string.Empty;
        IsActive = true;
        Student = null;
        Course = null;

        Title = "Новая запись на курс";
        LogDebug("Initialized new enrollment dialog");
    }

    #endregion

    #region Validation

    private void SetupValidation()
    {
        // Validate required fields
        this.WhenAnyValue(
                x => x.Student,
                x => x.Course,
                x => x.EnrollmentDate,
                x => x.Status)
            .Subscribe(_ => Validate())
            .DisposeWith(Disposables);
    }

    protected bool Validate()
    {
        base.Validate();

        if (Student == null)
        {
            SetValidationError("Необходимо выбрать студента");
            return false;
        }

        if (Course == null)
        {
            SetValidationError("Необходимо выбрать курс");
            return false;
        }

        if (EnrollmentDate > DateTime.Now)
        {
            SetValidationError("Дата записи не может быть в будущем");
            return false;
        }

        if (CompletionDate.HasValue && CompletionDate.Value < EnrollmentDate)
        {
            SetValidationError("Дата завершения не может быть раньше даты записи");
            return false;
        }

        if (Status == EnrollmentStatus.Completed && !CompletionDate.HasValue)
        {
            SetValidationError("Для завершенной записи необходимо указать дату завершения");
            return false;
        }

        if (FinalGrade.HasValue && (FinalGrade.Value < 0 || FinalGrade.Value > 100))
        {
            SetValidationError("Итоговая оценка должна быть от 0 до 100");
            return false;
        }

        ValidationError = null;
        return true;
    }

    #endregion

    #region Reactive Subscriptions

    private void SetupReactiveSubscriptions()
    {
        // Auto-set completion date when status changes to completed
        this.WhenAnyValue(x => x.Status)
            .Where(status => status == EnrollmentStatus.Completed)
            .Where(_ => !CompletionDate.HasValue)
            .Subscribe(_ => CompletionDate = DateTime.Now)
            .DisposeWith(Disposables);

        // Clear completion date when status is not completed
        this.WhenAnyValue(x => x.Status)
            .Where(status => status != EnrollmentStatus.Completed)
            .Subscribe(_ => CompletionDate = null)
            .DisposeWith(Disposables);

        // Update title based on mode
        this.WhenAnyValue(x => x.IsEditMode)
            .Subscribe(isEdit => Title = isEdit ? "Редактирование записи" : "Новая запись на курс")
            .DisposeWith(Disposables);
    }

    #endregion

    #region Data Loading

    private async Task LoadStudentsAsync()
    {
        try
        {
            var students = await _studentService.GetAllAsync();
            Students.Clear();
            Students.AddRange(students);
        }
        catch (Exception ex)
        {
            LogError($"Ошибка загрузки студентов: {ex.Message}");
        }
    }

    private async Task LoadCoursesAsync()
    {
        try
        {
            var courseInstances = await _courseInstanceService.GetAllAsync();
            CourseInstances.Clear();
            CourseInstances.AddRange(courseInstances);
        }
        catch (Exception ex)
        {
            LogError($"Ошибка загрузки экземпляров курсов: {ex.Message}");
        }
    }

    #endregion

    #region Save/Cancel

    protected async Task<Enrollment> SaveEntityAsync()
    {
        if (Student == null || Course == null)
        {
            ShowError("Необходимо выбрать студента и курс");
            return null;
        }

        try
        {
            var enrollment = new Enrollment
            {
                Uid = EnrollmentUid,
                StudentUid = Student?.Uid ?? Guid.Empty,
                CourseInstanceUid = Course?.Uid ?? Guid.Empty,
                EnrollmentDate = EnrollmentDate,
                CompletionDate = CompletionDate,
                Status = Status,
                FinalGrade = FinalGrade,
                Notes = Notes
            };

            if (IsEditMode)
            {
                LogInfo($"Updating enrollment {EnrollmentUid}");
                return await _enrollmentService.UpdateAsync(enrollment);
            }
            else
            {
                LogInfo($"Creating new enrollment");
                return await _enrollmentService.CreateAsync(enrollment);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to save enrollment");
            return null;
        }
    }

    protected async Task CancelAsync()
    {
        // Close dialog logic here
    }

    #endregion
} 

