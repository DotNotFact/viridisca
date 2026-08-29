using System;
using System.Collections.Generic;
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
using ViridiscaUi.ViewModels.System;
using DynamicData;
using DynamicData.Binding;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления заданиями
/// </summary>
/// <remarks>
/// Was previously duplicating AssignmentsViewModel's "assignments" route - both classes
/// derive from RoutableViewModelBase, so registering the same path twice made whichever
/// one won route-scan order (non-deterministic across rebuilds) silently unreachable,
/// since only AssignmentsViewModel has a ReactiveViewLocator mapping. Not a page - no
/// [Route] here.
/// </remarks>
public class AssignmentViewModel : RoutableViewModelBase
{
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAssignmentService _assignmentService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<AssignmentViewModel> _logger;

    #region Properties

    [Reactive] public Assignment Assignment { get; set; } = new();

    // Основные свойства задания
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public DateTime? DueDate { get; set; }
    [Reactive] public double MaxScore { get; set; } = 100.0;
    [Reactive] public AssignmentType Type { get; set; } = AssignmentType.Homework;
    [Reactive] public AssignmentDifficulty Difficulty { get; set; } = AssignmentDifficulty.Medium;
    [Reactive] public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public string Instructions { get; set; } = string.Empty;
    [Reactive] public bool IsPublished { get; set; } = false;
    [Reactive] public Guid CourseInstanceUid { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    // Связанные данные курса
    [Reactive] public Guid CourseUid { get; set; }
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string CourseCode { get; set; } = string.Empty;

    // UI состояние
    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    // Коллекции для управления заданиями
    [Reactive] public ObservableCollection<AssignmentViewModel> Assignments { get; set; } = new();
    [Reactive] public AssignmentViewModel? SelectedAssignment { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 15;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalAssignments { get; set; }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadAssignmentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<Assignment, Unit> EditAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<Assignment, Unit> DeleteAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<Assignment, Unit> ViewAssignmentDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Детали задания для отображения
    /// </summary>
    public string AssignmentDetails => $"{Title} ({CourseCode})";

    /// <summary>
    /// Детали курса для отображения
    /// </summary>
    public string CourseDetails => $"{CourseName} ({CourseCode})";

    /// <summary>
    /// Текстовое представление типа задания
    /// </summary>
    public string TypeText => Type switch
    {
        AssignmentType.Homework => "Домашнее задание",
        AssignmentType.Quiz => "Тест",
        AssignmentType.Exam => "Экзамен",
        AssignmentType.Project => "Проект",
        AssignmentType.LabWork => "Лабораторная работа",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Текстовое представление статуса задания
    /// </summary>
    public string StatusText => Status switch
    {
        AssignmentStatus.Draft => "Черновик",
        AssignmentStatus.Published => "Опубликовано",
        AssignmentStatus.Completed => "Завершено",
        AssignmentStatus.Overdue => "Просрочено",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Цвет для отображения статуса
    /// </summary>
    public string StatusColor => Status switch
    {
        AssignmentStatus.Draft => "Orange",
        AssignmentStatus.Published => "Green",
        AssignmentStatus.Completed => "Blue",
        AssignmentStatus.Overdue => "Red",
        _ => "Gray"
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Основной конструктор
    /// </summary>
    public AssignmentViewModel(
        IScreen hostScreen,
        IUnifiedNavigationService navigationService, 
        INotificationService notificationService,
        IAssignmentService assignmentService,
        ICourseInstanceService courseInstanceService,
        IDialogService dialogService,
        ILogger<AssignmentViewModel> logger)
        : base(hostScreen)
    {
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService)); 
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Title = "Задания";
        
        InitializeCommands();
        SetupSubscriptions();
    }

    /// <summary>
    /// Конструктор с заданием
    /// </summary>
    public AssignmentViewModel(Assignment assignment, ILogger<AssignmentViewModel> logger) : base(null!)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));
        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Uid must be set before UpdateFromAssignment - it guards on assignment.Uid == Uid to
        // avoid overwriting the wrong tracked instance, and defaults to Guid.Empty otherwise.
        Uid = assignment.Uid;
        UpdateFromAssignment(assignment);
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Простой конструктор для использования в коллекциях
    /// </summary>
    public AssignmentViewModel(Assignment assignment) : base(null!)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));
            
        Assignment = assignment;
        Uid = assignment.Uid;
        Title = assignment.Title;
        Description = assignment.Description;
        DueDate = assignment.DueDate;
        MaxScore = assignment.MaxScore;
        Type = assignment.Type;
        Difficulty = assignment.Difficulty;
        Status = assignment.Status;
        Instructions = assignment.Instructions;
        IsPublished = assignment.IsPublished;
        CourseInstanceUid = assignment.CourseInstanceUid;
        CreatedAt = assignment.CreatedAt;
        LastModifiedAt = assignment.LastModifiedAt;
        
        // Получаем данные из связанной модели CourseInstance
        if (assignment.CourseInstance != null)
        {
            CourseInstanceUid = assignment.CourseInstance.Uid;
            if (assignment.CourseInstance.Subject != null)
            {
                CourseName = assignment.CourseInstance.Subject.Name;
                CourseCode = assignment.CourseInstance.Subject.Code;
            }
        }
        
        SetupPropertyChangeNotifications();
    }

    #endregion

    #region Initialization

    private void InitializeCommands()
    {
        LoadAssignmentsCommand = ReactiveCommand.CreateFromTask(LoadAssignmentsAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        CreateAssignmentCommand = ReactiveCommand.CreateFromTask(CreateAssignmentAsync);
        EditAssignmentCommand = ReactiveCommand.CreateFromTask<Assignment>(EditAssignmentAsync);
        DeleteAssignmentCommand = ReactiveCommand.CreateFromTask<Assignment>(DeleteAssignmentAsync);
        ViewAssignmentDetailsCommand = ReactiveCommand.CreateFromTask<Assignment>(ViewAssignmentDetailsAsync);
        SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchAsync);
        GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
        NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync);
        PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync);
        FirstPageCommand = ReactiveCommand.CreateFromTask(FirstPageAsync);
        LastPageCommand = ReactiveCommand.CreateFromTask(LastPageAsync);
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async searchText => 
            {
                if (!IsLoading)
                {
                    await SearchAsync(searchText ?? string.Empty);
                }
            })
            .DisposeWith(Disposables);
    }

    #endregion

    #region Command Methods

    private async Task LoadAssignmentsAsync()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        try
        {
            var (assignments, totalCount) = await _assignmentService.GetPagedAsync(
                CurrentPage, PageSize, SearchText);
            
            Assignments.Clear();
            foreach (var assignment in assignments)
            {
                Assignments.Add(new AssignmentViewModel(assignment));
            }

            TotalAssignments = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            
            _logger.LogInformation("Loaded {Count} assignments", assignments.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assignments");
            ShowError("Не удалось загрузить задания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadAssignmentsAsync();
        ShowSuccess("Данные обновлены");
    }

    private async Task CreateAssignmentAsync()
    {
        try
        {
            var newAssignment = new Assignment
            {
                Title = "Новое задание",
                Description = "",
                Type = AssignmentType.Homework,
                Status = AssignmentStatus.Draft,
                MaxScore = 100,
                DueDate = DateTime.Now.AddDays(7)
            };

            var result = await _dialogService.ShowAssignmentEditDialogAsync(newAssignment);
            if (result != null)
            {
                var created = await _assignmentService.CreateAsync(result);
                await LoadAssignmentsAsync();
                ShowSuccess($"Задание '{created.Title}' создано");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignment");
            ShowError("Не удалось создать задание");
        }
    }

    private async Task EditAssignmentAsync(Assignment assignment)
    {
        try
        {
            var result = await _dialogService.ShowAssignmentEditDialogAsync(assignment);
            if (result != null)
            {
                await _assignmentService.UpdateAsync(result);
                await LoadAssignmentsAsync();
                ShowSuccess($"Задание '{result.Title}' обновлено");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing assignment");
            ShowError("Не удалось обновить задание");
        }
    }

    private async Task DeleteAssignmentAsync(Assignment assignment)
    {
        try
        {
            var confirmation = await _dialogService.ShowConfirmationAsync(
                "Удаление задания",
                $"Вы уверены, что хотите удалить задание '{assignment.Title}'?");

            if (confirmation)
            {
                await _assignmentService.DeleteAsync(assignment.Uid);
                await LoadAssignmentsAsync();
                ShowSuccess($"Задание '{assignment.Title}' удалено");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment");
            ShowError("Не удалось удалить задание");
        }
    }

    private async Task ViewAssignmentDetailsAsync(Assignment assignment)
    {
        try
        {
            await _dialogService.ShowAssignmentDetailsDialogAsync(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing assignment details");
            ShowError("Не удалось открыть детали задания");
        }
    }

    private async Task SearchAsync(string searchText)
    {
        SearchText = searchText;
        CurrentPage = 1;
        await LoadAssignmentsAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadAssignmentsAsync();
        }
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            await GoToPageAsync(CurrentPage + 1);
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await GoToPageAsync(CurrentPage - 1);
        }
    }

    private async Task FirstPageAsync()
    {
        if (CurrentPage > 1)
        {
            await GoToPageAsync(1);
        }
    }

    private async Task LastPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            await GoToPageAsync(TotalPages);
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
        this.WhenAnyValue(x => x.Title, x => x.CourseCode)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(AssignmentDetails));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.CourseName, x => x.CourseCode)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CourseDetails));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Type)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TypeText));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from an Assignment domain model
    /// </summary>
    public void UpdateFromAssignment(Assignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));
            
        if (assignment.Uid != Uid)
            throw new ArgumentException("Cannot update from assignment with different UID", nameof(assignment));
            
        Assignment = assignment;
        Uid = assignment.Uid;
        Title = assignment.Title;
        Description = assignment.Description;
        DueDate = assignment.DueDate;
        MaxScore = assignment.MaxScore;
        Type = assignment.Type;
        Difficulty = assignment.Difficulty;
        Status = assignment.Status;
        Instructions = assignment.Instructions;
        IsPublished = assignment.IsPublished;
        CourseInstanceUid = assignment.CourseInstanceUid;
        CreatedAt = assignment.CreatedAt;
        LastModifiedAt = assignment.LastModifiedAt;
        
        // Получаем данные из связанной модели CourseInstance
        if (assignment.CourseInstance != null)
        {
            CourseInstanceUid = assignment.CourseInstance.Uid;
            if (assignment.CourseInstance.Subject != null)
            {
                CourseName = assignment.CourseInstance.Subject.Name;
                CourseCode = assignment.CourseInstance.Subject.Code;
            }
        }
    }

    /// <summary>
    /// Converts this ViewModel back to an Assignment domain model
    /// </summary>
    public Assignment ToAssignment()
    {
        var assignment = new Assignment
        {
            Uid = Uid,
            Title = Title,
            Description = Description,
            DueDate = DueDate,
            MaxScore = MaxScore,
            Type = Type,
            Status = Status,
            CourseInstanceUid = CourseInstanceUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return assignment;
    }

    /// <summary>
    /// Validates the assignment data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Необходимо указать название задания");
            
        // Business logic validation
        if (DueDate < DateTime.Now)
            errors.Add("Дата сдачи не может быть в прошлом");
            
        if (MaxScore <= 0 || MaxScore > 100)
            errors.Add("Некорректное максимальное количество баллов");
            
        if (Status == AssignmentStatus.Published && string.IsNullOrWhiteSpace(Description))
            warnings.Add("Рекомендуется добавить описание задания");
            
        if (Status == AssignmentStatus.Published && !IsActive)
            warnings.Add("Опубликованное задание не может быть неактивным");
        
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
    /// Returns a string representation of the assignment
    /// </summary>
    public override string ToString()
    {
        return $"{Title} ({CourseCode})";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is AssignmentViewModel other && Uid.Equals(other.Uid);
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

