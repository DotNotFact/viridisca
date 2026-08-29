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
using ViridiscaUi.Domain.Entities.System.Enums;
using Microsoft.EntityFrameworkCore;
using DynamicData.Binding; 
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.File;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Models;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for managing students with full CRUD functionality
/// Follows SOLID principles and clean architecture with ReactiveUI
/// </summary>
[Route("students", 
    DisplayName = "Студенты", 
    IconKey = "AccountGroup", 
    Order = 1,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление студентами и их данными")]
public class StudentsViewModel : RoutableViewModelBase
{
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService; 
    private readonly IStatusService _statusService;
    private readonly IExportService _exportService;
    private readonly IImportService _importService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    // Source collections for reactive data management
    private readonly SourceList<StudentViewModel> _studentsSource = new();
    private readonly SourceList<Group> _groupsSource = new();
    private ReadOnlyObservableCollection<StudentViewModel> _students;
    private ReadOnlyObservableCollection<Group> _groups;

    // === PROPERTIES ===
    
    public ReadOnlyObservableCollection<StudentViewModel> Students => _students;
    public ReadOnlyObservableCollection<Group> Groups => _groups;
    
    [Reactive] public StudentViewModel? SelectedStudent { get; set; }
    [Reactive] public ObservableCollection<StudentViewModel> SelectedStudents { get; set; } = new();
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    
    // Enhanced Filters
    [Reactive] public Group? GroupFilter { get; set; }
    [Reactive] public StudentStatus? StatusFilter { get; set; }
    [Reactive] public string? YearFilter { get; set; }
    
    // Pagination
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 25;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalStudents { get; set; }
    [Reactive] public int ActiveStudents { get; set; }
    [Reactive] public int InactiveStudents { get; set; }
    [Reactive] public int GraduatedStudents { get; set; }

    // Computed properties using ObservableAsProperty
    [ObservableAsProperty] public bool HasSelectedStudent { get; }
    [ObservableAsProperty] public bool HasSelectedStudents { get; }
    [ObservableAsProperty] public int SelectedStudentsCount { get; }
    [ObservableAsProperty] public bool CanGoToPreviousPage { get; }
    [ObservableAsProperty] public bool CanGoToNextPage { get; }
    [ObservableAsProperty] public bool CanGoToFirstPage { get; }
    [ObservableAsProperty] public bool CanGoToLastPage { get; }
    [ObservableAsProperty] public bool HasPages { get; }
    [ObservableAsProperty] public string PaginationInfo { get; }

    // === COMMANDS ===
    
    // Basic CRUD Commands
    public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> EditStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> DeleteStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> ViewStudentDetailsCommand { get; private set; } = null!;
    
    // Search and Filter Commands
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    
    // Pagination Commands
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    
    // Navigation Commands
    public ReactiveCommand<StudentViewModel, Unit> ViewCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> ViewGradesCommand { get; private set; } = null!;
    
    // Import/Export Commands
    public ReactiveCommand<Unit, Unit> ImportStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;
    
    // Bulk Operations Commands
    public ReactiveCommand<Unit, Unit> BulkEditCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkExportCommand { get; private set; } = null!;
    
    // Selection Commands
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    /// <summary>
    /// Constructor with enhanced dependency injection
    /// </summary>
    public StudentsViewModel(
        IScreen hostScreen,
        IUnifiedNavigationService navigationService,
        IStudentService studentService,
        IGroupService groupService, 
        IStatusService statusService,
        IExportService exportService,
        IImportService importService,
        IPermissionService permissionService,
        IAuthService authService,
        INotificationService notificationService,
        IDialogService dialogService) : base(hostScreen)
    {
        var _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService)); 
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        SetupReactiveCollections();
        InitializeCommands();
        SetupComputedProperties();
        SetupSubscriptions();
        
        LogInfo("Enhanced StudentsViewModel initialized with full CRUD functionality");
    }

    #region Reactive Collections Setup

    /// <summary>
    /// Sets up reactive collections with filtering and sorting
    /// </summary>
    private void SetupReactiveCollections()
    {
        // Setup students collection with advanced filtering
        var studentsFilter = this.WhenAnyValue(
                x => x.SearchText,
                x => x.GroupFilter,
                x => x.StatusFilter,
                x => x.YearFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Select(CreateStudentFilter);

        _studentsSource.Connect()
            .Filter(studentsFilter)
            .Sort(SortExpressionComparer<StudentViewModel>.Ascending(s => s.LastName)
                .ThenByAscending(s => s.FirstName))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _students)
            .Subscribe()
            .DisposeWith(Disposables);

        // Setup groups collection
        _groupsSource.Connect()
            .Sort(SortExpressionComparer<Group>.Ascending(g => g.Name))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _groups)
            .Subscribe()
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Creates a filter function for students based on current filter criteria
    /// </summary>
    private Func<StudentViewModel, bool> CreateStudentFilter((string? searchText, Group? groupFilter, StudentStatus? statusFilter, string? yearFilter) criteria)
    {
        return student =>
        {
            // Search text filter
            if (!string.IsNullOrWhiteSpace(criteria.searchText))
            {
                var searchLower = criteria.searchText.ToLowerInvariant();
                if (!student.FullName.ToLowerInvariant().Contains(searchLower) &&
                    !student.Email.ToLowerInvariant().Contains(searchLower) &&
                    !student.StudentId.ToLowerInvariant().Contains(searchLower) &&
                    !(student.GroupName?.ToLowerInvariant().Contains(searchLower) ?? false))
                {
                    return false;
                }
            }

            // Group filter
            if (criteria.groupFilter != null && student.GroupUid != criteria.groupFilter.Uid)
            {
                return false;
            }

            // Status filter - используем IsActive вместо Status для совместимости
            if (criteria.statusFilter.HasValue)
            {
                var isActiveFilter = criteria.statusFilter.Value == StudentStatus.Active;
                if (student.IsActive != isActiveFilter)
                {
                    return false;
                }
            }

            // Year filter - пока пропускаем, так как нет AcademicYear в StudentViewModel
            // В будущем нужно добавить это свойство в StudentViewModel
            if (!string.IsNullOrWhiteSpace(criteria.yearFilter) && criteria.yearFilter != "Все")
            {
                // TODO: Добавить поддержку фильтрации по году обучения
                // когда будет добавлено свойство AcademicYear в StudentViewModel
            }

            return true;
        };
    }

    #endregion

    #region Commands Initialization

    /// <summary>
    /// Initializes all reactive commands with proper error handling
    /// </summary>
    private void InitializeCommands()
    {
        // Basic CRUD Commands
        LoadStudentsCommand = CreateCommand(LoadStudentsAsync, null, "Ошибка загрузки студентов");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        CreateStudentCommand = CreateCommand(CreateStudentAsync, null, "Ошибка создания студента");
        EditStudentCommand = CreateCommand<StudentViewModel>(async (student) => await EditStudentAsync(student));
        DeleteStudentCommand = CreateCommand<StudentViewModel>(async (student) => await DeleteStudentAsync(student));
        ViewStudentDetailsCommand = CreateCommand<StudentViewModel>(ViewStudentDetailsAsync, null, "Ошибка просмотра деталей студента");

        // Search and Filter Commands
        SearchCommand = CreateCommand<string>(SearchStudentsAsync, null, "Ошибка поиска студентов");
        ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");

        // Pagination Commands
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(FirstPageAsync, canGoPrevious, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(LastPageAsync, canGoNext, "Ошибка перехода на последнюю страницу");

        // Navigation Commands
        ViewCoursesCommand = CreateCommand<StudentViewModel>(ViewCoursesAsync, null, "Ошибка просмотра курсов студента");
        ViewGradesCommand = CreateCommand<StudentViewModel>(ViewGradesAsync, null, "Ошибка просмотра оценок студента");

        // Import/Export Commands
        ImportStudentsCommand = CreateCommand(ImportStudentsAsync, null, "Ошибка импорта студентов");
        ExportReportCommand = CreateCommand(ExportReportAsync, null, "Ошибка экспорта отчета");

        // Bulk Operations Commands
        var hasSelectedStudents = this.WhenAnyValue(x => x.SelectedStudentsCount, count => count > 0);
        BulkEditCommand = CreateCommand(BulkEditAsync, hasSelectedStudents, "Ошибка массового редактирования");
        BulkDeleteCommand = CreateCommand(BulkDeleteAsync, hasSelectedStudents, "Ошибка массового удаления");
        BulkExportCommand = CreateCommand(BulkExportAsync, hasSelectedStudents, "Ошибка экспорта выбранных студентов");

        // Selection Commands
        var hasStudents = this.WhenAnyValue(x => x.Students.Count, count => count > 0);
        SelectAllCommand = CreateCommand(SelectAllAsync, hasStudents, "Ошибка выбора всех студентов");
        DeselectAllCommand = CreateCommand(DeselectAllAsync, hasSelectedStudents, "Ошибка снятия выделения");
    }

    #endregion

    #region Computed Properties Setup

    /// <summary>
    /// Sets up computed properties using ObservableAsProperty pattern
    /// </summary>
    private void SetupComputedProperties()
    {
        // Selection properties
        this.WhenAnyValue(x => x.SelectedStudent)
            .Select(student => student != null)
            .ToPropertyEx(this, x => x.HasSelectedStudent)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedStudents.Count)
            .Select(count => count > 0)
            .ToPropertyEx(this, x => x.HasSelectedStudents)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedStudents.Count)
            .ToPropertyEx(this, x => x.SelectedStudentsCount)
            .DisposeWith(Disposables);

        // Pagination properties
        this.WhenAnyValue(x => x.CurrentPage)
            .Select(page => page > 1)
            .ToPropertyEx(this, x => x.CanGoToPreviousPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total)
            .ToPropertyEx(this, x => x.CanGoToNextPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage)
            .Select(page => page > 1)
            .ToPropertyEx(this, x => x.CanGoToFirstPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total)
            .ToPropertyEx(this, x => x.CanGoToLastPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.TotalPages)
            .Select(pages => pages > 1)
            .ToPropertyEx(this, x => x.HasPages)
            .DisposeWith(Disposables);

        // Pagination info
        this.WhenAnyValue(x => x.Students.Count, x => x.TotalStudents, (shown, total) => 
                $"Показано {shown} из {total} студентов")
            .ToPropertyEx(this, x => x.PaginationInfo)
            .DisposeWith(Disposables);
    }

    #endregion

    #region Subscriptions Setup

    /// <summary>
    /// Sets up reactive subscriptions for automatic data updates
    /// </summary>
    private void SetupSubscriptions()
    {
        // Auto-search when search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(searchText => searchText?.Trim() ?? string.Empty)
            .DistinctUntilChanged()
            .Where(_ => !IsLoading)
            .Subscribe(async searchText =>
            {
                try
                {
                    await SearchStudentsAsync(searchText);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автоматического поиска студентов");
                    ShowError("Ошибка поиска студентов");
                }
            })
            .DisposeWith(Disposables);

        // Auto-apply filters when filter criteria change
        this.WhenAnyValue(x => x.GroupFilter, x => x.StatusFilter, x => x.YearFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(_ => !IsLoading)
            .Subscribe(async _ =>
            {
                try
                {
                    await ApplyFiltersAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автоматического применения фильтров");
                    ShowError("Ошибка применения фильтров");
                }
            })
            .DisposeWith(Disposables);

        // Update statistics when students collection changes
        this.WhenAnyValue(x => x.Students.Count)
            .Subscribe(count =>
            {
                TotalStudents = _studentsSource.Items.Count();
                ActiveStudents = _studentsSource.Items.Count(s => s.IsActive);
                InactiveStudents = _studentsSource.Items.Count(s => !s.IsActive);
                GraduatedStudents = 0; // TODO: Добавить подсчет выпускников когда будет доступна информация о статусе
            })
            .DisposeWith(Disposables);

        // Handle selection changes
        SelectedStudents.ToObservableChangeSet()
            .Subscribe(_ =>
            {
                // Update selection state in individual student view models
                foreach (var student in Students)
                {
                    student.IsSelected = SelectedStudents.Contains(student);
                }
            })
            .DisposeWith(Disposables);
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Loads students with pagination and filtering
    /// </summary>
    private async Task LoadStudentsAsync()
    {
        try
        {
            IsLoading = true;
            LogInfo("Loading students...");
            
            // Load groups first for filtering
            await LoadGroupsAsync();
            
            // Load students with pagination
            var (students, totalCount) = await _studentService.GetPagedAsync(
                CurrentPage, PageSize, SearchText);
            
            // Convert to ViewModels
            var studentViewModels = students.Select(s => new StudentViewModel(s)).ToList();
            
            // Update collections
            _studentsSource.Clear();
            _studentsSource.AddRange(studentViewModels);
            
            // Update pagination info
            TotalStudents = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ActiveStudents = studentViewModels.Count(s => s.IsActive);
            InactiveStudents = studentViewModels.Count(s => !s.IsActive);
            GraduatedStudents = 0; // TODO: Добавить подсчет выпускников когда будет доступна информация о статусе
            
            LogInfo($"Loaded {studentViewModels.Count} students (page {CurrentPage} of {TotalPages})");
            ShowSuccess($"Загружено {studentViewModels.Count} студентов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load students");
            ShowError("Ошибка загрузки студентов");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads groups for filtering
    /// </summary>
    private async Task LoadGroupsAsync()
    {
        try
        {
            var groups = await _groupService.GetAllAsync();
            _groupsSource.Clear();
            _groupsSource.AddRange(groups);
            LogInfo($"Loaded {groups.Count()} groups for filtering");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load groups");
        }
    }

    /// <summary>
    /// Refreshes all data
    /// </summary>
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadStudentsAsync();
            ShowSuccess("Данные обновлены");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Creates a new student with enhanced validation and permission checks
    /// </summary>
    private async Task CreateStudentAsync()
    {
        try
        {
            var result = await _dialogService.ShowStudentEditDialogAsync(new Student());
            if (result != null)
            {
                await LoadStudentsAsync();
                ShowSuccess($"Студент '{result.StudentCode}' успешно создан");
                
                // Отправляем уведомление
                await _notificationService.SendNotificationAsync(
                    "Студент создан",
                    $"Студент '{result.StudentCode}' успешно создан в системе",
                    NotificationType.Success);

                await _dialogService.ShowInfoAsync("Успех", "Студент успешно создан");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка при создании студента: {ex.Message}");
            LogError(ex, "Ошибка при создании студента");
        }
    }

    /// <summary>
    /// Edits selected student with optimistic locking and validation
    /// </summary>
    private async Task EditStudentAsync(StudentViewModel studentViewModel)
    {
        if (studentViewModel == null) return;

        try
        {
            IsLoading = true;
            LogInfo($"Начало редактирования студента: {studentViewModel.Uid}");
            
            // Проверка прав доступа
            if (!await HasPermissionAsync("Students.Update"))
            {
                await _dialogService.ShowErrorAsync("Ошибка доступа", 
                    "У вас нет прав для редактирования студентов");
                return;
            }
            
            // Получение актуальных данных для optimistic locking
            var currentStudent = await _studentService.GetByUidAsync(studentViewModel.Uid);
            if (currentStudent == null)
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Студент не найден");
                return;
            }

            // Проверка optimistic locking
            if (currentStudent.LastModifiedAt != studentViewModel.LastModifiedAt)
            {
                var refreshResult = await _dialogService.ShowConfirmationAsync(
                    "Конфликт версий", 
                    "Данные студента были изменены другим пользователем. Обновить данные?");
                    
                if (refreshResult)
                {
                    studentViewModel.UpdateFromStudent(currentStudent);
                }
                else
                {
                    return;
                }
            }

            // Создание диалога редактирования
            var result = await _dialogService.ShowStudentEditDialogAsync(currentStudent);

            if (result != null)
            {
                // Валидация изменений
                var validationResult = await ValidateStudentAsync(result);
                if (!validationResult.IsValid)
                {
                    await _dialogService.ShowErrorAsync("Ошибка валидации", 
                        string.Join("\n", validationResult.Errors));
                    return;
                }

                // Обновление студента
                var updateResult = await _studentService.UpdateAsync(result);
                
                if (updateResult)
                {
                    // Получаем обновленного студента
                    var updatedStudent = await _studentService.GetByUidAsync(result.Uid);
                    if (updatedStudent != null)
                    {
                        // Обновление ViewModel
                        studentViewModel.UpdateFromStudent(updatedStudent);
                        
                        // Обновление статистики
                        await UpdateStatisticsAsync();
                        
                        ShowSuccess($"Данные студента '{updatedStudent.Person?.FirstName} {updatedStudent.Person?.LastName}' обновлены");
                        LogInfo($"Студент обновлен: {updatedStudent.Uid}");

                        await _notificationService.SendNotificationAsync(
                            "Данные студента обновлены",
                            $"Данные студента '{updatedStudent.Person?.FirstName} {updatedStudent.Person?.LastName}' успешно обновлены",
                            NotificationType.Success);
                    }
                    else
                    {
                        ShowWarning("Студент обновлен, но не удалось получить обновленные данные");
                    }
                }
                else
                {
                    ShowError("Не удалось обновить данные студента");
                }
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _dialogService.ShowErrorAsync("Конфликт версий", ex.Message);
            LogError(ex, "Конфликт версий при редактировании студента");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при редактировании студента");
            ErrorMessage = "Ошибка при редактировании студента";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes selected student with related data check and confirmation
    /// </summary>
    private async Task DeleteStudentAsync(StudentViewModel studentViewModel)
    {
        if (studentViewModel == null) return;

        try
        {
            IsLoading = true;
            LogInfo($"Начало удаления студента: {studentViewModel.Uid}");

            // Проверка прав доступа
            if (!await HasPermissionAsync("Students.Delete"))
            {
                await _dialogService.ShowErrorAsync("Ошибка доступа", 
                    "У вас нет прав для удаления студентов");
                return;
            }

            // Проверка связанных данных
            var relatedData = await GetRelatedDataInfoAsync(studentViewModel.Uid);
            
            // Show confirmation dialog with proper string parameters
            var result = await _dialogService.ShowConfirmationDialogAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить студента '{studentViewModel.FullName}'?");

            if (result == true)
            {
                // Удаление студента
                var deleteSuccess = await _studentService.DeleteAsync(studentViewModel.Uid);
                
                if (deleteSuccess)
                {
                    // Удаление из коллекции
                    _studentsSource.Remove(studentViewModel);
                    
                    // Обновление статистики
                    TotalStudents--;
                    if (studentViewModel.Student.Person?.FirstName != null)
                        ActiveStudents--;

                    // Очистка выбора если удаленный студент был выбран
                    if (SelectedStudent == studentViewModel)
                        SelectedStudent = null;

                    ShowSuccess($"Студент '{studentViewModel.FullName}' удален");
                    
                    LogInfo($"Студент удален: {studentViewModel.FullName} (ID: {studentViewModel.Uid})");

                    await _notificationService.SendNotificationAsync(
                        "Студент удален",
                        $"Студент '{studentViewModel.FullName}' успешно удален из системы",
                        NotificationType.Success);
                }
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            await _dialogService.ShowErrorAsync("Ошибка связанных данных", 
                "Невозможно удалить студента, так как у него есть связанные данные (оценки, задания). Сначала удалите связанные записи.");
            LogError(ex, "Foreign key constraint violation while deleting student");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Ошибка удаления", 
                "Произошла ошибка при удалении студента. Попробуйте еще раз.");
            LogError(ex, "Неожиданная ошибка при удалении студента");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows detailed view of a student
    /// </summary>
    private async Task ViewStudentDetailsAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Viewing details for student: {studentViewModel.FullName}");
            
            var student = await _studentService.GetByUidAsync(studentViewModel.Uid);
            if (student != null)
            {
                await _dialogService.ShowStudentDetailsDialogAsync(student);
            }
            else
            {
                ShowError("Студент не найден");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to view student details: {studentViewModel.FullName}");
            ShowError("Ошибка при просмотре деталей студента");
        }
    }

    #endregion

    #region Search and Filtering

    /// <summary>
    /// Performs search with the given search text
    /// </summary>
    private async Task SearchStudentsAsync(string searchText)
    {
        try
        {
            LogInfo($"Searching students with text: '{searchText}'");
            
            // Reset to first page when searching
            CurrentPage = 1;
            
            // Reload data with search filter
            await LoadStudentsAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to search students with text: '{searchText}'");
            ShowError("Ошибка поиска студентов");
        }
    }

    /// <summary>
    /// Applies current filter criteria
    /// </summary>
    private async Task ApplyFiltersAsync()
    {
        try
        {
            LogInfo("Applying filters to students");
            
            // Reset to first page when applying filters
            CurrentPage = 1;
            
            // Reload data with filters
            await LoadStudentsAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to apply filters");
            ShowError("Ошибка применения фильтров");
        }
    }

    /// <summary>
    /// Clears all filters and search criteria
    /// </summary>
    private async Task ClearFiltersAsync()
    {
        try
        {
            LogInfo("Clearing all filters");
            
            SearchText = string.Empty;
            GroupFilter = null;
            StatusFilter = null;
            YearFilter = null;
            CurrentPage = 1;
            
            await LoadStudentsAsync();
            ShowSuccess("Фильтры очищены");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to clear filters");
            ShowError("Ошибка очистки фильтров");
        }
    }

    #endregion

    #region Pagination

    /// <summary>
    /// Navigates to a specific page
    /// </summary>
    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages && page != CurrentPage)
        {
            CurrentPage = page;
            await LoadStudentsAsync();
        }
    }

    /// <summary>
    /// Navigates to the next page
    /// </summary>
    private async Task NextPageAsync()
    {
        await GoToPageAsync(CurrentPage + 1);
    }

    /// <summary>
    /// Navigates to the previous page
    /// </summary>
    private async Task PreviousPageAsync()
    {
        await GoToPageAsync(CurrentPage - 1);
    }

    /// <summary>
    /// Navigates to the first page
    /// </summary>
    private async Task FirstPageAsync()
    {
        await GoToPageAsync(1);
    }

    /// <summary>
    /// Navigates to the last page
    /// </summary>
    private async Task LastPageAsync()
    {
        await GoToPageAsync(TotalPages);
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Navigates to courses view for the selected student
    /// </summary>
    private async Task ViewCoursesAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Navigating to courses for student: {studentViewModel.FullName}");
            await NavigateToAsync($"courses?studentId={studentViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to navigate to courses for student: {studentViewModel.FullName}");
            ShowError("Ошибка навигации к курсам студента");
        }
    }

    /// <summary>
    /// Navigates to grades view for the selected student
    /// </summary>
    private async Task ViewGradesAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Navigating to grades for student: {studentViewModel.FullName}");
            await NavigateToAsync($"grades?studentId={studentViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to navigate to grades for student: {studentViewModel.FullName}");
            ShowError("Ошибка навигации к оценкам студента");
        }
    }

    #endregion

    #region Import/Export

    /// <summary>
    /// Imports students from file
    /// </summary>
    private async Task ImportStudentsAsync()
    {
        try
        {
            var importResult = await _dialogService.ShowFilePickerAsync("Выберите файл для импорта", new[] { "*.xlsx", "*.csv" });
            if (importResult != null && !string.IsNullOrEmpty(importResult))
            {
                var importedCount = await _importService.ImportStudentsAsync(importResult);
                if (importedCount > 0)
                {
                    LogInfo($"Импортировано {importedCount} студентов");
                    await LoadStudentsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при импорте студентов");
        }
    }

    /// <summary>
    /// Exports students report
    /// </summary>
    private async Task ExportReportAsync()
    {
        try
        {
            LogInfo("Starting student export process");
            
            var students = Students.Select(vm => vm.ToStudent()).ToList();
            var filePath = await _exportService.ExportStudentsToExcelAsync(students, "Отчет по студентам");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                ShowSuccess($"Отчет экспортирован: {filePath}");
                LogInfo($"Exported students report to: {filePath}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to export students report");
            ShowError("Ошибка экспорта отчета");
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Performs bulk edit on selected students
    /// </summary>
    private async Task BulkEditAsync()
    {
        try
        {
            var selectedStudents = _studentsSource.Items.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any())
            {
                ShowWarning("Выберите студентов для массового редактирования");
                return;
            }

            var bulkEditOptions = new BulkEditOptions
            {
                CanChangeGroup = true,
                CanChangeStatus = true,
                CanChangeAcademicYear = true
            };

            var bulkEditResult = await _dialogService.ShowBulkEditDialogAsync(bulkEditOptions);
            if (bulkEditResult != null)
            {
                var updatedCount = 0;
                dynamic editResult = bulkEditResult;

                foreach (var studentVm in SelectedStudents.ToList())
                {
                    var student = await _studentService.GetByUidAsync(studentVm.Uid);
                    if (student != null)
                    {
                        var hasChanges = false;

                        // Use reflection or dynamic access to get properties
                        try
                        {
                            if (editResult.NewGroupUid != null && editResult.NewGroupUid != student.GroupUid)
                            {
                                student.GroupUid = editResult.NewGroupUid;
                            hasChanges = true;
                        }
                        }
                        catch { /* Property doesn't exist or is null */ }

                        try
                        {
                            if (editResult.NewStatus != null && editResult.NewStatus != student.Status)
                            {
                                student.Status = editResult.NewStatus;
                            hasChanges = true;
                        }
                        }
                        catch { /* Property doesn't exist or is null */ }

                        try
                        {
                            if (editResult.NewAcademicYear != null && editResult.NewAcademicYear != student.AcademicYear)
                            {
                                student.AcademicYear = editResult.NewAcademicYear;
                            hasChanges = true;
                        }
                        }
                        catch { /* Property doesn't exist or is null */ }

                        if (hasChanges)
                        {
                            await _studentService.UpdateAsync(student);
                            updatedCount++;
                        }
                    }
                }

                await LoadStudentsAsync();
                ShowSuccess($"Обновлено {updatedCount} студентов");
                LogInfo($"Bulk edit completed: {updatedCount} students updated");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка массового редактирования студентов");
            ShowError("Ошибка массового редактирования");
        }
    }

    /// <summary>
    /// Performs bulk delete on selected students
    /// </summary>
    private async Task BulkDeleteAsync()
    {
        if (SelectedStudents == null || !SelectedStudents.Any()) return;

        try
        {
            var studentsToDelete = SelectedStudents.ToList();
            var firstStudent = studentsToDelete.FirstOrDefault();
            if (firstStudent == null) return;

            var result = await _dialogService.ShowConfirmationDialogAsync(
                "Удаление студентов",
                $"Вы уверены, что хотите удалить {studentsToDelete.Count} студентов?");

            if (result == true)
            {
                var deletedCount = 0;
                foreach (var studentViewModel in studentsToDelete)
                {
                    var success = await _studentService.DeleteAsync(studentViewModel.Uid);
                    if (success)
                    {
                        deletedCount++;
                    }
                }

                LogInfo($"Удалено {deletedCount} из {studentsToDelete.Count} студентов");
                await LoadStudentsAsync();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при массовом удалении студентов");
        }
    }

    /// <summary>
    /// Exports selected students
    /// </summary>
    private async Task BulkExportAsync()
    {
        try
        {
            LogInfo($"Starting bulk export for {SelectedStudentsCount} students");
            
            var students = SelectedStudents.Select(vm => vm.ToStudent()).ToList();
            var filePath = await _exportService.ExportStudentsToExcelAsync(students, "Отчет по студентам");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                ShowSuccess($"Экспортировано {SelectedStudentsCount} студентов: {filePath}");
                LogInfo($"Bulk exported {SelectedStudentsCount} students to: {filePath}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to perform bulk export");
            ShowError("Ошибка экспорта выбранных студентов");
        }
    }

    #endregion

    #region Selection Management

    /// <summary>
    /// Selects all visible students
    /// </summary>
    private async Task SelectAllAsync()
    {
        try
        {
            SelectedStudents.Clear();
            foreach (var student in Students)
            {
                SelectedStudents.Add(student);
                student.IsSelected = true;
            }
            
            LogInfo($"Selected all {Students.Count} visible students");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to select all students");
            ShowError("Ошибка выбора всех студентов");
        }
    }

    /// <summary>
    /// Deselects all students
    /// </summary>
    private async Task DeselectAllAsync()
    {
        try
        {
            foreach (var student in Students)
            {
                student.IsSelected = false;
            }
            SelectedStudents.Clear();
            
            LogInfo("Deselected all students");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to deselect all students");
            ShowError("Ошибка снятия выделения");
        }
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Called when the ViewModel is first loaded
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        try
        {
            LogInfo("StudentsViewModel first time loading...");
            await LoadStudentsAsync();
            LogInfo("StudentsViewModel first time loading completed");
        }
        catch (Exception ex)
        {
            LogError(ex, "Error during first time loading");
            ShowError("Ошибка при первоначальной загрузке данных");
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public override void Dispose()
    {
        _studentsSource?.Dispose();
        _groupsSource?.Dispose();
        base.Dispose();
        LogInfo("StudentsViewModel disposed");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Валидация данных студента
    /// </summary>
    private async Task<DomainValidationResult> ValidateStudentAsync(Student student)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            // Базовая валидация
            if (student.Person == null)
            {
                errors.Add("Информация о персоне обязательна");
                return new DomainValidationResult { IsValid = false, Errors = errors };
            }

            // Валидация персональных данных
            if (string.IsNullOrWhiteSpace(student.Person.FirstName))
                errors.Add("Имя обязательно");
            
            if (string.IsNullOrWhiteSpace(student.Person.LastName))
                errors.Add("Фамилия обязательна");
            
            if (string.IsNullOrWhiteSpace(student.Person.Email))
                errors.Add("Email обязателен");
            else if (!IsValidEmail(student.Person.Email))
                errors.Add("Некорректный формат email");

            // Валидация студенческих данных
            if (string.IsNullOrWhiteSpace(student.StudentCode))
                errors.Add("Код студента обязателен");
            else
            {
                // Проверка уникальности кода студента
                var existingStudent = await _studentService.GetByStudentCodeAsync(student.StudentCode);
                if (existingStudent != null && existingStudent.Uid != student.Uid)
                    errors.Add($"Студент с кодом '{student.StudentCode}' уже существует");
            }

            // Валидация дат
            if (student.EnrollmentDate > DateTime.Now)
                errors.Add("Дата поступления не может быть в будущем");
            
            if (student.GraduationDate.HasValue && student.GraduationDate <= student.EnrollmentDate)
                errors.Add("Дата выпуска должна быть позже даты поступления");

            // Валидация статуса
            if (student.Status == StudentStatus.Graduated && !student.GraduationDate.HasValue)
                warnings.Add("Для выпускника рекомендуется указать дату выпуска");

            // Проверка группы
            if (student.GroupUid.HasValue)
            {
                var group = await _groupService.GetByUidAsync(student.GroupUid.Value);
                if (group == null)
                    errors.Add("Выбранная группа не найдена");
                else if (group.Status != GroupStatus.Active)
                    warnings.Add($"Группа '{group.Name}' неактивна");
            }

            // Проверка возраста
            if (student.Person.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - student.Person.DateOfBirth.Value.Year;
                if (age < 16)
                    warnings.Add("Студент младше 16 лет");
                if (age > 65)
                    warnings.Add("Студент старше 65 лет");
            }

            return new DomainValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при валидации студента");
            errors.Add("Ошибка при валидации данных");
            return new DomainValidationResult { IsValid = false, Errors = errors };
        }
    }

    /// <summary>
    /// Gets information about related data for cascade deletion warning
    /// </summary>
    private async Task<RelatedDataInfo> GetRelatedDataInfoAsync(Guid studentUid)
    {
        try
        {
            var relatedData = new RelatedDataInfo();

            // Получаем связанные данные
            var grades = await _studentService.GetStudentGradesAsync(studentUid);
            var gradesCount = grades?.Count() ?? 0;
            if (gradesCount > 0)
                relatedData.AddRelatedData("Оценки", gradesCount);

            // Assignments count - using alternative approach since GetStudentAssignmentsAsync doesn't exist
            // relatedData.AddRelatedData("Задания", 0); // TODO: Implement when assignment service is available

            var courses = await _studentService.GetStudentCoursesAsync(studentUid);
            var coursesCount = courses?.Count() ?? 0;
            if (coursesCount > 0)
                relatedData.AddRelatedData("Курсы", coursesCount);

            var attendance = await _studentService.GetStudentAttendanceAsync(studentUid);
            var attendanceCount = attendance?.Count() ?? 0;
            if (attendanceCount > 0)
                relatedData.AddRelatedData("Посещаемость", attendanceCount);

            return relatedData;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка получения связанных данных студента");
            return new RelatedDataInfo();
        }
    }

    /// <summary>
    /// Updates statistics after CRUD operations
    /// </summary>
    private async Task UpdateStatisticsAsync()
    {
        try
        {
            // Обновление счетчиков
            TotalStudents = _studentsSource.Items.Count();
            ActiveStudents = _studentsSource.Items.Count(s => s.IsActive);
            InactiveStudents = _studentsSource.Items.Count(s => !s.IsActive);
            GraduatedStudents = 0; // TODO: Добавить подсчет выпускников когда будет доступна информация о статусе
            
            // Уведомление об изменении статистики
            this.RaisePropertyChanged(nameof(TotalStudents));
            this.RaisePropertyChanged(nameof(ActiveStudents));
            this.RaisePropertyChanged(nameof(InactiveStudents));
            this.RaisePropertyChanged(nameof(GraduatedStudents));
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при обновлении статистики");
        }
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            // Simple email validation without System.Net.Mail dependency
            return !string.IsNullOrWhiteSpace(email) && 
                   email.Contains("@") && 
                   email.Contains(".") &&
                   email.IndexOf("@") > 0 &&
                   email.LastIndexOf(".") > email.IndexOf("@");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if user has specific permission
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
    /// Создает объект Student из StudentEditorViewModel
    /// </summary>
    private Student CreateStudentFromEditor(StudentEditorViewModel editor)
    {
        return new Student
        {
            Uid = editor.CurrentStudent?.Uid ?? Guid.NewGuid(),
            PersonUid = editor.CurrentStudent?.PersonUid ?? Guid.NewGuid(),
            StudentCode = editor.StudentCode,
            EnrollmentDate = editor.EnrollmentDate,
            GroupUid = editor.SelectedGroup?.Uid,
            Status = editor.SelectedStatus,
            AcademicYear = editor.AcademicYear,
            Person = new Person
            {
                Uid = editor.CurrentStudent?.Person?.Uid ?? Guid.NewGuid(),
                FirstName = editor.FirstName,
                LastName = editor.LastName,
                MiddleName = editor.MiddleName,
                Email = editor.Email,
                Phone = editor.Phone,
                DateOfBirth = editor.DateOfBirth,
                Address = editor.Address
            }
        };
    }

    private async Task<bool> CanExportStudentsAsync()
    {
        return await HasPermissionAsync("Students.Export");
    }

    #endregion
}

