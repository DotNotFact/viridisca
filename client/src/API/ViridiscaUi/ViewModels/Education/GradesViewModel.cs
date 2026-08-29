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
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления оценками
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("grades", 
    DisplayName = "Оценки", 
    IconKey = "StarBox", 
    Order = 7,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление оценками и успеваемостью")]
public class GradesViewModel : RoutableViewModelBase
{
    private readonly IGradeService _gradeService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IGroupService _groupService;
    private readonly IStudentService _studentService;
    private readonly IAssignmentService _assignmentService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IDialogService _dialogService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<GradeViewModel> Grades { get; set; } = new();
    [Reactive] public GradeViewModel? SelectedGrade { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    
    // Фильтры
    [Reactive] public ObservableCollection<CourseInstance> Courses { get; set; } = new();
    [Reactive] public ObservableCollection<Group> Groups { get; set; } = new();
    [Reactive] public CourseInstance? SelectedCourse { get; set; }
    [Reactive] public Group? SelectedGroupFilter { get; set; }
    [Reactive] public string? GradeRangeFilter { get; set; }
    [Reactive] public string? PeriodFilter { get; set; }
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 25;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalGrades { get; set; }

    // Статистика
    [Reactive] public double AverageGrade { get; set; }
    [Reactive] public int ExcellentCount { get; set; }
    [Reactive] public int GoodCount { get; set; }
    [Reactive] public int SatisfactoryCount { get; set; }
    [Reactive] public int UnsatisfactoryCount { get; set; }
    [Reactive] public double PassRate { get; set; }
    [Reactive] public double QualityRate { get; set; }
    // Count of grades that count as passing (5/4/3 on the codebase's existing >=3 convention,
    // same threshold already used above for PassRate)
    [Reactive] public int PassingGrades { get; set; }

    // Bulk selection
    [Reactive] public int SelectedGradesCount { get; set; }

    // Computed properties
    public bool HasSelectedGrade => SelectedGrade != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public bool CanGoToFirstPage => CurrentPage > 1;
    public bool CanGoToLastPage => CurrentPage < TotalPages;
    public bool HasPages => TotalPages > 1;
    public string PaginationInfo
    {
        get
        {
            if (TotalGrades == 0) return "Нет данных";
            var from = (CurrentPage - 1) * PageSize + 1;
            var to = Math.Min(CurrentPage * PageSize, TotalGrades);
            return $"Показано {from}-{to} из {TotalGrades}";
        }
    }

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadGradesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateGradeCommand { get; private set; } = null!;
    public ReactiveCommand<GradeViewModel, Unit> EditGradeCommand { get; private set; } = null!;
    public ReactiveCommand<GradeViewModel, Unit> DeleteGradeCommand { get; private set; } = null!;
    public ReactiveCommand<GradeViewModel, Unit> ViewGradeDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<GradeViewModel, Unit> AddCommentCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkGradingCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GenerateAnalyticsReportCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NotifyParentsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkEditGradesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkApproveCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkRejectCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    [Reactive] public ObservableCollection<StudentViewModel> Students { get; set; } = new();
    [Reactive] public StudentViewModel? SelectedStudent { get; set; }
    [Reactive] public Guid? SelectedStudentUid { get; set; }
    [Reactive] public ObservableCollection<CourseInstanceViewModel> CourseInstances { get; set; } = new();
    [Reactive] public CourseInstanceViewModel? SelectedCourseInstance { get; set; }
    [Reactive] public Guid? SelectedCourseInstanceUid { get; set; }

    // Filter and search properties
    [Reactive] public string SearchTerm { get; set; } = string.Empty;
    [Reactive] public Guid? StudentUid { get; set; }
    [Reactive] public Guid? GradeUid { get; set; }

    /// <summary>
    /// Конструктор
    /// </summary>
    public GradesViewModel(
        IScreen hostScreen,
        IGradeService gradeService,
        IStudentService studentService,
        ICourseInstanceService courseInstanceService,
        IGroupService groupService,
        IAssignmentService assignmentService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IDialogService dialogService) : base(hostScreen)
    {
        _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService)); 
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        InitializeCommands();
        SetupSubscriptions();
    }

    private void InitializeCommands()
    {
        LoadGradesCommand = CreateCommand(async () => await LoadGradesAsync());
        RefreshCommand = CreateCommand(async () => await RefreshAsync());
        CreateGradeCommand = CreateCommand(async () => await CreateGradeAsync());
        EditGradeCommand = CreateCommand<GradeViewModel>(async (grade) => await EditGradeAsync(grade));
        DeleteGradeCommand = CreateCommand<GradeViewModel>(async (grade) => await DeleteGradeAsync(grade));
        ViewGradeDetailsCommand = CreateCommand<GradeViewModel>(async (grade) => await ViewGradeDetailsAsync(grade));
        AddCommentCommand = CreateCommand<GradeViewModel>(async (grade) => await AddCommentAsync(grade));
        BulkGradingCommand = CreateCommand(async () => await BulkGradingAsync());
        ExportReportCommand = CreateCommand(async () => await ExportReportAsync());
        ExportToExcelCommand = CreateCommand(async () => await ExportToExcelAsync());
        GenerateAnalyticsReportCommand = CreateCommand(async () => await GenerateAnalyticsReportAsync());
        NotifyParentsCommand = CreateCommand(async () => await NotifyParentsAsync());
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchGradesAsync(searchTerm));
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync());
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync());
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page));
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), canGoPrevious, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), canGoNext, "Ошибка перехода на последнюю страницу");

        var hasSelection = this.WhenAnyValue(x => x.SelectedGradesCount).Select(count => count > 0);
        BulkEditGradesCommand = CreateCommand(async () => await BulkEditGradesAsync(), hasSelection, "Ошибка массового редактирования оценок");
        BulkApproveCommand = CreateCommand(async () => await BulkApproveGradesAsync(), hasSelection, "Ошибка утверждения оценок");
        BulkRejectCommand = CreateCommand(async () => await BulkRejectGradesAsync(), hasSelection, "Ошибка отклонения оценок");
        SelectAllCommand = CreateCommand(async () => await SelectAllGradesAsync(), null, "Ошибка выбора всех оценок");
        DeselectAllCommand = CreateCommand(async () => await DeselectAllGradesAsync(), null, "Ошибка снятия выделения");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска - исправляем вложенную подписку
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(searchText => searchText?.Trim() ?? string.Empty)
            .DistinctUntilChanged()
            .Catch<string, Exception>(ex =>
            {
                LogError(ex, "Ошибка поиска оценок");
                return Observable.Empty<string>();
            })
            .InvokeCommand(SearchCommand)
            .DisposeWith(Disposables);

        // Обновление computed properties - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedGrade)
            .Catch<GradeViewModel?, Exception>(ex =>
            {
                LogError(ex, "Ошибка обновления HasSelectedGrade");
                return Observable.Return<GradeViewModel?>(null);
            })
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGrade)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, x => x.TotalGrades, x => x.PageSize)
            .Catch<(int, int, int, int), Exception>(ex =>
            {
                LogError(ex, "Ошибка обновления пагинации");
                return Observable.Return((1, 1, 0, PageSize));
            })
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                this.RaisePropertyChanged(nameof(CanGoToNextPage));
                this.RaisePropertyChanged(nameof(CanGoToFirstPage));
                this.RaisePropertyChanged(nameof(CanGoToLastPage));
                this.RaisePropertyChanged(nameof(HasPages));
                this.RaisePropertyChanged(nameof(PaginationInfo));
            })
            .DisposeWith(Disposables);

        // Автоматическое применение фильтров - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedCourse, x => x.SelectedGroupFilter, x => x.GradeRangeFilter, x => x.PeriodFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_ => Unit.Default)
            .Catch<Unit, Exception>(ex =>
            {
                LogError(ex, "Ошибка применения фильтров");
                return Observable.Empty<Unit>();
            })
            .InvokeCommand(ApplyFiltersCommand)
            .DisposeWith(Disposables);
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("GradesViewModel first time loaded");
        
        try
        {
            await LoadFiltersDataAsync();
            await LoadGradesAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load grades on first time");
            ShowError("Не удалось загрузить данные оценок");
        }
    }

    private async Task LoadFiltersDataAsync()
    {
        try
        {
            LogInfo("Loading filters data");
            
            // Sequential, not Task.WhenAll: these share one scoped EF DbContext,
            // which throws "A second operation was started on this context instance" under concurrent awaits.
            var courses = await _courseInstanceService.GetAllAsync();
            var groups = await _groupService.GetAllAsync();

            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(course);
            }

            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group);
            }
            
            LogInfo($"Loaded {Courses.Count} courses and {Groups.Count} groups for filters");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load filters data");
            ShowWarning("Не удалось загрузить данные для фильтров");
        }
    }

    private async Task LoadGradesAsync()
    {
        if (IsLoading) return;
        
        LogInfo("Loading grades with filters");
        IsLoading = true;
        ShowInfo("Загрузка оценок...");

        try
        {
            var gradeRangeFilter = ParseGradeRangeFilter();
            var periodFilter = ParsePeriodFilter();
            
            var (grades, totalCount) = await _gradeService.GetPagedAsync(
                CurrentPage, 
                PageSize, 
                SearchText);
            
            Grades.Clear();
            foreach (var grade in grades)
            {
                var gradeViewModel = new GradeViewModel(grade);
                gradeViewModel.WhenAnyValue(x => x.IsSelected)
                    .Subscribe(_ => SelectedGradesCount = Grades.Count(g => g.IsSelected))
                    .DisposeWith(Disposables);
                Grades.Add(gradeViewModel);
            }
            SelectedGradesCount = 0;

            TotalGrades = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            await LoadStatisticsAsync();

            ShowSuccess($"Загружено {grades.Count()} оценок");
            LogInfo($"Loaded {grades.Count()} grades");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при загрузке оценок");
            ShowError("Ошибка при загрузке оценок");
            Grades.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Загружает статистику оценок
    /// </summary>
    private async Task LoadStatisticsAsync()
    {
        try
        {
            var statistics = await _gradeService.GetGradeStatisticsAsync(GradeUid);
            // statistics is a tuple, not a nullable type, so we check if it has meaningful data
            if (statistics.TotalGrades > 0)
            {
                await _dialogService.ShowMessageAsync(
                    "Статистика оценок",
                    $"Средняя оценка: {statistics.AverageGrade:F2}\n" +
                    $"Максимальная оценка: {statistics.MaxGrade:F2}\n" +
                    $"Минимальная оценка: {statistics.MinGrade:F2}\n" +
                    $"Всего оценок: {statistics.TotalGrades}");
                    
                AverageGrade = (double)statistics.AverageGrade;
                ExcellentCount = statistics.GradeDistribution.GetValueOrDefault("5", 0);
                GoodCount = statistics.GradeDistribution.GetValueOrDefault("4", 0);
                SatisfactoryCount = statistics.GradeDistribution.GetValueOrDefault("3", 0);
                UnsatisfactoryCount = statistics.GradeDistribution.GetValueOrDefault("2", 0);
                PassRate = (double)statistics.GradeDistribution.Where(g => int.Parse(g.Key) >= 3).Sum(g => g.Value) / statistics.TotalGrades * 100;
                QualityRate = (double)statistics.GradeDistribution.Where(g => int.Parse(g.Key) >= 4).Sum(g => g.Value) / statistics.TotalGrades * 100;
                // Same >=3 "passing" convention already used for PassRate above
                PassingGrades = statistics.GradeDistribution.Where(g => int.Parse(g.Key) >= 3).Sum(g => g.Value);

                LogDebug($"Grade statistics loaded successfully");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load grade statistics");
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing grades data");
        IsRefreshing = true;
        
        await LoadFiltersDataAsync();
        await LoadGradesAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создание новой оценки с расширенной валидацией
    /// </summary>
    private async Task CreateGradeAsync()
    {
        LogInfo("Creating new grade");
        
        // Проверка прав доступа
        if (!await HasPermissionAsync("Grades.Create"))
        {
            ShowError("У вас нет прав для создания оценок");
            return;
        }

        try
        {
            var newGrade = new Grade
            {
                Uid = Guid.NewGuid(),
                Value = 0m,
                Comment = string.Empty,
                Type = GradeType.Assignment,
                IsPublished = false,
                GradedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Получаем списки студентов и заданий для диалога
            var students = await _studentService.GetAllAsync();
            var assignments = await _assignmentService.GetAllAsync();

            var dialogResult = await _dialogService.ShowGradeEditDialogAsync(newGrade, students, assignments);
            if (dialogResult == null)
            {
                LogDebug("Grade creation cancelled by user");
                return;
            }

            // Валидация данных оценки
            var validationResult = await ValidateGradeAsync(dialogResult);
            if (!validationResult.IsValid)
            {
                await _dialogService.ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
                return;
            }

            // Создание оценки
            var createdGrade = await _gradeService.CreateAsync(dialogResult);
            
            // Обновление UI
            Grades.Add(new GradeViewModel(createdGrade));
            TotalGrades++;
            
            // Автоматическое обновление статистики
            await LoadStatisticsAsync();
            
            LogInfo($"Grade created successfully for student {StudentUid}");
            ShowSuccess("Оценка успешно создана");
            
            // Уведомление студенту и родителям
            if (createdGrade.IsPublished)
            {
                await _notificationService.CreateNotificationAsync(
                    createdGrade.StudentUid,
                    "Оценка создана",
                    $"Вам выставлена новая оценка: {createdGrade.Value}",
                    NotificationType.Success);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже существует"))
        {
            LogError(ex, "Дублирование при создании оценки");
            ErrorMessage = $"Оценка для данного студента уже существует: {ex.Message}";
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create grade");
            ShowError("Не удалось создать оценку. Попробуйте еще раз.");
        }
    }

    /// <summary>
    /// Редактирует выбранную оценку
    /// </summary>
    private async Task EditGradeAsync(GradeViewModel gradeViewModel)
    {
        if (gradeViewModel == null) return;

        try
        {
            var grade = new Grade
            {
                Uid = gradeViewModel.Uid,
                Value = (decimal)gradeViewModel.Value,
                Type = (GradeType)gradeViewModel.Type,
                Comment = gradeViewModel.Comment,
                IsPublished = gradeViewModel.IsPublished,
                StudentUid = gradeViewModel.StudentUid,
                AssignmentUid = gradeViewModel.AssignmentUid
            };

            // Получаем списки студентов и заданий для диалога
            var students = await _studentService.GetAllAsync();
            var assignments = await _assignmentService.GetAllAsync();

            var result = await _dialogService.ShowGradeEditDialogAsync(grade, students, assignments);
            if (result != null && result is Grade updatedGrade)
            {
                await _gradeService.UpdateAsync(updatedGrade);
                await LoadGradesAsync();
                ShowSuccess($"Оценка успешно обновлена");
                await _notificationService.CreateNotificationAsync(
                    updatedGrade.StudentUid,
                    "Оценка обновлена",
                    $"Ваша оценка изменена: {updatedGrade.Value}",
                    NotificationType.Success);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Конфликт одновременного редактирования оценки");
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка при редактировании оценки: {ex.Message}");
            LogError(ex, "Ошибка при редактировании оценки");
        }
    }

    /// <summary>
    /// Удаляет выбранную оценку
    /// </summary>
    private async Task DeleteGradeAsync(GradeViewModel gradeViewModel)
    {
        if (gradeViewModel == null) return;

        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить оценку?");

            if (result)
            {
                await _gradeService.DeleteAsync(gradeViewModel.Uid);
                await LoadGradesAsync();
                await _notificationService.CreateNotificationAsync(
                    gradeViewModel.Grade.StudentUid,
                    "Оценка удалена",
                    "Одна из ваших оценок была удалена преподавателем",
                    NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка при удалении оценки: {ex.Message}");
            LogError(ex, "Ошибка при удалении оценки");
        }
    }

    /// <summary>
    /// Массовое редактирование выбранных оценок (последовательно открывает диалог
    /// редактирования для каждой выбранной оценки, как EditGradeAsync)
    /// </summary>
    private async Task BulkEditGradesAsync()
    {
        var selected = Grades.Where(g => g.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var students = await _studentService.GetAllAsync();
            var assignments = await _assignmentService.GetAllAsync();

            var updated = 0;
            foreach (var item in selected)
            {
                var grade = new Grade
                {
                    Uid = item.Uid,
                    Value = item.Value,
                    Type = (GradeType)item.Type,
                    Comment = item.Comment,
                    IsPublished = item.IsPublished,
                    StudentUid = item.StudentUid,
                    AssignmentUid = item.AssignmentUid
                };

                var result = await _dialogService.ShowGradeEditDialogAsync(grade, students, assignments);
                if (result is Grade updatedGrade)
                {
                    await _gradeService.UpdateAsync(updatedGrade);
                    updated++;
                }
            }

            if (updated > 0)
            {
                await LoadGradesAsync();
                ShowSuccess($"Обновлено оценок: {updated}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового редактирования оценок");
            ShowError("Не удалось выполнить массовое редактирование оценок");
        }
    }

    /// <summary>
    /// Массовое утверждение (публикация) выбранных оценок
    /// </summary>
    private async Task BulkApproveGradesAsync()
    {
        var selected = Grades.Where(g => g.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var success = await _gradeService.BulkPublishGradesAsync(selected.Select(g => g.Uid));
            if (success)
            {
                foreach (var item in selected)
                {
                    item.Grade.IsPublished = true;
                }
                ShowSuccess($"Утверждено оценок: {selected.Count}");
            }
            else
            {
                ShowError("Не удалось утвердить оценки");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового утверждения оценок");
            ShowError("Не удалось утвердить оценки");
        }
    }

    /// <summary>
    /// Массовое отклонение (снятие публикации) выбранных оценок
    /// </summary>
    private async Task BulkRejectGradesAsync()
    {
        var selected = Grades.Where(g => g.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            foreach (var item in selected)
            {
                await _gradeService.UnpublishGradeAsync(item.Uid);
                item.Grade.IsPublished = false;
            }
            ShowSuccess($"Отклонено оценок: {selected.Count}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового отклонения оценок");
            ShowError("Не удалось отклонить оценки");
        }
    }

    private Task SelectAllGradesAsync()
    {
        foreach (var grade in Grades)
        {
            grade.IsSelected = true;
        }
        return Task.CompletedTask;
    }

    private Task DeselectAllGradesAsync()
    {
        foreach (var grade in Grades)
        {
            grade.IsSelected = false;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Валидация данных оценки
    /// </summary>
    private async Task<DomainValidationResult> ValidateGradeAsync(Grade grade)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (grade.Value <= 0)
            errors.Add("Значение оценки обязательно");
        else if (grade.Value < 0 || grade.Value > 100)
            errors.Add("Значение оценки должно быть от 0 до 100");

        // Проверка студента
        if (grade.StudentUid == Guid.Empty)
            errors.Add("Необходимо указать студента");
        else
        {
            var student = await _studentService.GetByUidAsync(grade.StudentUid);
            if (student == null)
                errors.Add("Указанный студент не существует");
        }

        // Проверка курса или задания
        if (grade.CourseInstanceUid != null && grade.CourseInstanceUid != Guid.Empty)
        {
            var courseInstance = await _courseInstanceService.GetByUidAsync(grade.CourseInstanceUid);
            if (courseInstance == null)
                errors.Add("Указанный экземпляр курса не существует");
        }

        if (grade.AssignmentUid != null && grade.AssignmentUid != Guid.Empty)
        {
            var assignment = await _assignmentService.GetByIdAsync(grade.AssignmentUid.Value);
            if (assignment == null)
                errors.Add("Указанное задание не существует");
        }

        if ((grade.CourseInstanceUid == null || grade.CourseInstanceUid == Guid.Empty) && 
            (grade.AssignmentUid == null || grade.AssignmentUid == Guid.Empty))
            errors.Add("Необходимо указать курс или задание");

        // Проверка дат
        if (grade.GradedAt > DateTime.UtcNow)
            warnings.Add("Дата выставления оценки в будущем");

        // Проверка дублирования
        if (grade.AssignmentUid.HasValue && grade.AssignmentUid.Value != Guid.Empty)
        {
            var existingGrade = await _gradeService.GetByStudentAndAssignmentAsync(
                grade.StudentUid, grade.AssignmentUid.Value);
            if (existingGrade != null && existingGrade.Uid != grade.Uid)
                errors.Add("Оценка за это задание уже существует для данного студента");
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            return await _permissionService.HasPermissionAsync(permission);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to check permission: {permission}");
            return false;
        }
    }

    private async Task ViewGradeDetailsAsync(GradeViewModel gradeViewModel)
    {
        if (gradeViewModel == null) return;

        LogInfo($"Viewing grade details: {GradeUid}");
        
        try
        {
            await NavigateToAsync($"grade-details/{gradeViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to grade details");
            ShowError("Не удалось открыть детали оценки");
        }
    }

    private async Task AddCommentAsync(GradeViewModel gradeViewModel)
    {
        // Реализация добавления комментария к оценке
        LogInfo($"Adding comment to grade: {GradeUid}");
    }

    private async Task BulkGradingAsync()
    {
        // Реализация массового выставления оценок
        LogInfo("Starting bulk grading");
    }

    private async Task ExportReportAsync()
    {
        // Реализация экспорта отчета
        LogInfo("Exporting grades report");
    }

    private async Task ExportToExcelAsync()
    {
        // Реализация экспорта в Excel
        LogInfo("Exporting grades to Excel");
    }

    private async Task GenerateAnalyticsReportAsync()
    {
        // Реализация генерации аналитического отчета
        LogInfo("Generating analytics report");
    }

    private async Task NotifyParentsAsync()
    {
        // Реализация уведомления родителей
        LogInfo("Notifying parents about grades");
    }

    private async Task SearchGradesAsync(string searchTerm)
    {
        LogInfo($"Searching grades with term: {SearchTerm}");
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadGradesAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters to grades");
        CurrentPage = 1; // Сброс на первую страницу при применении фильтров
        await LoadGradesAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing all filters");
        SelectedCourse = null;
        SelectedGroupFilter = null;
        GradeRangeFilter = null;
        PeriodFilter = null;
        CurrentPage = 1;
        await LoadGradesAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadGradesAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadGradesAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadGradesAsync();
        }
    }

    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await LoadGradesAsync();
    }

    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadGradesAsync();
    }

    /// <summary>
    /// Парсит фильтр диапазона оценок
    /// </summary>
    private (decimal? min, decimal? max) ParseGradeRangeFilter()
    {
        if (string.IsNullOrWhiteSpace(GradeRangeFilter))
            return (null, null);

        try
        {
            var parts = GradeRangeFilter.Split('-');
            if (parts.Length == 2)
            {
                var minStr = parts[0].Trim();
                var maxStr = parts[1].Trim();

                decimal? min = string.IsNullOrEmpty(minStr) ? null : decimal.Parse(minStr);
                decimal? max = string.IsNullOrEmpty(maxStr) ? null : decimal.Parse(maxStr);

                return (min, max);
            }
            else if (decimal.TryParse(GradeRangeFilter, out var singleValue))
            {
                return (singleValue, singleValue);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to parse grade range filter: {GradeRangeFilter}");
        }

        return (null, null);
    }

    /// <summary>
    /// Парсит фильтр периода
    /// </summary>
    private (DateTime? start, DateTime? end) ParsePeriodFilter()
    {
        if (string.IsNullOrWhiteSpace(PeriodFilter))
            return (null, null);

        try
        {
            return PeriodFilter.ToLower() switch
            {
                "текущий семестр" => GetCurrentSemesterDates(),
                "прошлый месяц" => (DateTime.Now.AddMonths(-1).Date, DateTime.Now.Date),
                "текущий месяц" => (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now.Date),
                "текущая неделя" => (DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date, DateTime.Now.Date),
                _ => (null, null)
            };
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to parse period filter: {PeriodFilter}");
            return (null, null);
        }
    }

    /// <summary>
    /// Получает даты текущего семестра
    /// </summary>
    private (DateTime start, DateTime end) GetCurrentSemesterDates()
    {
        var now = DateTime.Now;
        var year = now.Year;
        
        // Примерные даты семестров (можно настроить)
        if (now.Month >= 9 || now.Month <= 1) // Осенний семестр
        {
            var start = new DateTime(year, 9, 1);
            var end = new DateTime(year + 1, 1, 31);
            return (start, end);
        }
        else // Весенний семестр
        {
            var start = new DateTime(year, 2, 1);
            var end = new DateTime(year, 6, 30);
            return (start, end);
        }
    }

    private async Task<bool> CanExportGradesAsync()
    {
        return await HasPermissionAsync("Grades.Export");
    }
}

public class GradeStatistics
{
    public double AverageGrade { get; set; }
    public int ExcellentCount { get; set; }
    public int GoodCount { get; set; }
    public int SatisfactoryCount { get; set; }
    public int UnsatisfactoryCount { get; set; }
    public double PassRate { get; set; }
    public double QualityRate { get; set; }
    public int TotalGrades { get; set; }
} 

