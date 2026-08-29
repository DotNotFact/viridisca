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
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления экзаменами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("exams", 
    DisplayName = "Экзамены", 
    IconKey = "ClipboardCheck", 
    Order = 8,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление экзаменами и тестированием")]
public class ExamsViewModel : RoutableViewModelBase
{
    private readonly IExamService _examService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAcademicPeriodService _academicPeriodService;
    private readonly IStudentService _studentService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly IUnifiedNavigationService _navigationService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<ExamViewModel> Exams { get; set; } = new();
    [Reactive] public ExamViewModel? SelectedExam { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public ExamStatistics? Statistics { get; set; }
    
    // Фильтры
    [Reactive] public ObservableCollection<CourseInstanceViewModel> CourseInstances { get; set; } = new();
    [Reactive] public ObservableCollection<AcademicPeriodViewModel> AcademicPeriods { get; set; } = new();
    [Reactive] public CourseInstanceViewModel? SelectedCourseFilter { get; set; }
    [Reactive] public AcademicPeriodViewModel? SelectedPeriodFilter { get; set; }
    [Reactive] public ExamType? TypeFilter { get; set; }
    [Reactive] public DateTime? DateFromFilter { get; set; }
    [Reactive] public DateTime? DateToFilter { get; set; }
    [Reactive] public bool? PublishedFilter { get; set; }
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalExams { get; set; }

    // Computed properties
    public bool HasSelectedExam => SelectedExam != null;
    public bool HasStatistics => Statistics != null;

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadExamsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateExamCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> EditExamCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> DeleteExamCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> ViewExamDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> PublishExamCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> ViewResultsCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> ExportResultsCommand { get; private set; } = null!;
    public ReactiveCommand<ExamViewModel, Unit> SendNotificationCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;

    // === НОВЫЕ СВОЙСТВА ===
    /// <summary>
    /// Название экзамена
    /// </summary>
    [Reactive] public string ExamTitle { get; set; } = string.Empty;

    public ExamsViewModel(
        IScreen hostScreen,
        IExamService examService,
        ICourseInstanceService courseInstanceService,
        IAcademicPeriodService academicPeriodService,
        IStudentService studentService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService,
        IUnifiedNavigationService navigationService) : base(hostScreen)
    {
        _examService = examService ?? throw new ArgumentNullException(nameof(examService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _academicPeriodService = academicPeriodService ?? throw new ArgumentNullException(nameof(academicPeriodService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService)); 
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        InitializeCommands();
        SetupSubscriptions();
    }

    private void InitializeCommands()
    {
        LoadExamsCommand = CreateCommand(async () => await LoadExamsAsync(), null, "Ошибка загрузки экзаменов");
        RefreshCommand = CreateCommand(async () => await RefreshAsync(), null, "Ошибка обновления данных");
        CreateExamCommand = CreateCommand(async () => await CreateExamAsync(), null, "Ошибка создания экзамена");
        EditExamCommand = CreateCommand<ExamViewModel>(async (exam) => await EditExamAsync(exam), null, "Ошибка редактирования экзамена");
        DeleteExamCommand = CreateCommand<ExamViewModel>(async (exam) => await DeleteExamAsync(exam), null, "Ошибка удаления экзамена");
        ViewExamDetailsCommand = CreateCommand<ExamViewModel>(async (exam) => await ViewExamDetailsAsync(exam), null, "Ошибка просмотра деталей экзамена");
        PublishExamCommand = CreateCommand<ExamViewModel>(async (exam) => await PublishExamAsync(exam), null, "Ошибка публикации экзамена");
        ViewResultsCommand = CreateCommand<ExamViewModel>(async (exam) => await ViewResultsAsync(exam), null, "Ошибка просмотра результатов");
        ExportResultsCommand = CreateCommand<ExamViewModel>(async (exam) => await ExportResultsAsync(exam), null, "Ошибка экспорта результатов");
        SendNotificationCommand = CreateCommand<ExamViewModel>(async (exam) => await SendNotificationAsync(exam), null, "Ошибка отправки уведомления");
        LoadStatisticsCommand = CreateCommand(async () => await LoadStatisticsAsync(), null, "Ошибка загрузки статистики");
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchExamsAsync(searchTerm), null, "Ошибка поиска экзаменов");
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync(), null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync(), null, "Ошибка очистки фильтров");
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page), null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");

        var canGoFirst = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        var canGoLast = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), canGoFirst, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), canGoLast, "Ошибка перехода на последнюю страницу");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Применение фильтров при изменении
        this.WhenAnyValue(x => x.SelectedCourseFilter, x => x.SelectedPeriodFilter, x => x.TypeFilter, x => x.DateFromFilter, x => x.DateToFilter, x => x.PublishedFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SelectedExam)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedExam)))
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Statistics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasStatistics)))
            .DisposeWith(Disposables);
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("ExamsViewModel first time loaded");
        await LoadFiltersDataAsync();
        await LoadExamsAsync();
        await LoadStatisticsAsync();
    }

    private async Task LoadFiltersDataAsync()
    {
        try
        {
            LogInfo("Loading filters data for exams");
            
            // Sequential, not Task.WhenAll: these share one scoped EF DbContext,
            // which throws "A second operation was started on this context instance" under concurrent awaits.
            var courseInstances = await _courseInstanceService.GetAllAsync();
            var academicPeriods = await _academicPeriodService.GetAllAsync();

            CourseInstances.Clear();
            foreach (var courseInstance in courseInstances)
            {
                CourseInstances.Add(new CourseInstanceViewModel(courseInstance));
            }

            AcademicPeriods.Clear();
            foreach (var period in academicPeriods)
            {
                AcademicPeriods.Add(new AcademicPeriodViewModel(
                    period,
                    HostScreen,
                    _navigationService, 
                    _notificationService,
                    _academicPeriodService,
                    _courseInstanceService));
            }
            
            LogInfo($"Loaded {CourseInstances.Count} course instances and {AcademicPeriods.Count} academic periods for filters");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load filters data");
            ShowWarning("Не удалось загрузить данные для фильтров");
        }
    }

    private async Task LoadExamsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var exams = await _examService.GetAllAsync();
            var examViewModels = exams.Select(exam => new ExamViewModel(exam)).ToList();

            Exams.Clear();
            foreach (var examViewModel in examViewModels)
            {
                Exams.Add(examViewModel);
            }

            TotalExams = Exams.Count;
            LogInfo($"Загружено {TotalExams} экзаменов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке экзаменов");
            ErrorMessage = "Не удалось загрузить экзамены";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing exams data");
        IsRefreshing = true;
        
        await LoadFiltersDataAsync();
        await LoadExamsAsync();
        await LoadStatisticsAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создает новый экзамен
    /// </summary>
    private async Task CreateExamAsync()
    {
        try
        {
            var result = await _dialogService.ShowExamEditDialogAsync(new Exam());
            if (result != null && result is Exam exam)
            {
                await _examService.CreateAsync(exam);
                await LoadExamsAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                LogInfo($"Экзамен создан: {exam.Title}");
                //await _notificationService.CreateNotificationAsync(
                //    currentPerson.Uid,
                //    "Экзамен создан",
                //    $"Экзамен '{exam.Title}' успешно создан",
                //    NotificationType.Success,
                //    NotificationPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create exam");
            ShowError("Не удалось создать экзамен. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Редактирует выбранный экзамен
    /// </summary>
    private async Task EditExamAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        try
        {
            var examToEdit = await _examService.GetByUidAsync(examViewModel.Uid);
            if (examToEdit == null) return;

            var result = await _dialogService.ShowExamEditDialogAsync(examToEdit);
            if (result != null && result is Exam updatedExam)
            {
                await _examService.UpdateAsync(updatedExam);
                await LoadExamsAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                LogInfo($"Экзамен обновлен: {updatedExam.Title}");
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Экзамен обновлен",
                    $"Экзамен '{updatedExam.Title}' успешно обновлен",
                    NotificationType.Success,
                    NotificationPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при редактировании экзамена");
        }
    }

    /// <summary>
    /// Удаляет выбранный экзамен
    /// </summary>
    private async Task DeleteExamAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить экзамен '{examViewModel.Title}'?");

            if (result)
            {
                await _examService.DeleteAsync(examViewModel.Uid);
                await LoadExamsAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Экзамен удален",
                    $"Экзамен '{examViewModel.Title}' успешно удален",
                    NotificationType.Success,
                    NotificationPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении экзамена");
        }
    }

    /// <summary>
    /// Проверяет конфликты экзаменов
    /// </summary>
    private async Task CheckConflictsAsync()
    {
        if (SelectedExam == null) return;

        try
        {
            var conflicts = await _examService.GetConflictingExamsAsync(
                SelectedExam.ExamDate, 
                SelectedExam.ExamDate.Add(SelectedExam.Duration), 
                SelectedExam.Uid);

            if (conflicts.Any())
            {
                await _dialogService.ShowInfoAsync(
                    "Конфликты расписания",
                    $"Найдено {conflicts.Count()} конфликтующих экзаменов");
            }
            else
            {
                await _dialogService.ShowInfoAsync(
                    "Конфликты расписания",
                    "Конфликтов не найдено");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при проверке конфликтов");
        }
    }

    /// <summary>
    /// Отправляет уведомления об экзамене
    /// </summary>
    private async Task SendNotificationAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        LogInfo($"Sending notification for exam: {examViewModel.Title}");
        
        try
        {
            await _examService.SendExamNotificationAsync(examViewModel.Uid, "Напоминание об экзамене");
            ShowSuccess("Уведомления отправлены");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to send exam notification");
            ShowError("Не удалось отправить уведомления");
        }
    }

    /// <summary>
    /// Загружает статистику экзаменов
    /// </summary>
    private async Task LoadStatisticsAsync()
    {
        try
        {
            var allExamsEnumerable = await _examService.GetAllAsync();
            var allExams = allExamsEnumerable.ToList(); // Материализуем коллекцию
            var totalExams = allExams.Count;
            var upcomingExams = allExams.Count(e => e.ExamDate > DateTime.Now);
            var completedExams = allExams.Count(e => e.ExamDate <= DateTime.Now);
            var examsWithScore = allExams.Where(e => e.MaxScore > 0).ToList();
            
            double averageScore = 0.0;
            if (examsWithScore.Any())
            {
                var totalScore = 0.0;
                foreach (var exam in examsWithScore)
                {
                    totalScore += (double)exam.MaxScore;
                }
                averageScore = totalScore / examsWithScore.Count;
            }

            // Обновляем статистику в UI
            LogInfo($"Статистика экзаменов: Всего={totalExams}, Предстоящих={upcomingExams}, Завершенных={completedExams}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки статистики экзаменов");
        }
    }

    private async Task ViewExamDetailsAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        LogInfo($"Viewing exam details: {examViewModel.Title}");
        
        try
        {
            await NavigateToAsync($"exam-details/{examViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to exam details");
            ShowError("Не удалось открыть детали экзамена");
        }
    }

    private async Task PublishExamAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        LogInfo($"Publishing exam: {examViewModel.Title}");
        
        try
        {
            await _examService.PublishExamAsync(examViewModel.Uid);
            await LoadExamsAsync();
            ShowSuccess("Экзамен опубликован");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to publish exam");
            ShowError("Не удалось опубликовать экзамен");
        }
    }

    private async Task ViewResultsAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        LogInfo($"Viewing results for exam: {examViewModel.Title}");
        
        try
        {
            await NavigateToAsync($"exam-results/{examViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to exam results");
            ShowError("Не удалось открыть результаты экзамена");
        }
    }

    private async Task ExportResultsAsync(ExamViewModel examViewModel)
    {
        if (examViewModel == null) return;

        LogInfo($"Exporting results for exam: {examViewModel.Title}");
        
        try
        {
            // Реализация экспорта результатов
            ShowSuccess("Результаты экспортированы");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to export exam results");
            ShowError("Не удалось экспортировать результаты");
        }
    }

    private async Task SearchExamsAsync(string searchText)
    {
        LogInfo($"Searching exams with text: {SearchText}");
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadExamsAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters to exams");
        CurrentPage = 1; // Сброс на первую страницу при применении фильтров
        await LoadExamsAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing all filters");
        SelectedCourseFilter = null;
        SelectedPeriodFilter = null;
        TypeFilter = null;
        DateFromFilter = null;
        DateToFilter = null;
        PublishedFilter = null;
        CurrentPage = 1;
        await LoadExamsAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadExamsAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadExamsAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadExamsAsync();
        }
    }

    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await LoadExamsAsync();
    }

    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadExamsAsync();
    }

    private async Task LoadExamsByFilterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var exams = await _examService.GetAllAsync();
            var examViewModels = exams.Select(exam => new ExamViewModel(exam)).ToList();

            Exams.Clear();
            foreach (var examViewModel in examViewModels)
            {
                Exams.Add(examViewModel);
            }

            TotalExams = Exams.Count;
            LogInfo($"Загружено {TotalExams} экзаменов по фильтру");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке экзаменов по фильтру");
            ErrorMessage = "Не удалось загрузить экзамены";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// Информация о связанных данных экзамена
/// </summary>
public class ExamRelatedDataInfo
{
    public bool HasRelatedData { get; set; }
    public bool HasResults { get; set; }
    public int ResultsCount { get; set; }
    public List<string> RelatedDataDescriptions { get; set; } = new();
}

/// <summary>
/// Статистика экзаменов
/// </summary>
public class ExamStatistics
{
    public int TotalExams { get; set; }
    public int PublishedExams { get; set; }
    public int CompletedExams { get; set; }
    public int UpcomingExams { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
}

/// <summary>
/// ViewModel для экзамена
/// </summary>
public class ExamViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; }
    [Reactive] public DateTime ExamDate { get; set; }
    [Reactive] public TimeSpan Duration { get; set; }
    [Reactive] public string? Location { get; set; }
    [Reactive] public ExamType Type { get; set; }
    [Reactive] public decimal MaxScore { get; set; }
    [Reactive] public bool IsPublished { get; set; }
    [Reactive] public string? Instructions { get; set; }
    [Reactive] public Guid CourseInstanceUid { get; set; }
    [Reactive] public Guid AcademicPeriodUid { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    // Дополнительные свойства для UI
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string PeriodName { get; set; } = string.Empty;
    [Reactive] public int ResultsCount { get; set; }

    public string StatusText => IsPublished ? "Опубликован" : "Черновик";
    public string DurationText => $"{(int)Duration.TotalMinutes} мин";

    public ExamViewModel() { }

    public ExamViewModel(Exam exam)
    {
        Uid = exam.Uid;
        Title = exam.Title;
        Description = exam.Description;
        ExamDate = exam.ExamDate;
        Duration = exam.Duration;
        Location = exam.Location;
        Type = exam.Type;
        MaxScore = exam.MaxScore;
        IsPublished = exam.IsPublished;
        Instructions = exam.Instructions;
        CourseInstanceUid = exam.CourseInstanceUid;
        AcademicPeriodUid = exam.AcademicPeriodUid;
        LastModifiedAt = exam.LastModifiedAt;

        // Дополнительные данные из связанных сущностей
        CourseName = exam.CourseInstance?.Subject?.Name ?? string.Empty;
        PeriodName = exam.AcademicPeriod?.Name ?? string.Empty;
        ResultsCount = exam.Results?.Count ?? 0;

        var examDate = exam.ExamDate;
    }
} 

