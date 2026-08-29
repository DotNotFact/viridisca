using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования задания
/// </summary>
public class AssignmentDialogViewModel : ViewModelBase
{
    private readonly IAssignmentService _assignmentService; 
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly INotificationService _notificationService;
    
    #region Commands

    public ReactiveCommand<Unit, Unit> LoadCourseInstancesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; } = null!;

    #endregion

    #region Properties

    [Reactive] public string AssignmentTitle { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Instructions { get; set; } = string.Empty;
    [Reactive] public DateTime DueDate { get; set; }
    [Reactive] public double MaxScore { get; set; }
    [Reactive] public AssignmentType Type { get; set; }
    [Reactive] public AssignmentDifficulty Difficulty { get; set; }
    [Reactive] public AssignmentStatus Status { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public Course? Course { get; set; }
    [Reactive] public Assignment? Entity { get; set; }
    [Reactive] public Guid? CourseInstanceUid { get; set; }

    #endregion

    #region Collections

    public ObservableCollection<Course> AvailableCourses { get; } = new();
    public ObservableCollection<CourseInstance> CourseInstances { get; } = new();

    #endregion

    #region Constructor

    public AssignmentDialogViewModel(
        IAssignmentService assignmentService,
        ICourseInstanceService courseInstanceService, 
        INotificationService notificationService,
        ILogger<AssignmentDialogViewModel> logger)
    {
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService)); 
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Команды уже инициализированы в базовом классе
        LoadCourseInstancesCommand = ReactiveCommand.CreateFromTask(LoadCourseInstancesAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);
        
        // Инициализация коллекций
        CourseInstances = [];

        this.WhenAnyValue(x => x.Entity)
            .Where(x => x != null);
    }

    #endregion

    #region Overrides

    protected void InitializeFromEntity(Assignment entity)
    {
        Entity = entity;
        AssignmentTitle = entity.Title ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        DueDate = entity.DueDate ?? DateTime.Now.AddDays(7);
        MaxScore = (double)entity.MaxScore;
        Type = entity.Type;
        Status = entity.Status;
        
        Title = "Редактирование задания";
    }

    protected void InitializeNew()
    {
        Entity = new Assignment();
        AssignmentTitle = string.Empty;
        Description = string.Empty;
        DueDate = DateTime.Now.AddDays(7);
        MaxScore = 100.0;
        Type = AssignmentType.Homework;
        Status = AssignmentStatus.Draft;
        
        Title = "Создание задания";
    }

    protected async Task<Assignment?> SaveEntityAsync()
    {
        if (Entity == null)
        {
            Entity = new Assignment();
        }

        Entity.Title = AssignmentTitle;
        Entity.Description = Description;
        Entity.DueDate = DueDate;
        Entity.MaxScore = MaxScore;
        Entity.Type = Type;
        Entity.Status = Status;
        
        if (Course != null)
        {
            Entity.CourseInstanceUid = Course.Uid;
        }
        
        Entity.LastModifiedAt = DateTime.UtcNow;

        if (IsEditMode)
        {
            return await _assignmentService.UpdateAsync(Entity);
        }
        else
        {
            return await _assignmentService.CreateAsync(Entity);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания нового задания
    /// </summary>
    public async Task InitializeAsync()
    {
        Initialize(null);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующего задания
    /// </summary>
    public async Task InitializeAsync(Assignment assignment)
    {
        Initialize(assignment);
        await Task.CompletedTask;
    }

    #endregion

    #region Methods

    private void ValidateInternal()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(AssignmentTitle))
            errors.Add("Необходимо указать название задания");
            
        if (Course == null)
            errors.Add("Необходимо выбрать курс");
            
        if (DueDate < DateTime.Now)
            errors.Add("Дата сдачи не может быть в прошлом");
            
        if (MaxScore <= 0 || MaxScore > 1000)
            errors.Add("Некорректное максимальное количество баллов");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    private async Task LoadCourseInstancesAsync()
    {
        try
        {
            // Загружаем экземпляры курсов
            var courseInstances = await _courseInstanceService.GetAllAsync();
            
            AvailableCourses.Clear();
            foreach (var courseInstance in courseInstances)
            {
                if (courseInstance.Subject != null)
                {
                    var course = new Course
                    {
                        Uid = courseInstance.Uid,
                        Name = courseInstance.Subject.Name,
                        Code = courseInstance.Subject.Code
                    };
                    AvailableCourses.Add(course);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось загрузить курсы: {ex.Message}");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            ValidateInternal();
            if (!string.IsNullOrEmpty(ValidationError))
            {
                return;
            }

            SetLoading(true, "Сохранение задания...");
            Entity = await SaveEntityAsync();
            
            ShowSuccess("Задание успешно сохранено");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка сохранения задания");
            ShowError("Не удалось сохранить задание");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void Cancel()
    {
        // Логика отмены - можно добавить проверку изменений
    }

    private void Initialize(Assignment? assignment)
    {
        // Инициализация ViewModel
        LoadCourseInstancesAsync();
    }

    public void LoadAssignment(Assignment assignment)
    {
        Entity = assignment;
        IsEditMode = true;
        Title = "Редактирование задания";
        
        // Загрузка данных из assignment
        AssignmentTitle = assignment.Title;
        Description = assignment.Description;
        DueDate = assignment.DueDate ?? DateTime.Now.AddDays(7);
        MaxScore = assignment.MaxScore;
        Type = assignment.Type;
        Status = assignment.Status;
        CourseInstanceUid = assignment.CourseInstanceUid;
        
        Initialize(assignment);
    }

    #endregion
} 

