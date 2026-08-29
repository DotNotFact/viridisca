using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.System;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления курсами
/// </summary>
[Route("courses", 
    DisplayName = "Курсы", 
    IconKey = "BookOpenPageVariant", 
    Order = 4,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление курсами и дисциплинами")]
public class CourseViewModel : ViewModelBase
{
    #region Fields

    private readonly ICourseService _courseService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeacherService _teacherService; 
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    #endregion

    #region Properties

    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public Course? SelectedCourse { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public new string? ErrorMessage { get; set; }
    [Reactive] public new bool HasError { get; set; }

    // Фильтры
    [Reactive] public Department? SelectedDepartment { get; set; }
    [Reactive] public CourseType? SelectedCourseType { get; set; }
    [Reactive] public bool? IsActiveFilter { get; set; }

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<Course> Courses { get; set; } = new();
    [Reactive] public ObservableCollection<Department> Departments { get; set; } = new();
    [Reactive] public ObservableCollection<CourseType> CourseTypes { get; set; } = new();

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadCoursesCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCourseCommand { get; }
    public ReactiveCommand<Course, Unit> EditCourseCommand { get; }
    public ReactiveCommand<Course, Unit> DeleteCourseCommand { get; }
    public ReactiveCommand<Course, Unit> ViewCourseDetailsCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCoursesCommand { get; }
    public ReactiveCommand<Unit, Unit> ImportCoursesCommand { get; }

    #endregion

    #region Constructor

    public CourseViewModel(
        ICourseService courseService,
        IDepartmentService departmentService,
        ITeacherService teacherService, 
        INotificationService notificationService,
        IDialogService dialogService)
        : base()
    {
        _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _teacherService = teacherService; 
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Инициализация команд
        LoadCoursesCommand = ReactiveCommand.CreateFromTask(LoadCoursesAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        CreateCourseCommand = ReactiveCommand.CreateFromTask(CreateCourseAsync);
        EditCourseCommand = ReactiveCommand.CreateFromTask<Course>(EditCourseAsync);
        DeleteCourseCommand = ReactiveCommand.CreateFromTask<Course>(DeleteCourseAsync);
        ViewCourseDetailsCommand = ReactiveCommand.CreateFromTask<Course>(ViewCourseDetailsAsync);
        ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);
        ExportCoursesCommand = ReactiveCommand.CreateFromTask(ExportCoursesAsync);
        ImportCoursesCommand = ReactiveCommand.CreateFromTask(ImportCoursesAsync);

        // Инициализация типов курсов
        CourseTypes.Clear();
        foreach (var courseType in Enum.GetValues<CourseType>())
        {
            CourseTypes.Add(courseType);
        }

        // Подписка на изменения для автоматической фильтрации
        this.WhenAnyValue(
                x => x.SearchText,
                x => x.SelectedDepartment,
                x => x.SelectedCourseType,
                x => x.IsActiveFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LoadCoursesCommand.Execute().Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Подписка на изменения ошибок
        this.WhenAnyValue(x => x.ErrorMessage)
            .Select(error => !string.IsNullOrEmpty(error))
            .ToProperty(this, x => x.HasError)
            .DisposeWith(Disposables);

        // Загрузка данных при инициализации
        LoadInitialDataAsync().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Загружает начальные данные
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        try
        {
            await LoadDepartmentsAsync();
            await LoadCoursesAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке начальных данных");
            SetError("Ошибка при загрузке данных", ex);
        }
    }

    /// <summary>
    /// Загружает список департаментов
    /// </summary>
    private async Task LoadDepartmentsAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllAsync();
            Departments.Clear();
            Departments.AddRange(departments);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке департаментов");
            throw;
        }
    }

    /// <summary>
    /// Загружает список курсов с применением фильтров
    /// </summary>
    private async Task LoadCoursesAsync()
    {
        try
        {
            SetLoading(true, "Загрузка курсов...");
            ClearError();

            var courses = await _courseService.GetCoursesAsync();

            Courses.Clear();
            Courses.AddRange(courses);

            LogInfo($"Загружено {Count} курсов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке курсов");
            SetError("Ошибка при загрузке курсов", ex);
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Обновляет данные
    /// </summary>
    private async Task RefreshAsync()
    {
        try
        {
            SetLoading(true, "Обновление данных...");
            await LoadDepartmentsAsync();
            await LoadCoursesAsync();
            ShowSuccess("Данные обновлены");
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка обновления курса: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Создает новый курс
    /// </summary>
    private async Task CreateCourseAsync()
    {
        try
        {
            var result = await ShowCreateDialogAsync();
            if (result)
            {
                await LoadCoursesAsync();
                ShowSuccess("Курс успешно создан");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка создания курса: {ex.Message}");
        }
    }

    /// <summary>
    /// Редактирует курс
    /// </summary>
    private async Task EditCourseAsync(Course course)
    {
        try
        {
            var result = await ShowEditDialogAsync(course);
            if (result)
            {
                await LoadCoursesAsync();
                _notificationService.ShowSuccess("Курс успешно обновлен");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при редактировании курса");
            _notificationService.ShowError("Ошибка при редактировании курса");
        }
    }

    /// <summary>
    /// Удаляет курс
    /// </summary>
    private async Task DeleteCourseAsync(Course course)
    {
        try
        {
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления", 
                $"Вы уверены, что хотите удалить курс '{course.Name}'?");
            
            if (confirmResult)
            {
                await _courseService.DeleteAsync(course.Uid);
                await LoadCoursesAsync();
                ShowSuccess("Курс успешно удален");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении курса");
            _notificationService.ShowError("Ошибка при удалении курса");
        }
    }

    /// <summary>
    /// Показывает детали курса
    /// </summary>
    private async Task ViewCourseDetailsAsync(Course course)
    {
        try
        {
            await _dialogService.ShowMessageAsync(
                "Информация",
                $"Курс: {course.Name}\nОписание: {course.Description}\nКредиты: {course.Credits}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при просмотре деталей курса");
            _notificationService.ShowError("Ошибка при просмотре деталей курса");
        }
    }

    /// <summary>
    /// Очищает все фильтры
    /// </summary>
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDepartment = null;
        SelectedCourseType = null;
        IsActiveFilter = null;
    }

    /// <summary>
    /// Экспортирует курсы
    /// </summary>
    private async Task ExportCoursesAsync()
    {
        try
        {
            SetLoading(true, "Экспорт курсов...");
            
            // Здесь должна быть логика экспорта
            await Task.Delay(1000); // Заглушка
            
            _notificationService.ShowSuccess("Курсы успешно экспортированы");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при экспорте курсов");
            _notificationService.ShowError("Ошибка при экспорте курсов");
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Импортирует курсы
    /// </summary>
    private async Task ImportCoursesAsync()
    {
        try
        {
            SetLoading(true, "Импорт курсов...");
            
            // Здесь должна быть логика импорта
            await Task.Delay(1000); // Заглушка
            
            await LoadCoursesAsync();
            _notificationService.ShowSuccess("Курсы успешно импортированы");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при импорте курсов");
            _notificationService.ShowError("Ошибка при импорте курсов");
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Показывает диалог создания курса
    /// </summary>
    protected virtual async Task<bool> ShowCreateDialogAsync()
    {
        // Здесь должен быть вызов диалога создания курса
        // Пока что возвращаем false как заглушку
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Показывает диалог редактирования курса
    /// </summary>
    protected virtual async Task<bool> ShowEditDialogAsync(Course course)
    {
        // Здесь должен быть вызов диалога редактирования курса
        // Пока что возвращаем false как заглушку
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Устанавливает состояние загрузки
    /// </summary>
    private new void SetLoading(bool isLoading, string? message = null)
    {
        IsLoading = isLoading;
        IsBusy = isLoading;
        if (isLoading && !string.IsNullOrEmpty(message))
        {
            LogDebug($"Loading started: {Message}");
        }
        else if (!isLoading)
        {
            LogDebug("Loading finished");
        }
    }

    /// <summary>
    /// Устанавливает ошибку
    /// </summary>
    private new void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;
        HasError = true;
        
        if (exception != null)
        {
            LogError(exception, message);
        }
        else
        {
            LogError(message);
        }
    }

    /// <summary>
    /// Очищает ошибку
    /// </summary>
    private new void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    // Computed properties
    public int Count => Courses?.Count ?? 0;
    public string Message { get; set; } = string.Empty;

    #endregion
} 

