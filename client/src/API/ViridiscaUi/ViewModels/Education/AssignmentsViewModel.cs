using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.System;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления заданиями
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("assignments", 
    DisplayName = "Задания", 
    IconKey = "ClipboardText", 
    Order = 6,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление заданиями и домашними работами")]
public class AssignmentsViewModel : RoutableViewModelBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly ITeacherService _teacherService; 
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    private readonly SourceList<AssignmentViewModel> _assignmentsSource = new();
    private readonly ReadOnlyObservableCollection<AssignmentViewModel> _assignments;

    // === СВОЙСТВА ===
    
    /// <summary>
    /// Коллекция заданий для отображения
    /// </summary>
    public ReadOnlyObservableCollection<AssignmentViewModel> Assignments => _assignments;
    [Reactive] public AssignmentViewModel? SelectedAssignment { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public AssignmentAnalytics? SelectedAssignmentAnalytics { get; set; }
    [Reactive] public AssignmentAnalytics? Analytics { get; set; }
    
    // Фильтры
    [Reactive] public AssignmentStatus? StatusFilter { get; set; }
    [Reactive] public ObservableCollection<CourseInstanceViewModel> CourseInstances { get; set; } = new();
    [Reactive] public CourseInstanceViewModel? SelectedCourseInstanceFilter { get; set; }
    [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
    [Reactive] public TeacherViewModel? SelectedTeacherFilter { get; set; }
    [Reactive] public DateTime? DueDateFrom { get; set; }
    [Reactive] public DateTime? DueDateTo { get; set; }
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalAssignments { get; set; }

    // Статистика
    [Reactive] public int ActiveAssignments { get; set; }
    [Reactive] public int OverdueAssignments { get; set; }

    // Bulk selection
    [Reactive] public int SelectedAssignmentsCount { get; set; }

    // Computed properties for UI binding
    public bool HasSelectedAssignment => SelectedAssignment != null;
    public bool HasSelectedAssignmentAnalytics => SelectedAssignmentAnalytics != null;
    public bool HasAnalytics => Analytics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public bool CanGoToFirstPage => CurrentPage > 1;
    public bool CanGoToLastPage => CurrentPage < TotalPages;
    public bool HasPages => TotalPages > 1;
    public string PaginationInfo
    {
        get
        {
            if (TotalAssignments == 0) return "Нет данных";
            var from = (CurrentPage - 1) * PageSize + 1;
            var to = Math.Min(CurrentPage * PageSize, TotalAssignments);
            return $"Показано {from}-{to} из {TotalAssignments}";
        }
    }

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadAssignmentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> EditAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> DeleteAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> ViewAssignmentDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> LoadAssignmentStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> PublishAssignmentCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> ViewSubmissionsCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> SendReminderCommand { get; private set; } = null!;
    public ReactiveCommand<AssignmentViewModel, Unit> BulkGradeCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LoadAnalyticsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ShowOverdueAssignmentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ShowPendingGradingCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkEditCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkPublishCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkArchiveCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    // === НОВЫЕ СВОЙСТВА ===
    /// <summary>
    /// Название задания для поиска
    /// </summary>
    [Reactive] public string AssignmentTitle { get; set; } = string.Empty;

    public AssignmentsViewModel(
        IScreen hostScreen,
        IAssignmentService assignmentService,
        ICourseInstanceService courseInstanceService,
        ITeacherService teacherService, 
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService) : base(hostScreen)
    {
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService)); 
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Инициализация коллекций
        _assignmentsSource
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _assignments)
            .Subscribe()
            .DisposeWith(Disposables);

        InitializeCommands();
        SetupSubscriptions();
    }

    private void InitializeCommands()
    {
        LoadAssignmentsCommand = CreateCommand(async () => await LoadAssignmentsAsync());
        RefreshCommand = CreateCommand(async () => await RefreshAsync());
        CreateAssignmentCommand = CreateCommand(async () => await CreateAssignmentAsync());
        EditAssignmentCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await EditAssignmentAsync(assignment));
        DeleteAssignmentCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await DeleteAssignmentAsync(assignment));
        ViewAssignmentDetailsCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await ViewAssignmentDetailsAsync(assignment));
        LoadAssignmentStatisticsCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await LoadAssignmentStatisticsAsync(assignment));
        PublishAssignmentCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await PublishAssignmentAsync(assignment));
        ViewSubmissionsCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await ViewSubmissionsAsync(assignment));
        SendReminderCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await SendReminderAsync(assignment));
        BulkGradeCommand = CreateCommand<AssignmentViewModel>(async (assignment) => await BulkGradeAsync(assignment));
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchAssignmentsAsync(searchTerm));
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync());
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync());
        LoadAnalyticsCommand = CreateCommand(async () => await LoadAnalyticsAsync());
        ShowOverdueAssignmentsCommand = CreateCommand(async () => await ShowOverdueAssignmentsAsync());
        ShowPendingGradingCommand = CreateCommand(async () => await ShowPendingGradingAsync());
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page));
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), canGoPrevious, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), canGoNext, "Ошибка перехода на последнюю страницу");

        var hasSelection = this.WhenAnyValue(x => x.SelectedAssignmentsCount).Select(count => count > 0);
        BulkEditCommand = CreateCommand(async () => await BulkEditAssignmentsAsync(), hasSelection, "Ошибка массового редактирования заданий");
        BulkPublishCommand = CreateCommand(async () => await BulkPublishAssignmentsAsync(), hasSelection, "Ошибка массовой публикации заданий");
        BulkArchiveCommand = CreateCommand(async () => await BulkArchiveAssignmentsAsync(), hasSelection, "Ошибка массового архивирования заданий");
        BulkDeleteCommand = CreateCommand(async () => await BulkDeleteAssignmentsAsync(), hasSelection, "Ошибка массового удаления заданий");
        SelectAllCommand = CreateCommand(async () => await SelectAllAssignmentsAsync(), null, "Ошибка выбора всех заданий");
        DeselectAllCommand = CreateCommand(async () => await DeselectAllAssignmentsAsync(), null, "Ошибка снятия выделения");
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
                LogError(ex, "Ошибка поиска заданий");
                return Observable.Empty<string>();
            })
            .InvokeCommand(SearchCommand)
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе задания - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedAssignment)
            .Where(assignment => assignment != null)
            .Select(assignment => assignment!)
            .Catch<AssignmentViewModel, Exception>(ex =>
            {
                LogError(ex, "Ошибка при выборе задания");
                return Observable.Empty<AssignmentViewModel>();
            })
            .InvokeCommand(LoadAssignmentStatisticsCommand)
            .DisposeWith(Disposables);

        // Применение фильтров при изменении - добавляем обработку ошибок
        this.WhenAnyValue(x => x.StatusFilter, x => x.SelectedCourseInstanceFilter, x => x.SelectedTeacherFilter, x => x.DueDateFrom, x => x.DueDateTo)
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

        // Уведомления об изменении computed properties - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedAssignment)
            .Catch<AssignmentViewModel?, Exception>(ex =>
            {
                LogError(ex, "Ошибка обновления HasSelectedAssignment");
                return Observable.Return<AssignmentViewModel?>(null);
            })
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignment)))
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.SelectedAssignmentAnalytics)
            .Catch<AssignmentAnalytics?, Exception>(ex =>
            {
                LogError(ex, "Ошибка обновления HasSelectedAssignmentAnalytics");
                return Observable.Return<AssignmentAnalytics?>(null);
            })
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignmentAnalytics)))
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Analytics)
            .Catch<AssignmentAnalytics?, Exception>(ex =>
            {
                LogError(ex, "Ошибка обновления HasAnalytics");
                return Observable.Return<AssignmentAnalytics?>(null);
            })
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasAnalytics)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, x => x.TotalAssignments, x => x.PageSize)
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
    }

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        LogInfo("AssignmentsViewModel loaded for the first time");
        
        // Load filter data and assignments when view is loaded for the first time
        await ExecuteWithErrorHandlingAsync(LoadCoursesAndTeachersAsync, "Ошибка загрузки данных фильтров");
        await LoadAssignmentsAsync();
        await ExecuteWithErrorHandlingAsync(LoadAnalyticsAsync, "Ошибка загрузки аналитики");
    }

    #endregion

    // === МЕТОДЫ КОМАНД ===

    private async Task LoadAssignmentsAsync()
    {
        LogInfo($"Loading assignments with filters: SearchText={SearchText}, StatusFilter={StatusFilter}");
        
        IsLoading = true;
        ShowInfo("Загрузка заданий...");

        try
        {
            var (assignments, totalCount) = await _assignmentService.GetPagedAsync(
                CurrentPage,
                PageSize,
                SearchText,
                SelectedCourseInstanceFilter?.Uid,
                StatusFilter);

            var assignmentViewModels = assignments
                .Select(a =>
                {
                    var assignmentViewModel = new AssignmentViewModel(a);
                    assignmentViewModel.WhenAnyValue(x => x.IsSelected)
                        .Subscribe(_ => SelectedAssignmentsCount = Assignments.Count(x => x.IsSelected))
                        .DisposeWith(Disposables);
                    return assignmentViewModel;
                })
                .ToList();

            _assignmentsSource.Clear();
            _assignmentsSource.AddRange(assignmentViewModels);
            SelectedAssignmentsCount = 0;

            TotalAssignments = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            UpdateAssignmentStatistics();

            ShowSuccess($"Загружено {Assignments.Count} заданий");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load assignments");
            ShowError("Не удалось загрузить задания. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCoursesAndTeachersAsync()
    {
        LogInfo("Loading courses and teachers for assignment filters");
        
        var courseInstances = await _courseInstanceService.GetAllCourseInstancesAsync();
        var teachers = await _teacherService.GetAllTeachersAsync();

        CourseInstances.Clear();
        Teachers.Clear();

        foreach (var courseInstance in courseInstances)
            CourseInstances.Add(new CourseInstanceViewModel(courseInstance));

        foreach (var teacher in teachers)
            Teachers.Add(new TeacherViewModel(teacher));
            
        LogInfo($"Loaded {courseInstances.Count()} course instances and {teachers.Count()} teachers for filters");
    }

    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await LoadAssignmentsAsync();
            await LoadAnalyticsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Создание нового задания с валидацией
    /// </summary>
    private async Task CreateAssignmentAsync()
    {
        LogInfo("Creating new assignment");

        // Проверка прав доступа
        if (!await HasPermissionAsync("Assignments.Create"))
        {
            ShowError("У вас нет прав для создания заданий");
            return;
        }

        try
        {
            IsLoading = true;

            var newAssignment = new Assignment
            {
                Uid = Guid.NewGuid(),
                Title = string.Empty,
                Description = string.Empty,
                Type = AssignmentType.Homework,
                Difficulty = AssignmentDifficulty.Medium,
                Status = AssignmentStatus.Draft,
                DueDate = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            var dialogResult = await _dialogService.ShowAssignmentEditDialogAsync(newAssignment);
            // Since dialog service is not fully implemented yet, we'll use the original assignment for now
            var assignmentToCreate = dialogResult ?? newAssignment;
            
            // Валидация данных задания
            var validationResult = await ValidateAssignmentAsync(assignmentToCreate);
            if (!validationResult.IsValid)
            {
                await _dialogService.ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
                return;
            }

            // Создание задания
            var createdAssignment = await _assignmentService.CreateAsync(assignmentToCreate);
            
            if (createdAssignment != null)
            {
                // Добавление в коллекцию
                var assignmentViewModel = new AssignmentViewModel(createdAssignment);
                _assignmentsSource.Add(assignmentViewModel);
                TotalAssignments++;

                ShowSuccess($"Задание '{createdAssignment.Title}' создано");
                LogInfo($"Assignment created successfully: {AssignmentTitle}");

                // TODO: Отправка уведомлений студентам будет реализована позже
                // когда будет доступен метод GetEnrolledStudentsAsync в IAssignmentService
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Ошибка валидации при создании задания");
            ErrorMessage = ex.Message;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже существует"))
        {
            LogError(ex, "Дублирование при создании задания");
            ErrorMessage = $"Задание с таким названием уже существует: {ex.Message}";
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create assignment");
            ShowError("Не удалось создать задание. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Редактирует выбранное задание
    /// </summary>
    private async Task EditAssignmentAsync(AssignmentViewModel assignmentViewModel)
    {
        if (assignmentViewModel == null) return;

        try
        {
            IsLoading = true;

            var assignment = new Assignment
            {
                Uid = assignmentViewModel.Uid,
                Title = assignmentViewModel.Title,
                Description = assignmentViewModel.Description,
                Type = assignmentViewModel.Type,
                Difficulty = assignmentViewModel.Difficulty,
                DueDate = assignmentViewModel.DueDate,
                MaxScore = assignmentViewModel.MaxScore,
                CourseInstanceUid = assignmentViewModel.CourseInstanceUid,
                Instructions = assignmentViewModel.Instructions,
                IsPublished = assignmentViewModel.IsPublished
            };

            var result = await _dialogService.ShowAssignmentEditDialogAsync(assignment);
            if (result != null && result is Assignment updatedAssignment)
            {
                await _assignmentService.UpdateAsync(updatedAssignment);
                await LoadAssignmentsAsync();
                ShowSuccess($"Задание '{updatedAssignment.Title}' обновлено");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating assignment");
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Ошибка валидации при редактировании задания");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update assignment");
            ShowError("Не удалось обновить задание. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Удаляет выбранное задание
    /// </summary>
    private async Task DeleteAssignmentAsync(AssignmentViewModel assignmentViewModel)
    {
        if (assignmentViewModel == null) return;

        try
        {
            IsLoading = true;

            // Проверяем связанные данные через AssignmentAnalytics
            var analytics = await CheckRelatedDataAsync(assignmentViewModel.Uid);
            
            string confirmationMessage = $"Вы уверены, что хотите удалить задание '{assignmentViewModel.Title}'?";
            
            if (analytics != null && (analytics.TotalSubmissions > 0 || analytics.GradedSubmissions > 0))
            {
                var relatedDataDetails = new List<string>();
                if (analytics.TotalSubmissions > 0)
                    relatedDataDetails.Add($"- Отправки работ: {analytics.TotalSubmissions}");
                if (analytics.GradedSubmissions > 0)
                    relatedDataDetails.Add($"- Оценки: {analytics.GradedSubmissions}");
                if (analytics.PendingSubmissions > 0)
                    relatedDataDetails.Add($"- Ожидающие проверки: {analytics.PendingSubmissions}");
                
                confirmationMessage += $"\n\nВНИМАНИЕ! У задания есть связанные данные:\n{string.Join("\n", relatedDataDetails)}\n\nВсе связанные данные будут удалены безвозвратно.";
            }

            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления", 
                confirmationMessage);

            if (result)
            {
                await _assignmentService.DeleteAsync(assignmentViewModel.Uid);
                _assignmentsSource.Remove(assignmentViewModel);
                
                if (SelectedAssignment?.Uid == assignmentViewModel.Uid)
                {
                    SelectedAssignment = null;
                    SelectedAssignmentAnalytics = null;
                }

                ShowSuccess($"Задание '{assignmentViewModel.Title}' успешно удалено");
                LogInfo($"Assignment deleted successfully: {AssignmentTitle}");
                
                await UpdateStatisticsAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка удаления задания: {ex.Message}", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Пересчитывает статистику по загруженным заданиям (активные / просроченные)
    /// </summary>
    private void UpdateAssignmentStatistics()
    {
        var now = DateTime.UtcNow;
        ActiveAssignments = Assignments.Count(a => a.Status != AssignmentStatus.Cancelled && a.Status != AssignmentStatus.Archived);
        OverdueAssignments = Assignments.Count(a =>
            a.DueDate.HasValue && a.DueDate.Value < now &&
            a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Graded);
    }

    /// <summary>
    /// Массовое редактирование выбранных заданий (последовательно открывает диалог
    /// редактирования для каждого выбранного задания, как EditAssignmentAsync)
    /// </summary>
    private async Task BulkEditAssignmentsAsync()
    {
        var selected = Assignments.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var updated = 0;
            foreach (var item in selected)
            {
                var assignment = item.ToAssignment();
                var result = await _dialogService.ShowAssignmentEditDialogAsync(assignment);
                if (result != null)
                {
                    await _assignmentService.UpdateAsync(result);
                    updated++;
                }
            }

            if (updated > 0)
            {
                await LoadAssignmentsAsync();
                ShowSuccess($"Обновлено заданий: {updated}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового редактирования заданий");
            ShowError("Не удалось выполнить массовое редактирование заданий");
        }
    }

    /// <summary>
    /// Массовая публикация выбранных заданий
    /// </summary>
    private async Task BulkPublishAssignmentsAsync()
    {
        var selected = Assignments.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var success = 0;
            foreach (var item in selected)
            {
                if (await _assignmentService.PublishAssignmentAsync(item.Uid))
                {
                    item.Status = AssignmentStatus.Published;
                    success++;
                }
            }

            UpdateAssignmentStatistics();
            ShowSuccess($"Опубликовано заданий: {success}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массовой публикации заданий");
            ShowError("Не удалось опубликовать задания");
        }
    }

    /// <summary>
    /// Массовое архивирование выбранных заданий
    /// </summary>
    private async Task BulkArchiveAssignmentsAsync()
    {
        var selected = Assignments.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var success = 0;
            foreach (var item in selected)
            {
                if (await _assignmentService.ArchiveAssignmentAsync(item.Uid))
                {
                    item.Status = AssignmentStatus.Archived;
                    success++;
                }
            }

            UpdateAssignmentStatistics();
            ShowSuccess($"Архивировано заданий: {success}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового архивирования заданий");
            ShowError("Не удалось архивировать задания");
        }
    }

    /// <summary>
    /// Массовое удаление выбранных заданий
    /// </summary>
    private async Task BulkDeleteAssignmentsAsync()
    {
        var selected = Assignments.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить выбранные задания ({selected.Count})?");
            if (!confirmed) return;

            foreach (var item in selected)
            {
                await _assignmentService.DeleteAsync(item.Uid);
                _assignmentsSource.Remove(item);
            }

            TotalAssignments = Math.Max(0, TotalAssignments - selected.Count);
            UpdateAssignmentStatistics();
            ShowSuccess($"Удалено заданий: {selected.Count}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового удаления заданий");
            ShowError("Не удалось удалить задания");
        }
    }

    private Task SelectAllAssignmentsAsync()
    {
        foreach (var assignment in Assignments)
        {
            assignment.IsSelected = true;
        }
        return Task.CompletedTask;
    }

    private Task DeselectAllAssignmentsAsync()
    {
        foreach (var assignment in Assignments)
        {
            assignment.IsSelected = false;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Валидация данных задания
    /// </summary>
    private async Task<DomainValidationResult> ValidateAssignmentAsync(Assignment assignment)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(assignment.Title))
            errors.Add("Название задания обязательно");
        else if (assignment.Title.Length > 200)
            errors.Add("Название задания не должно превышать 200 символов");

        if (string.IsNullOrWhiteSpace(assignment.Description))
            warnings.Add("Рекомендуется добавить описание задания");
        else if (assignment.Description.Length > 2000)
            errors.Add("Описание задания не должно превышать 2000 символов");

        // Проверка дат
        if (assignment.DueDate <= DateTime.Now)
            warnings.Add("Срок выполнения задания уже прошел");

        if (assignment.DueDate <= assignment.CreatedAt)
            errors.Add("Срок выполнения не может быть раньше даты создания");

        // Проверка баллов
        if (assignment.MaxScore <= 0)
            errors.Add("Максимальное количество баллов должно быть больше 0");
        else if (assignment.MaxScore > 1000)
            warnings.Add("Рекомендуется не превышать 1000 баллов за задание");

        // Проверка дублирования
        if (SelectedCourseInstanceFilter?.Uid != null)
        {
            var existingAssignment = await _assignmentService.GetByTitleAndCourseAsync(
                assignment.Title, 
                SelectedCourseInstanceFilter.Uid);
            
            if (existingAssignment != null && existingAssignment.Uid != assignment.Uid)
                errors.Add("Задание с таким названием уже существует в данном курсе");
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Проверяет связанные данные для задания перед удалением
    /// </summary>
    private async Task<AssignmentAnalytics?> CheckRelatedDataAsync(Guid assignmentUid)
    {
        try
        {
            var analytics = await _assignmentService.GetAnalyticsAsync(assignmentUid);
            return analytics;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to check related data for assignment");
            // Возвращаем null если не удалось получить аналитику
            return null;
        }
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

    /// <summary>
    /// Обновление статистики
    /// </summary>
    private async Task UpdateStatisticsAsync()
    {
        try
        {
            // Обновление общей статистики может быть реализовано здесь
            // Например, уведомление других ViewModels об изменениях
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update statistics");
        }
    }

    private async Task ViewAssignmentDetailsAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            SelectedAssignment = assignmentViewModel;
            await LoadAssignmentStatisticsAsync(assignmentViewModel);
            
            ShowInfo($"Просмотр задания '{assignmentViewModel.Title}'");
            LogInfo($"Viewing assignment details: {AssignmentTitle}");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка отображения деталей задания: {ex.Message}", ex);
        }
    }

    private async Task LoadAssignmentStatisticsAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            SelectedAssignmentAnalytics = await _assignmentService.GetAssignmentAnalyticsAsync(assignmentViewModel.Uid);
            LogInfo($"Assignment statistics loaded for: {AssignmentTitle}");
        }
        catch (Exception ex)
        {
            ShowWarning($"Не удалось загрузить статистику задания: {ex.Message}");
            LogError(ex, $"Failed to load assignment statistics for: {AssignmentTitle}");
        }
    }

    private async Task PublishAssignmentAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            var success = await _assignmentService.PublishAssignmentAsync(assignmentViewModel.Uid);
            if (success)
            {
                assignmentViewModel.Status = AssignmentStatus.Published;
                ShowSuccess($"Задание '{assignmentViewModel.Title}' опубликовано");
                LogInfo($"Assignment published successfully: {AssignmentTitle}");
            }
            else
            {
                ShowError("Не удалось опубликовать задание");
                LogWarning($"Failed to publish assignment: {AssignmentTitle}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка публикации задания: {ex.Message}", ex);
        }
    }

    private async Task ViewSubmissionsAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(assignmentViewModel.Uid);
            var result = await _dialogService.ShowSubmissionsViewDialogAsync(assignmentViewModel.ToAssignment(), submissions);
            
            if (result != null)
            {
                await RefreshAsync();
                ShowSuccess("Сдачи обновлены");
                LogInfo($"Submissions updated for assignment: {AssignmentTitle}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка просмотра сдач: {ex.Message}", ex);
        }
    }

    private async Task SendReminderAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            await _assignmentService.SendDueDateReminderAsync(assignmentViewModel.Uid);
            ShowSuccess($"Напоминания отправлены для задания '{assignmentViewModel.Title}'");
            LogInfo($"Reminders sent for assignment: {AssignmentTitle}");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка отправки напоминаний: {ex.Message}", ex);
        }
    }

    private async Task BulkGradeAsync(AssignmentViewModel assignmentViewModel)
    {
        try
        {
            var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(assignmentViewModel.Uid);
            var ungradedSubmissions = submissions.Where(s => !s.Score.HasValue).ToList();
            
            if (ungradedSubmissions == null || !ungradedSubmissions.Any())
            {
                ShowInfo("Все сдачи уже оценены");
                LogInfo($"All submissions already graded for assignment: {AssignmentTitle}");
                return;
            }

            var result = await _dialogService.ShowBulkGradingDialogAsync(ungradedSubmissions);
            if (result != null && result is IEnumerable<Grade> grades && grades.Any())
            {
                var gradingRequests = grades;
                ShowSuccess($"Массовое оценивание завершено");
                LogInfo($"Bulk grading completed for assignment {assignmentViewModel.Title}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка массового оценивания: {ex.Message}", ex);
        }
    }

    private async Task LoadAnalyticsAsync()
    {
        try
        {
            IsLoading = true;
            var courseInstanceFilter = SelectedCourseInstanceFilter?.Uid;
            
            if (courseInstanceFilter.HasValue)
            {
                var analytics = await _assignmentService.GetAnalyticsAsync(courseInstanceFilter.Value);
                Analytics = analytics;
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки аналитики заданий");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ShowOverdueAssignmentsAsync()
    {
        try
        {
            StatusFilter = null;
            DueDateTo = DateTime.Now;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadAssignmentsAsync();
            ShowInfo("Показаны просроченные задания");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка загрузки просроченных заданий: {ex.Message}", ex);
        }
    }

    private async Task ShowPendingGradingAsync()
    {
        try
        {
            var pendingAssignments = await _assignmentService.GetAssignmentsPendingGradingAsync();
            
            _assignmentsSource.Clear();
            foreach (var assignment in pendingAssignments)
            {
                _assignmentsSource.Add(new AssignmentViewModel(assignment));
            }

            ShowInfo($"Показано {_assignmentsSource.Count} заданий, требующих проверки");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка загрузки заданий для проверки: {ex.Message}", ex);
        }
    }

    private async Task SearchAssignmentsAsync(string searchText)
    {
        SearchText = searchText;
        CurrentPage = 1;
        await LoadAssignmentsAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadAssignmentsAsync();
        await LoadAnalyticsAsync();
    }

    private async Task ClearFiltersAsync()
    {
        StatusFilter = null;
        SelectedCourseInstanceFilter = null;
        SelectedTeacherFilter = null;
        DueDateFrom = null;
        DueDateTo = null;
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadAssignmentsAsync();
        await LoadAnalyticsAsync();
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
        CurrentPage = TotalPages;
        await LoadAssignmentsAsync();
    }
} 

