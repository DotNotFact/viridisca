using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для редактирования задания
/// </summary>
public class AssignmentEditorViewModel : ViewModelBase
{
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAssignmentService _assignmentService;
    private readonly SourceCache<CourseInstance, Guid> _courseInstancesSource = new(c => c.Uid);
    private ReadOnlyObservableCollection<CourseInstance> _courseInstances;

    public ReadOnlyObservableCollection<CourseInstance> CourseInstances => _courseInstances;
    public ObservableCollection<CourseInstance> AvailableCourseInstances { get; } = new();

    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string? Instructions { get; set; }
    [Reactive] public AssignmentType Type { get; set; } = AssignmentType.Homework;
    [Reactive] public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    [Reactive] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
    [Reactive] public DateTime? StartDate { get; set; } = DateTime.Today;
    [Reactive] public decimal MaxScore { get; set; }
    [Reactive] public int? TimeLimit { get; set; }
    [Reactive] public int MaxAttempts { get; set; } = 1;
    [Reactive] public bool AllowLateSubmission { get; set; } = true;
    [Reactive] public decimal? LatePenalty { get; set; }
    [Reactive] public Guid CourseId { get; set; }
    [Reactive] public CourseInstance? SelectedCourseInstance { get; set; }
    [Reactive] public string? Resources { get; set; }
    [Reactive] public string? GradingCriteria { get; set; }
    [Reactive] public AssignmentDifficulty Difficulty { get; set; } = AssignmentDifficulty.Easy;

    [Reactive] public bool IsLoading { get; set; }
    [ObservableAsProperty] public bool IsValid { get; }
    [ObservableAsProperty] public bool CanSave { get; }

    public string WindowTitle => Assignment == null ? "Добавить задание" : "Редактировать задание";

    public Assignment? Assignment { get; set; }
    public Assignment? Result { get; set; }

    public ReactiveCommand<Unit, Assignment?> SaveCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

    // Предопределенные типы заданий
    public ObservableCollection<AssignmentType> AssignmentTypes { get; } = new()
    {
        AssignmentType.Homework,
        AssignmentType.Quiz,
        AssignmentType.Exam,
        AssignmentType.Project,
        AssignmentType.LabWork
    };

    public ObservableCollection<AssignmentDifficulty> DifficultyLevels { get; } = new()
    {
        AssignmentDifficulty.Easy,
        AssignmentDifficulty.Medium,
        AssignmentDifficulty.Hard
    };

    public ObservableCollection<AssignmentStatus> StatusOptions { get; } = new()
    {
        AssignmentStatus.Draft,
        AssignmentStatus.Published,
        AssignmentStatus.Closed,
        AssignmentStatus.Archived
    };

    public AssignmentEditorViewModel(ICourseInstanceService courseInstanceService, IAssignmentService assignmentService, Assignment? assignment = null)
    {
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        Assignment = assignment;

        // decimal field initializers on [Reactive] properties corrupt ReactiveUI.Fody's
        // IL weaving (see TeacherEditorViewModel.HourlyRate) - assign the default here instead.
        MaxScore = 100;

        // Инициализация из существующего задания
        if (assignment != null)
        {
            Title = assignment.Title;
            Description = assignment.Description;
            Instructions = assignment.Instructions;
            DueDate = assignment.DueDate ?? DateTime.Today.AddDays(7);
            MaxScore = (decimal)assignment.MaxScore;
            Type = assignment.Type;
            Difficulty = assignment.Difficulty;
            Status = assignment.Status;
            CourseId = assignment.CourseInstanceUid;
            SelectedCourseInstance = CourseInstances.FirstOrDefault(c => c.Uid == assignment.CourseInstanceUid);
        }

        // Команды
        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                LoadCourses();
                
                if (Assignment.Uid == Guid.Empty)
                {
                    Assignment.Uid = Guid.NewGuid();
                    Assignment.CreatedAt = DateTime.UtcNow;
                    await _assignmentService.CreateAsync(Assignment);
                }
                else
                {
                    Assignment.LastModifiedAt = DateTime.UtcNow;
                    await _assignmentService.UpdateAsync(Assignment);
                }
                
                return Assignment;
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при сохранении задания");
                return null;
            }
        });
        CancelCommand = ReactiveCommand.Create(() => { });

        SetupSubscriptions();
        LoadDataAsync();
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Sync CourseId and SelectedCourse
        this.WhenAnyValue(x => x.SelectedCourseInstance)
            .Subscribe(course => CourseId = course?.Uid ?? Guid.Empty);

        // Bind courses to observable collection
        _courseInstancesSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _courseInstances)
            .Subscribe();

        // Auto-set start date when due date changes
        this.WhenAnyValue(x => x.DueDate)
            .Where(dueDate => StartDate == null || StartDate >= dueDate)
            .Subscribe(dueDate => StartDate = dueDate.AddDays(-7));

        // Validation
        var canSave = this.WhenAnyValue(
            x => x.Title,
            x => x.Description,
            x => x.DueDate,
            x => x.MaxScore,
            x => x.SelectedCourseInstance,
            (title, description, dueDate, maxScore, course) =>
                !string.IsNullOrWhiteSpace(title) &&
                !string.IsNullOrWhiteSpace(description) &&
                dueDate > DateTime.Today &&
                maxScore > 0 &&
                course != null
        );

        canSave.ToPropertyEx(this, x => x.CanSave);
    }

    /// <summary>
    /// Загружает данные для выпадающих списков
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            await LoadCoursesAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке курсов");
        }
    }

    /// <summary>
    /// Загружает список курсов
    /// </summary>
    private async Task LoadCoursesAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllCourseInstancesAsync();
            _courseInstancesSource.AddOrUpdate(courses);
            
            // Set selected course if editing existing assignment
            if (Assignment?.CourseInstanceUid != Guid.Empty)
            {
                SelectedCourseInstance = courses.FirstOrDefault(c => c.Uid == Assignment.CourseInstanceUid);
            }
        }
        catch (Exception ex)
        {
            SetError("Ошибка загрузки курсов", ex);
        }
    }

    private void LoadCourses()
    {
        try
        {
            // Заглушка для загрузки курсов - используем существующий SourceCache
            // _courses уже инициализирован через _coursesSource.Connect().Bind()
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке курсов");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (Assignment == null)
            {
                Assignment = new Assignment();
            }

            Assignment.Title = Title;
            Assignment.Description = Description;
            Assignment.Instructions = Instructions;
            Assignment.DueDate = DueDate;
            Assignment.MaxScore = (double)MaxScore;
            Assignment.Type = Type;
            Assignment.Difficulty = Difficulty;
            Assignment.Status = Status;
            Assignment.CourseInstanceUid = SelectedCourseInstance?.Uid ?? Guid.Empty;

            Result = Assignment;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при сохранении задания");
        }
    }

    private async Task SaveAssignmentAsync()
    {
        try
        {
            IsLoading = true;
            
            if (Assignment.Uid == Guid.Empty)
            {
                // Создание нового задания
                Assignment.Uid = Guid.NewGuid();
                Assignment.CreatedAt = DateTime.UtcNow;
                
                var result = await _assignmentService.CreateAsync(Assignment);
                if (result != null)
                {
                    Assignment = result;
                    ShowSuccess("Задание успешно создано");
                    LogInfo($"Сохранение задания: {Title}");
                }
            }
            else
            {
                // Обновление существующего задания
                Assignment.LastModifiedAt = DateTime.UtcNow;
                
                var updateResult = await _assignmentService.UpdateAsync(Assignment);
                if (updateResult != null)
                {
                    ShowSuccess("Задание успешно обновлено");
                    LogInfo($"Сохранение задания: {Title}");
                }
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Ошибка валидации при сохранении задания");
            ShowError($"Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при сохранении задания");
            ShowError("Не удалось сохранить задание. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCourseInstancesAsync()
    {
        try
        {
            var courseInstances = await _courseInstanceService.GetAllAsync();
            
            AvailableCourseInstances.Clear();
            foreach (var courseInstance in courseInstances)
            {
                AvailableCourseInstances.Add(courseInstance);
            }
            
            LogInfo($"Загружено {courseInstances.Count()} экземпляров курсов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки экземпляров курсов");
            ShowError("Ошибка загрузки списка курсов");
        }
    }
} 

