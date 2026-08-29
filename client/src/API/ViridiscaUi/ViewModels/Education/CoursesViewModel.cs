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
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления курсами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("courses",
    DisplayName = "Курсы",
    IconKey = "BookOpenPageVariant",
    Order = 4,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление курсами и программами")]
public class CoursesViewModel : RoutableViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly ISubjectService _subjectService;
    private readonly IDepartmentService _departmentService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly IGroupService _groupService;

    // === СВОЙСТВА ===

    [Reactive] public ObservableCollection<CourseInstanceViewModel> CourseInstances { get; set; } = new();
    [Reactive] public CourseInstanceViewModel? SelectedCourseInstance { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public CourseAnalytics? Statistics { get; set; }

    // Фильтры
    [Reactive] public string? CategoryFilter { get; set; }
    [Reactive] public string? StatusFilter { get; set; }
    [Reactive] public string? DifficultyFilter { get; set; }
    [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
    [Reactive] public TeacherViewModel? SelectedTeacherFilter { get; set; }

    // Добавляем коллекции для фильтров
    [Reactive] public ObservableCollection<string> Categories { get; set; } = new();
    [Reactive] public ObservableCollection<string> Difficulties { get; set; } = new();
    [Reactive] public ObservableCollection<string> Statuses { get; set; } = new();

    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 15;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalCourses { get; set; }
    [Reactive] public int ActiveCourses { get; set; }

    // Computed properties for UI binding
    public bool HasSelectedCourseInstance => SelectedCourseInstance != null;
    public bool HasSelectedCourseInstanceStatistics => Statistics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public bool HasPages => TotalPages > 1;
    public bool CanGoToFirstPage => CurrentPage > 1;
    public bool CanGoToLastPage => CurrentPage < TotalPages;
    public string PaginationInfo => TotalCourses == 0
        ? "Нет данных"
        : $"Показано {(CurrentPage - 1) * PageSize + 1}-{Math.Min(CurrentPage * PageSize, TotalCourses)} из {TotalCourses}";

    /// <summary>
    /// Суммарное количество записей на курсы среди загруженных экземпляров курсов
    /// </summary>
    [Reactive] public int TotalEnrollments { get; set; }

    /// <summary>
    /// Количество выбранных курсов (для панели массовых действий)
    /// </summary>
    [Reactive] public int SelectedCoursesCount { get; set; }

    // === КОМАНДЫ ===

    public ReactiveCommand<Unit, Unit> LoadCourseInstancesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> EditCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> DeleteCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ViewCourseInstanceDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ViewStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ManageEnrollmentsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ManageContentCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> CloneCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkArchiveCommand { get; private set; } = null!;
    // BulkEditCommand and BulkExportCommand intentionally left unwired: there is no generic
    // per-item "edit" operation to batch (needs a dedicated multi-record edit dialog), and
    // ICourseInstanceService exposes no export operation to loop over - see report.
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    /// <summary>
    /// Количество курсов
    /// </summary>
    [Reactive] public int CourseCount { get; set; }

    /// <summary>
    /// Путь к файлу
    /// </summary>
    [Reactive] public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Конструктор
    /// </summary>
    public CoursesViewModel(
        IScreen hostScreen,
        ICourseService courseService,
        ICourseInstanceService courseInstanceService,
        ISubjectService subjectService,
        IDepartmentService departmentService,
        INotificationService notificationService,
        IDialogService dialogService,
        IStatusService statusService,
        ITeacherService teacherService,
        IStudentService studentService,
        IGroupService groupService) : base(hostScreen)
    {
        _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));

        InitializeCommands();
        SetupSubscriptions();

        LogInfo("CoursesViewModel initialized");
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadCourseInstancesCommand = CreateCommand(async () => await LoadCourseInstancesAsync(), null, "Ошибка загрузки курсов");
        RefreshCommand = CreateCommand(async () => await RefreshAsync(), null, "Ошибка обновления данных");
        CreateCourseInstanceCommand = CreateCommand(async () => await CreateCourseInstanceAsync(), null, "Ошибка создания курса");
        EditCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await EditCourseInstanceAsync(course), null, "Ошибка редактирования курса");
        DeleteCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await DeleteCourseInstanceAsync(course), null, "Ошибка удаления курса");
        ViewCourseInstanceDetailsCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await ViewCourseInstanceDetailsAsync(course), null, "Ошибка просмотра деталей курса");
        ViewStatisticsCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await ViewStatisticsAsync(course), null, "Ошибка просмотра статистики");
        ManageEnrollmentsCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await ManageEnrollmentsAsync(course), null, "Ошибка управления записями");
        ManageContentCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await ManageContentAsync(course), null, "Ошибка управления контентом");
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchCoursesAsync(searchTerm), null, "Ошибка поиска курсов");
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync(), null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync(), null, "Ошибка очистки фильтров");
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page), null, "Ошибка навигации по страницам");

        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);

        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), null, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), null, "Ошибка перехода на последнюю страницу");
        CloneCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(async (course) => await CloneCourseInstanceAsync(course), null, "Ошибка клонирования курса");
        ImportCoursesCommand = CreateCommand(async () => await ImportCoursesAsync(), null, "Ошибка импорта курсов");
        ExportReportCommand = CreateCommand(async () => await ExportReportAsync(), null, "Ошибка экспорта отчета");

        var hasSelection = this.WhenAnyValue(x => x.SelectedCoursesCount).Select(count => count > 0);
        BulkDeleteCommand = CreateCommand(async () => await BulkDeleteCourseInstancesAsync(), hasSelection, "Ошибка массового удаления курсов");
        BulkArchiveCommand = CreateCommand(async () => await BulkArchiveCourseInstancesAsync(), hasSelection, "Ошибка массового архивирования курсов");

        SelectAllCommand = CreateCommand(async () => await SelectAllAsync(), null, "Ошибка выбора всех курсов");
        DeselectAllCommand = CreateCommand(async () => await DeselectAllAsync(), null, "Ошибка снятия выделения");
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска - используем безопасный подход без вложенных команд
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(searchText => searchText?.Trim() ?? string.Empty)
            .DistinctUntilChanged()
            .Where(_ => !IsLoading) // Предотвращаем выполнение во время загрузки
            .Subscribe(async searchText =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(searchText) || CurrentPage > 1)
                    {
                        // Используем прямой вызов метода вместо команды
                        await SearchCoursesAsync(searchText);
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автопоиска");
                }
            })
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе курса - используем безопасный подход
        this.WhenAnyValue(x => x.SelectedCourseInstance)
            .Where(courseInstance => courseInstance != null && !IsLoading)
            .Select(courseInstance => courseInstance!)
            .Subscribe(async courseInstance =>
            {
                try
                {
                    // Используем прямой вызов метода вместо команды
                    await ViewStatisticsCommand.Execute(courseInstance);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка загрузки статистики курса");
                }
            })
            .DisposeWith(Disposables);

        // Применение фильтров при изменении - используем безопасный подход
        this.WhenAnyValue(x => x.CategoryFilter, x => x.StatusFilter, x => x.DifficultyFilter, x => x.SelectedTeacherFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(_ => !IsLoading) // Предотвращаем выполнение во время загрузки
            .Subscribe(async _ =>
            {
                try
                {
                    // Используем прямой вызов метода вместо команды
                    await ApplyFiltersAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка применения фильтров");
                }
            })
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedCourseInstance)
            .Subscribe(_ =>
            {
                try
                {
                    this.RaisePropertyChanged(nameof(HasSelectedCourseInstance));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления HasSelectedCourseInstance");
                }
            })
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.Statistics)
            .Subscribe(_ =>
            {
                try
                {
                    this.RaisePropertyChanged(nameof(HasSelectedCourseInstanceStatistics));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления HasSelectedCourseInstanceStatistics");
                }
            })
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
            .Subscribe(_ =>
            {
                try
                {
                    this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                    this.RaisePropertyChanged(nameof(CanGoToNextPage));
                    this.RaisePropertyChanged(nameof(CanGoToFirstPage));
                    this.RaisePropertyChanged(nameof(CanGoToLastPage));
                    this.RaisePropertyChanged(nameof(HasPages));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления пагинации");
                }
            })
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.PageSize, x => x.TotalCourses)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(PaginationInfo)))
            .DisposeWith(Disposables);
    }

    private async Task LoadCourseInstancesAsync()
    {
        // Предотвращаем множественные одновременные вызовы
        if (IsLoading) return;

        LogInfo($"Loading courses with filters: SearchText={SearchText ?? string.Empty}, CategoryFilter={CategoryFilter ?? string.Empty}, StatusFilter={StatusFilter ?? string.Empty}");

        IsLoading = true;
        ShowInfo("Загрузка курсов...");

        try
        {
            // Используем правильный метод из сервиса
            var (courses, totalCount) = await _courseInstanceService.GetCourseInstancesPagedAsync(
                CurrentPage,
                PageSize,
                SearchText);

            CourseInstances.Clear();
            foreach (var course in courses)
            {
                var itemViewModel = new CourseInstanceViewModel(course);

                // The paged query doesn't eager-load Enrollments, so pull the real count
                // per instance from the dedicated service method instead of trusting the
                // (likely empty) navigation collection.
                itemViewModel.EnrollmentsCount = await _courseInstanceService.GetEnrollmentCountAsync(course.Uid);

                itemViewModel.WhenAnyValue(x => x.IsSelected)
                    .Subscribe(_ => SelectedCoursesCount = CourseInstances.Count(c => c.IsSelected))
                    .DisposeWith(Disposables);
                CourseInstances.Add(itemViewModel);
            }
            SelectedCoursesCount = 0;

            TotalCourses = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ActiveCourses = courses.Count(c => c.Status == CourseStatus.Active);
            TotalEnrollments = CourseInstances.Sum(c => c.EnrollmentsCount);

            ShowSuccess($"Загружено {CourseInstances.Count} курсов");
            LogInfo($"Loaded {CourseInstances.Count} courses, total: {totalCount}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки курсов");
            ShowError("Не удалось загрузить курсы");
            CourseInstances.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTeachersAsync()
    {
        LogInfo("Loading teachers for filter");

        try
        {
            // Используем правильный метод из сервиса
            var teachers = await _teacherService.GetAllTeachersAsync();
            Teachers.Clear();
            foreach (var teacher in teachers)
            {
                Teachers.Add(new TeacherViewModel(teacher));
            }

            LogInfo($"Loaded {teachers.Count()} teachers for filter");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки преподавателей");
            ShowError("Не удалось загрузить список преподавателей");
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing courses data");
        IsRefreshing = true;

        await LoadCourseInstancesAsync();
        ShowSuccess("Данные обновлены");

        IsRefreshing = false;
    }

    private async Task CreateCourseInstanceAsync()
    {
        LogInfo("Creating new course");

        var newCourse = new CourseInstance
        {
            Name = string.Empty,
            Description = string.Empty,
            Status = CourseStatus.Draft,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3),
            MaxEnrollments = 30
        };

        var dialogResult = await _dialogService.ShowCourseInstanceEditDialogAsync(newCourse);
        if (dialogResult == null)
        {
            LogDebug("Course creation cancelled by user");
            return;
        }

        // Используем новый универсальный метод создания
        var createdCourse = await _courseInstanceService.CreateAsync(dialogResult);
        CourseInstances.Add(new CourseInstanceViewModel(createdCourse));

        ShowSuccess($"Курс '{createdCourse.Name}' создан");
        LogInfo($"Course created successfully: {createdCourse.Name}");

        // Уведомление преподавателю
        if (createdCourse.TeacherUid.HasValue)
        {
            var teacher = await _teacherService.GetTeacherAsync(createdCourse.TeacherUid.Value);
            if (teacher != null)
            {
                await _notificationService.CreateNotificationAsync(
                    createdCourse.TeacherUid.Value,
                    "Назначение на курс",
                    $"Вы назначены преподавателем курса '{createdCourse.Name}'",
                    NotificationType.Info);
            }
        }
    }

    private async Task EditCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo($"Editing course: {courseInstanceViewModel.Uid}");

        // Получаем актуальные данные курса
        var course = await _courseInstanceService.GetByUidAsync(courseInstanceViewModel.Uid);
        if (course == null)
        {
            ShowError("Курс не найден");
            return;
        }

        var dialogResult = await _dialogService.ShowCourseInstanceEditDialogAsync(course);
        if (dialogResult == null)
        {
            LogDebug("Course editing cancelled by user");
            return;
        }

        // Используем новый универсальный метод обновления
        var success = await _courseInstanceService.UpdateAsync(dialogResult);
        if (success)
        {
            var index = CourseInstances.IndexOf(courseInstanceViewModel);
            if (index >= 0)
            {
                CourseInstances[index] = new CourseInstanceViewModel(dialogResult);
            }

            ShowSuccess($"Курс '{dialogResult.Name}' обновлен");
            LogInfo($"Course updated successfully: {dialogResult.Name}");
        }
        else
        {
            ShowError("Не удалось обновить курс");
        }

        LogInfo($"Course instance edited: {course?.Name ?? "Unknown"} ({course?.Uid})");
    }

    private async Task DeleteCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo($"Deleting course: {courseInstanceViewModel.Uid}");

        var confirmResult = await _dialogService.ShowConfirmationDialogAsync(
            "Удаление курса",
            $"Вы уверены, что хотите удалить курс '{courseInstanceViewModel.Name}'?");

        if (confirmResult == false)
        {
            LogDebug("Course deletion cancelled by user");
            return;
        }

        // Используем новый универсальный метод удаления
        var success = await _courseInstanceService.DeleteAsync(courseInstanceViewModel.Uid);
        if (success)
        {
            CourseInstances.Remove(courseInstanceViewModel);
            ShowSuccess($"Курс '{courseInstanceViewModel.Name}' удален");
            LogInfo($"Course deleted successfully: {courseInstanceViewModel.Name}");
        }
        else
        {
            ShowError("Не удалось удалить курс");
        }
    }

    private async Task ViewCourseInstanceDetailsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo($"Viewing course details: {courseInstanceViewModel.Uid}");

        SelectedCourseInstance = courseInstanceViewModel;
        await ViewStatisticsCommand.Execute(courseInstanceViewModel);

        ShowInfo($"Просмотр курса '{courseInstanceViewModel.Name}'");
    }

    private async Task ViewStatisticsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var statistics = await _courseService.GetCourseStatisticsAsync(courseInstanceViewModel.SubjectUid);
            // statistics is a tuple, not a nullable type, so we check if it has meaningful data
            if (statistics.TotalInstances > 0)
            {
                await _dialogService.ShowMessageAsync(
                    "Статистика курса",
                    $"Всего экземпляров: {statistics.TotalInstances}\n" +
                    $"Активных экземпляров: {statistics.ActiveInstances}\n" +
                    $"Всего студентов: {statistics.TotalEnrollments}\n" +
                    $"Завершенных записей: {statistics.CompletedEnrollments}\n" +
                    $"Процент завершения: {statistics.CompletionRate:F1}%\n" +
                    $"Средняя оценка: {statistics.AverageGrade:F2}");
                
                LogInfo($"Course statistics viewed for: {courseInstanceViewModel.Name}");
            }
            else
            {
                await _dialogService.ShowMessageAsync(
                    "Статистика курса",
                    "Статистика по курсу отсутствует или курс не имеет экземпляров.");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to load course statistics for: {courseInstanceViewModel.Name}");
            await _dialogService.ShowErrorAsync("Ошибка", 
                "Не удалось загрузить статистику курса. Попробуйте еще раз.");
        }
    }

    private async Task ManageEnrollmentsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var allStudents = await _studentService.GetStudentsAsync();
            var enrolledStudents = await _courseInstanceService.GetCoursesByStudentAsync(courseInstanceViewModel.Uid); // TODO: исправить логику

            var result = await _dialogService.ShowCourseEnrollmentDialogAsync(courseInstanceViewModel.ToCourseInstance(), allStudents);
            if (result != null)
            {
                await RefreshAsync();
                _statusService.ShowSuccess($"Записи на курс '{courseInstanceViewModel.Name}' обновлены", "Курсы");
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка управления записями: {ex.Message}", "Курсы");
        }
    }

    private async Task ManageContentAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            // Get the Subject from the CourseInstance to create a Course object
            var subject = await _subjectService.GetByUidAsync(courseInstanceViewModel.SubjectUid);
            if (subject == null)
            {
                _statusService.ShowError("Предмет не найден", "Курсы");
                return;
            }

            // Create a Course object from the Subject for the dialog
            var course = new Course
            {
                Uid = subject.Uid,
                Name = subject.Name,
                Code = subject.Code,
                Description = subject.Description,
                Credits = subject.Credits,
                DepartmentUid = subject.DepartmentUid
            };

            var result = await _dialogService.ShowCourseContentManagementDialogAsync(course);
            if (result != null)
            {
                await RefreshAsync();
                _statusService.ShowSuccess("Контент курса обновлен", "Курсы");
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка управления контентом: {ex.Message}", "Курсы");
        }
    }

    private async Task SearchCoursesAsync(string searchText)
    {
        SearchText = searchText;
        CurrentPage = 1;
        await LoadCourseInstancesAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        try
        {
            // Предотвращаем рекурсивные вызовы
            if (IsLoading) return;

            LogInfo($"Applying filters: Category={CategoryFilter}, Status={StatusFilter}, Difficulty={DifficultyFilter}, Teacher={SelectedTeacherFilter?.FullName ?? "null"}");

            CurrentPage = 1;
            await LoadCourseInstancesAsync();

            LogInfo($"Filters applied successfully. Loaded {CourseCount} courses");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка применения фильтров");
            ShowError("Ошибка применения фильтров");
        }
    }

    private async Task ClearFiltersAsync()
    {
        CategoryFilter = null;
        StatusFilter = null;
        DifficultyFilter = null;
        SelectedTeacherFilter = null;
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadCourseInstancesAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadCourseInstancesAsync();
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

    private async Task CloneCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var confirmation = await _dialogService.ShowConfirmationDialogAsync(
                "Клонирование курса",
                $"Создать копию курса '{courseInstanceViewModel.Name}'?\n\nБудет создан новый курс со всем содержимым, но без записанных студентов.");

            if (confirmation)
            {
                var newCourseName = await _dialogService.ShowTextInputDialogAsync(
                    "Название нового курса",
                    "Введите название для копии курса:",
                    $"{courseInstanceViewModel.Name} (копия)");

                if (!string.IsNullOrEmpty(newCourseName))
                {
                    // Для клонирования нужен новый академический период
                    // В реальном приложении здесь будет выбор периода через UI
                    var newAcademicPeriodUid = Guid.NewGuid(); // Заглушка для компиляции
                    var clonedCourse = await _courseInstanceService.CloneCourseAsync(courseInstanceViewModel.Uid, newAcademicPeriodUid);
                    if (clonedCourse != null)
                    {
                        await RefreshAsync();
                        _statusService.ShowSuccess($"Курс '{newCourseName}' создан как копия", "Курсы");

                        await _notificationService.SendNotificationAsync(
                            "Курс клонирован",
                            $"Создана копия курса: {newCourseName}",
                            NotificationType.Info);
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось клонировать курс", "Курсы");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка клонирования курса: {ex.Message}", "Курсы");
        }
    }

    private async Task ImportCoursesAsync()
    {
        try
        {
            var filePath = await _dialogService.ShowFileOpenDialogAsync(
                "Импорт курсов",
                new[] { "*.xlsx", "*.csv", "*.json" });

            if (!string.IsNullOrEmpty(filePath))
            {
                // Заглушка - в реальной реализации здесь будет импорт курсов
                ShowInfo($"Импорт курсов из файла: {filePath}");
                LogInfo($"Import courses requested from file: {FilePath}");

                //await _notificationService.SendNotificationAsync(
                //    "Импорт курсов",
                //    "Функция импорта курсов будет реализована позже",
                //    NotificationType.Info);
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка импорта курсов: {ex.Message}", "Импорт");
        }
    }

    private async Task ExportReportAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllCoursesAsync();

            // Заглушка - в реальной реализации здесь будет экспорт в Excel
            _statusService.ShowInfo($"Экспорт отчета: {courses.Count()} курсов готовы к экспорту", "Экспорт");
            LogInfo($"Export report requested for {CourseCount} courses");
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка экспорта отчета: {ex.Message}", "Экспорт");
        }
    }

    /// <summary>
    /// Отмечает все загруженные экземпляры курсов как выбранные
    /// </summary>
    private Task SelectAllAsync()
    {
        foreach (var course in CourseInstances)
        {
            course.IsSelected = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Снимает выделение со всех загруженных экземпляров курсов
    /// </summary>
    private Task DeselectAllAsync()
    {
        foreach (var course in CourseInstances)
        {
            course.IsSelected = false;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Массовое удаление выбранных экземпляров курсов
    /// </summary>
    private async Task BulkDeleteCourseInstancesAsync()
    {
        var selected = CourseInstances.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить выбранные курсы ({selected.Count})?");
        if (!confirmed) return;

        var deletedCount = 0;
        foreach (var item in selected)
        {
            var success = await _courseInstanceService.DeleteAsync(item.Uid);
            if (success)
            {
                CourseInstances.Remove(item);
                deletedCount++;
            }
        }

        SelectedCoursesCount = CourseInstances.Count(c => c.IsSelected);
        TotalCourses = Math.Max(0, TotalCourses - deletedCount);
        TotalPages = (int)Math.Ceiling((double)TotalCourses / PageSize);

        _statusService.ShowSuccess($"Удалено курсов: {deletedCount}", "Курсы");
    }

    /// <summary>
    /// Массовое архивирование выбранных экземпляров курсов
    /// </summary>
    private async Task BulkArchiveCourseInstancesAsync()
    {
        var selected = CourseInstances.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0) return;

        var archivedCount = 0;
        foreach (var item in selected)
        {
            var course = await _courseInstanceService.GetByUidAsync(item.Uid);
            if (course == null) continue;

            course.Status = CourseStatus.Archived;

            var success = await _courseInstanceService.UpdateAsync(course);
            if (success)
            {
                item.Status = CourseStatus.Archived;
                archivedCount++;
            }
        }

        ActiveCourses = CourseInstances.Count(c => c.Status == CourseStatus.Active);
        _statusService.ShowSuccess($"Архивировано курсов: {archivedCount}", "Курсы");
    }

    private void InitializeFilters()
    {
        LogInfo("Initializing course filters");

        // Инициализируем категории
        Categories.Clear();
        Categories.Add("Все категории");
        Categories.Add("Математика");
        Categories.Add("Естественные науки");
        Categories.Add("Гуманитарные науки");
        Categories.Add("Информатика");
        Categories.Add("Языки");
        Categories.Add("Искусство");
        Categories.Add("Спорт");
        Categories.Add("Экономика");
        Categories.Add("Инженерия");

        // Инициализируем уровни сложности
        Difficulties.Clear();
        Difficulties.Add("Все");
        Difficulties.Add("Начальный");
        Difficulties.Add("Средний");
        Difficulties.Add("Продвинутый");
        Difficulties.Add("Экспертный");

        // Инициализируем статусы
        Statuses.Clear();
        Statuses.Add("Все");
        Statuses.Add("Активные");
        Statuses.Add("Опубликованные");
        Statuses.Add("Черновики");
        Statuses.Add("Архивированные");
        Statuses.Add("Приостановленные");

        LogInfo("Course filters initialized");
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        LogInfo("CoursesViewModel loaded for the first time");

        // Initialize filters first
        InitializeFilters();

        // Load teachers and courses when view is loaded for the first time
        await ExecuteWithErrorHandlingAsync(LoadTeachersAsync, "Ошибка загрузки списка преподавателей");
        await LoadCourseInstancesAsync();
    }

    #endregion
}

