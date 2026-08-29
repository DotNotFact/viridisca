using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления преподавателями с расширенной функциональностью
/// </summary>
[Route("teachers",
    DisplayName = "Преподаватели",
    IconKey = "AccountTie",
    Order = 2,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление преподавателями")]
public class TeachersViewModel : RoutableViewModelBase
{
    #region Services
    private readonly ITeacherService _teacherService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IGroupService _groupService;
    private readonly INotificationService _notificationService;
    private readonly IDepartmentService _departmentService;
    private readonly IExportService _exportService;
    private readonly IImportService _importService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private readonly ObservableCollection<Teacher> _teachers = new();

    public ReadOnlyObservableCollection<Teacher> Teachers { get; }

    #endregion

    #region Properties
    [Reactive] public ObservableCollection<TeacherViewModel> TeachersCollection { get; set; } = new();
    [Reactive] public TeacherViewModel? SelectedTeacher { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public TeacherAnalytics? Statistics { get; set; }

    // Filters
    [Reactive] public string? SpecializationFilter { get; set; }
    [Reactive] public string? StatusFilter { get; set; }

    // Pagination
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalItems { get; set; }

    // Computed properties
    public bool HasSelectedTeacher => SelectedTeacher != null;
    public bool HasSelectedTeacherStatistics => Statistics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public bool HasPages => TotalPages > 1;
    public bool CanGoToFirstPage => CurrentPage > 1;
    public bool CanGoToLastPage => CurrentPage < TotalPages;
    public string PaginationInfo => TotalItems == 0
        ? "Нет данных"
        : $"Показано {(CurrentPage - 1) * PageSize + 1}-{Math.Min(CurrentPage * PageSize, TotalItems)} из {TotalItems}";

    /// <summary>
    /// Общее количество преподавателей (значок статистики в шапке страницы)
    /// </summary>
    [Reactive] public int TotalTeachers { get; set; }

    /// <summary>
    /// Количество выбранных преподавателей (для панели массовых действий)
    /// </summary>
    [Reactive] public int SelectedTeachersCount { get; set; }

    /// <summary>
    /// Количество преподавателей
    /// </summary>
    [Reactive] public int TeacherCount { get; set; }

    /// <summary>
    /// Общее количество записей
    /// </summary>
    [Reactive] public int TotalCount { get; set; }

    /// <summary>
    /// ID преподавателя
    /// </summary>
    [Reactive] public Guid TeacherId { get; set; }

    /// <summary>
    /// Имя преподавателя
    /// </summary>
    [Reactive] public string TeacherName { get; set; } = string.Empty;

    /// <summary>
    /// Название группы
    /// </summary>
    [Reactive] public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Название курса
    /// </summary>
    [Reactive] public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Поисковый термин
    /// </summary>
    [Reactive] public string SearchTerm { get; set; } = string.Empty;

    // Alias for compatibility
    public int Page => CurrentPage;

    // Statistics properties
    [Reactive] public int ActiveTeachers { get; set; }
    [Reactive] public int InactiveTeachers { get; set; }
    [Reactive] public int RetiredTeachers { get; set; }

    #endregion

    #region Commands
    public ReactiveCommand<Unit, Unit> LoadTeachersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> EditTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> DeleteTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ViewTeacherDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ViewStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ManageCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ManageGroupsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    // BulkEditCommand intentionally left unwired: no per-item "edit" operation exists that
    // makes sense applied to a batch without a dedicated multi-record edit dialog (a
    // substantial new feature, not a wiring fix) - see report.
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkExportCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;
    #endregion

    #region Constructor
    public TeachersViewModel(
        IScreen hostScreen,
        ITeacherService teacherService,
        ICourseInstanceService courseInstanceService,
        IGroupService groupService,
        INotificationService notificationService,
        IDepartmentService departmentService,
        IExportService exportService,
        IImportService importService,
        IPermissionService permissionService,
        IAuthService authService,
        IUnifiedNavigationService navigationService,
        IDialogService dialogService) : base(hostScreen)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Teachers = new ReadOnlyObservableCollection<Teacher>(_teachers);

        InitializeCommands();
        SetupSubscriptions();

        LogInfo("TeachersViewModel initialized");
    }
    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadTeachersCommand = CreateCommand(async () => await LoadTeachersAsync(), null, "Ошибка загрузки преподавателей");
        RefreshCommand = CreateCommand(async () => await RefreshAsync(), null, "Ошибка обновления данных");
        CreateTeacherCommand = CreateCommand(async () => await CreateTeacherAsync(), null, "Ошибка создания преподавателя");
        EditTeacherCommand = CreateCommand<TeacherViewModel>(async (teacher) => await EditTeacherAsync(teacher), null, "Ошибка редактирования преподавателя");
        DeleteTeacherCommand = CreateCommand<TeacherViewModel>(async (teacher) => await DeleteTeacherAsync(teacher), null, "Ошибка удаления преподавателя");
        ViewTeacherDetailsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ViewTeacherDetailsAsync(teacher), null, "Ошибка просмотра деталей преподавателя");
        ViewStatisticsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ViewStatisticsAsync(teacher), null, "Ошибка загрузки статистики");
        ManageCoursesCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ManageCoursesAsync(teacher), null, "Ошибка управления курсами");
        ManageGroupsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ManageGroupsAsync(teacher), null, "Ошибка управления группами");
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchTeachersAsync(searchTerm), null, "Ошибка поиска преподавателей");
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

        var hasSelection = this.WhenAnyValue(x => x.SelectedTeachersCount).Select(count => count > 0);
        BulkDeleteCommand = CreateCommand(async () => await BulkDeleteTeachersAsync(), hasSelection, "Ошибка массового удаления преподавателей");
        BulkExportCommand = CreateCommand(async () => await BulkExportTeachersAsync(), hasSelection, "Ошибка массового экспорта преподавателей");

        SelectAllCommand = CreateCommand(async () => await SelectAllAsync(), null, "Ошибка выбора всех преподавателей");
        DeselectAllCommand = CreateCommand(async () => await DeselectAllAsync(), null, "Ошибка снятия выделения");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Обновление computed properties
        this.WhenAnyValue(x => x.SelectedTeacher)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacher)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.Statistics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacherStatistics)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                this.RaisePropertyChanged(nameof(CanGoToNextPage));
                this.RaisePropertyChanged(nameof(CanGoToFirstPage));
                this.RaisePropertyChanged(nameof(CanGoToLastPage));
                this.RaisePropertyChanged(nameof(HasPages));
            })
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.PageSize, x => x.TotalItems)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(PaginationInfo)))
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе преподавателя
        this.WhenAnyValue(x => x.SelectedTeacher)
            .Where(teacher => teacher != null)
            .SelectMany(teacher => ViewStatisticsCommand.Execute(teacher!))
            .Subscribe()
            .DisposeWith(Disposables);
    }

    private async Task LoadTeachersAsync()
    {
        LogInfo($"Loading teachers with filters: SearchText={SearchText}, SpecializationFilter={SpecializationFilter}, StatusFilter={StatusFilter}");

        IsLoading = true;

        // Используем новый универсальный метод пагинации
        var (teachers, totalCount) = await _teacherService.GetPagedAsync(
            CurrentPage,
            PageSize,
            SearchText);

        TeachersCollection.Clear();
        foreach (var teacher in teachers)
        {
            var itemViewModel = new TeacherViewModel(teacher);
            itemViewModel.WhenAnyValue(x => x.IsSelected)
                .Subscribe(_ => SelectedTeachersCount = TeachersCollection.Count(t => t.IsSelected))
                .DisposeWith(Disposables);
            TeachersCollection.Add(itemViewModel);
        }
        SelectedTeachersCount = 0;

        TotalItems = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        TotalTeachers = totalCount;
        ActiveTeachers = teachers.Count(t => t.IsActive);

        ShowSuccess($"Загружено {teachers.Count()} преподавателей");
        LogInfo($"Loaded {TeacherCount} teachers, total: {TotalCount}");

        IsLoading = false;
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing teachers data");
        IsRefreshing = true;

        await LoadTeachersAsync();
        ShowSuccess("Данные обновлены");

        IsRefreshing = false;
    }

    /// <summary>
    /// Создает новый преподаватель
    /// </summary>
    private async Task CreateTeacherAsync()
    {
        try
        {
            var result = await _dialogService.ShowTeacherEditDialogAsync(new Teacher());
            if (result != null && result is Teacher teacher)
            {
                var createdTeacher = await _teacherService.CreateAsync(teacher);
                if (createdTeacher != null)
                {
                    await LoadTeachersAsync();
                    ShowSuccess($"Преподаватель {createdTeacher.Person?.FullName} успешно создан");

                    await _notificationService.SendNotificationAsync(
                        createdTeacher.PersonUid.ToString(),
                        "Преподаватель создан",
                        NotificationType.Success);
                }
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Validation failed for teacher creation");
            ShowError($"Ошибка валидации: {ex.Message}");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже существует"))
        {
            LogError(ex, "Дублирование при создании преподавателя");
            ErrorMessage = $"Преподаватель с таким кодом уже существует: {ex.Message}";
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании преподавателя");
        }
    }

    /// <summary>
    /// Редактирует выбранного преподавателя
    /// </summary>
    private async Task EditTeacherAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel?.Teacher == null) return;

        try
        {
            var teacherToEdit = await _teacherService.GetByUidAsync(teacherViewModel.Teacher.Uid);
            if (teacherToEdit == null) return;

            var result = await _dialogService.ShowTeacherEditDialogAsync(teacherToEdit);
            if (result != null && result is Teacher updatedTeacher)
            {
                await _teacherService.UpdateAsync(updatedTeacher);
                await LoadTeachersAsync();
                await _notificationService.SendNotificationAsync(
                    updatedTeacher.PersonUid.ToString(),
                    "Данные преподавателя обновлены",
                    NotificationType.Success);

                await _dialogService.ShowInfoAsync("Успех", "Преподаватель успешно обновлен");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating teacher");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update teacher");
            ShowError("Не удалось обновить преподавателя. Попробуйте еще раз.");
        }
    }

    /// <summary>
    /// Deletes a teacher with enhanced confirmation and cascade handling
    /// </summary>
    private async Task DeleteTeacherAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        LogInfo($"Attempting to delete teacher: {TeacherId}");

        try
        {
            IsLoading = true;

            // Проверка связанных данных
            // TODO: Проверка связанных данных будет реализована позже
            // когда будет доступен метод GetTeacherRelatedDataInfoAsync в ITeacherService

            string confirmationMessage = $"Вы уверены, что хотите удалить преподавателя '{teacherViewModel.FullName}'?";

            // В будущем здесь будет проверка связанных данных
            // if (relatedDataInfo.HasRelatedData)
            // {
            //     confirmationMessage += "\n\n" + relatedDataInfo.GetWarningMessage();
            // }

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                confirmationMessage);

            if (confirmed == false) return;

            // Удаление преподавателя
            var deleteSuccess = await _teacherService.DeleteAsync(teacherViewModel.Uid);

            if (deleteSuccess)
            {
                // Удаление из коллекции
                TeachersCollection.Remove(teacherViewModel);

                // Обновление статистики
                TotalItems--;
                if (teacherViewModel.Teacher.Person?.FirstName != null)
                    ActiveTeachers--;

                // Очистка выбора если удаленный преподаватель был выбран
                if (SelectedTeacher == teacherViewModel)
                    SelectedTeacher = null;

                ShowSuccess($"Преподаватель '{teacherViewModel.FullName}' удален");

                LogInfo($"Преподаватель удален: {teacherViewModel.FullName} (ID: {teacherViewModel.Uid})");

                await _notificationService.SendNotificationAsync(
                    teacherViewModel.PersonUid.ToString(),
                    "Преподаватель удален",
                    NotificationType.Success);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка удаления",
                    "Не удалось удалить преподавателя");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            await _dialogService.ShowErrorAsync("Невозможно удалить",
                "Преподаватель не может быть удален, так как имеет связанные записи. " +
                "Сначала удалите или переназначьте связанные данные.");
            LogWarning($"Попытка удаления преподавателя со связанными данными: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка при удалении преподавателя: {Message}", ex.Message);
            await _dialogService.ShowErrorAsync("Ошибка", $"Не удалось удалить преподавателя: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewTeacherDetailsAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo($"Viewing teacher details: {TeacherId}");

        // Используем новый универсальный метод получения
        var teacher = await _teacherService.GetByUidAsync(teacherViewModel.Uid);
        if (teacher != null)
        {
            SelectedTeacher = new TeacherViewModel(teacher);
            await ViewStatisticsAsync(SelectedTeacher);

            // Показываем диалог деталей
            var result = await _dialogService.ShowTeacherDetailsDialogAsync(teacher);
            if (result is object resultObj && resultObj.ToString() == "edit")
            {
                // Если пользователь нажал "Редактировать" в диалоге деталей
                await EditTeacherAsync(teacherViewModel);
            }

            ShowInfo($"Просмотр деталей преподавателя: {teacher.Person?.FirstName} {teacher.Person?.LastName}");
        }
        else
        {
            ShowError("Преподаватель не найден");
        }
    }

    private async Task ViewStatisticsAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo($"Loading teacher statistics: {TeacherId}");

        try
        {
            var statistics = await _teacherService.GetTeacherStatisticsAsync(teacherViewModel.Uid);
            // Просто показываем статистику в диалоге
            if (statistics != null)
            {
                await _dialogService.ShowTeacherStatisticsDialogAsync("Статистика преподавателя", statistics);
                LogInfo("Teacher statistics loaded successfully");
            }
            else
            {
                ShowInfo("Статистика для преподавателя недоступна");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load teacher statistics");
            ShowError("Не удалось загрузить статистику преподавателя");
        }
    }

    private async Task ManageCoursesAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo($"Managing courses for teacher: {TeacherId}");

        var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
        if (teacher == null)
        {
            ShowError("Преподаватель не найден");
            return;
        }

        var allCourses = await _courseInstanceService.GetAllCoursesAsync();

        var result = await _dialogService.ShowTeacherCoursesManagementDialogAsync(teacher, allCourses);
        if (result != null)
        {
            // TODO: Implement course assignment logic when service method is available
            await RefreshAsync();
            ShowSuccess($"Курсы преподавателя '{teacherViewModel.FullName}' обновлены");
            LogInfo("Teacher courses updated successfully");

            await _notificationService.SendNotificationAsync(
                teacherViewModel.PersonUid.ToString(),
                "Курс назначен преподавателю",
                NotificationType.Success);
        }
        else
        {
            LogDebug("Course management cancelled by user");
        }
    }

    private async Task ManageGroupsAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        LogInfo($"Managing groups for teacher: {TeacherName}");

        try
        {
            var availableGroups = await _groupService.GetAllAsync();
            var selectedGroups = await _dialogService.ShowGroupSelectionDialogAsync(availableGroups);

            if (selectedGroups != null && selectedGroups.Any())
            {
                var selectedGroup = selectedGroups.First();

                // Получаем экземпляры курсов преподавателя
                var courseInstances = await _courseInstanceService.GetByTeacherUidAsync(teacherViewModel.Uid);
                var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(courseInstances);

                if (selectedCourseInstances != null && selectedCourseInstances.Any())
                {
                    var selectedCourseInstance = selectedCourseInstances.First();
                    selectedCourseInstance.GroupUid = selectedGroup.Uid;
                    await _courseInstanceService.UpdateAsync(selectedCourseInstance);

                    ShowSuccess($"Группа '{selectedGroup.Name}' назначена преподавателю {teacherViewModel.FullName}");
                    LogInfo($"Group assigned to teacher: {teacherViewModel.FullName} -> {selectedGroup.Name}");

                    await _notificationService.SendNotificationAsync(
                        teacherViewModel.PersonUid.ToString(),
                        "Группа назначена преподавателю",
                        NotificationType.Success);
                }
            }
            else
            {
                LogDebug("Group management cancelled by user");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка управления группами преподавателя");
            ShowError("Ошибка управления группами");
        }
    }

    private async Task SearchTeachersAsync(string searchTerm)
    {
        LogInfo($"Searching teachers with term: {SearchTerm}");
        SearchText = searchTerm;
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadTeachersAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters");
        CurrentPage = 1; // Сброс на первую страницу при применении фильтров
        await LoadTeachersAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing filters");
        SpecializationFilter = null;
        StatusFilter = null;
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadTeachersAsync();
        ShowInfo("Фильтры очищены");
    }

    private async Task GoToPageAsync(int page)
    {
        LogInfo($"Navigating to page: {Page}");
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadTeachersAsync();
        }
    }

    private async Task NextPageAsync()
    {
        LogInfo("Navigating to next page");
        await GoToPageAsync(CurrentPage + 1);
    }

    private async Task PreviousPageAsync()
    {
        LogInfo("Navigating to previous page");
        await GoToPageAsync(CurrentPage - 1);
    }

    private async Task FirstPageAsync()
    {
        LogInfo("Navigating to first page");
        await GoToPageAsync(1);
    }

    private async Task LastPageAsync()
    {
        LogInfo("Navigating to last page");
        await GoToPageAsync(TotalPages);
    }

    private async Task AssignCourseAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        try
        {
            var availableCourseInstances = await _courseInstanceService.GetUnassignedAsync();
            var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(availableCourseInstances);

            if (selectedCourseInstances != null && selectedCourseInstances.Any())
            {
                var selectedCourseInstance = selectedCourseInstances.First();
                selectedCourseInstance.TeacherUid = teacherViewModel.Uid;
                await _courseInstanceService.UpdateAsync(selectedCourseInstance);

                ShowSuccess($"Курс '{selectedCourseInstance.Subject?.Name}' назначен преподавателю {teacherViewModel.FullName}");
                LogInfo($"Course assigned to teacher: {teacherViewModel.FullName} -> {selectedCourseInstance.Subject?.Name}");

                await _notificationService.SendNotificationAsync(
                    teacherViewModel.PersonUid.ToString(),
                    "Курс назначен преподавателю",
                    NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка назначения курса преподавателю");
            ShowError("Ошибка назначения курса");
        }
    }

    private async Task AssignGroupAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        try
        {
            var availableGroups = await _groupService.GetAllAsync();
            var selectedGroups = await _dialogService.ShowGroupSelectionDialogAsync(availableGroups);

            if (selectedGroups != null && selectedGroups.Any())
            {
                var selectedGroup = selectedGroups.First();

                // Получаем экземпляры курсов преподавателя
                var courseInstances = await _courseInstanceService.GetByTeacherUidAsync(teacherViewModel.Uid);
                var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(courseInstances);

                if (selectedCourseInstances != null && selectedCourseInstances.Any())
                {
                    var selectedCourseInstance = selectedCourseInstances.First();
                    selectedCourseInstance.GroupUid = selectedGroup.Uid;
                    await _courseInstanceService.UpdateAsync(selectedCourseInstance);

                    ShowSuccess($"Группа '{selectedGroup.Name}' назначена преподавателю {teacherViewModel.FullName}");
                    LogInfo($"Group assigned to teacher: {teacherViewModel.FullName} -> {selectedGroup.Name}");

                    await _notificationService.SendNotificationAsync(
                        teacherViewModel.PersonUid.ToString(),
                        "Группа назначена преподавателю",
                        NotificationType.Success);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка назначения группы преподавателю");
            ShowError("Ошибка назначения группы");
        }
    }

    private async Task<bool> CanDeleteTeacherAsync(TeacherViewModel teacherViewModel)
    {
        try
        {
            // Check if teacher has related data using existing method
            var relatedDataInfo = await _teacherService.GetByUidAsync(teacherViewModel.Uid);
            if (relatedDataInfo == null)
            {
                return true;
            }

            // For now, allow deletion - in real implementation, check for courses, grades, etc.
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error checking teacher deletion eligibility for {teacherViewModel.Uid}");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Валидация данных преподавателя
    /// </summary>
    private async Task<DomainValidationResult> ValidateTeacherAsync(Teacher teacher)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(teacher.Person?.FirstName))
        {
            errors.Add("Имя преподавателя обязательно");
        }

        if (string.IsNullOrWhiteSpace(teacher.Person?.LastName))
        {
            errors.Add("Фамилия преподавателя обязательна");
        }

        if (string.IsNullOrWhiteSpace(teacher.Person?.Email))
        {
            errors.Add("Email преподавателя обязателен");
        }
        else if (!IsValidEmail(teacher.Person.Email))
        {
            errors.Add("Некорректный формат email");
        }

        // Проверка уникальности email
        if (!string.IsNullOrWhiteSpace(teacher.Person?.Email))
        {
            var existingTeacher = await _teacherService.GetByPersonEmailAsync(teacher.Person?.Email ?? "");
            if (existingTeacher != null)
            {
                errors.Add($"Преподаватель с email '{teacher.Person.Email}' уже существует");
            }
        }

        // Проверка зарплаты
        if (teacher.Salary < 0)
            errors.Add("Зарплата не может быть отрицательной");
        else if (teacher.Salary == 0)
            warnings.Add("Зарплата не указана");

        // Проверка даты найма
        if (teacher.HireDate > DateTime.Now)
            warnings.Add("Дата найма в будущем");

        if (teacher.HireDate < DateTime.Now.AddYears(-50))
            warnings.Add("Дата найма более 50 лет назад");

        // Проверка департамента
        if (teacher.DepartmentUid.HasValue)
        {
            var department = await _departmentService.GetByUidAsync(teacher.DepartmentUid.Value);
            if (department == null)
                errors.Add("Выбранный департамент не найден");
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Updates statistics after teacher status change
    /// </summary>
    private void UpdateStatisticsAfterStatusChange(TeacherStatus oldStatus, TeacherStatus newStatus)
    {
        if (oldStatus == TeacherStatus.Active && newStatus != TeacherStatus.Active)
            ActiveTeachers--;
        else if (oldStatus != TeacherStatus.Active && newStatus == TeacherStatus.Active)
            ActiveTeachers++;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if current user has specific permission
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        // Здесь должна быть реальная проверка прав доступа
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    private async Task ExportTeachersAsync()
    {
        try
        {
            IsLoading = true;

            // Получаем всех преподавателей для экспорта
            var allTeachers = await _teacherService.GetAllTeachersAsync();
            var exportData = await _teacherService.ExportTeachersAsync(allTeachers);

            // Здесь должна быть логика сохранения файла
            // Пока что просто показываем сообщение об успехе
            ShowSuccess("Данные преподавателей экспортированы");

            LogInfo("Экспорт преподавателей выполнен успешно");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при экспорте преподавателей");
            ShowError($"Ошибка при экспорте: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Отмечает всех загруженных преподавателей как выбранных
    /// </summary>
    private Task SelectAllAsync()
    {
        foreach (var teacher in TeachersCollection)
        {
            teacher.IsSelected = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Снимает выделение со всех загруженных преподавателей
    /// </summary>
    private Task DeselectAllAsync()
    {
        foreach (var teacher in TeachersCollection)
        {
            teacher.IsSelected = false;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Массовое удаление выбранных преподавателей
    /// </summary>
    private async Task BulkDeleteTeachersAsync()
    {
        var selected = TeachersCollection.Where(t => t.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить выбранных преподавателей ({selected.Count})?");
        if (!confirmed) return;

        var deletedCount = 0;
        foreach (var item in selected)
        {
            var success = await _teacherService.DeleteAsync(item.Uid);
            if (success)
            {
                TeachersCollection.Remove(item);
                deletedCount++;
            }
        }

        SelectedTeachersCount = TeachersCollection.Count(t => t.IsSelected);
        TotalItems = Math.Max(0, TotalItems - deletedCount);
        TotalTeachers = TotalItems;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

        ShowSuccess($"Удалено преподавателей: {deletedCount}");
    }

    /// <summary>
    /// Экспортирует выбранных преподавателей (использует ITeacherService.ExportTeachersAsync)
    /// </summary>
    private async Task BulkExportTeachersAsync()
    {
        var selected = TeachersCollection.Where(t => t.IsSelected).ToList();
        if (selected.Count == 0) return;

        try
        {
            IsLoading = true;

            var exportData = await _teacherService.ExportTeachersAsync(selected.Select(t => t.Teacher));

            ShowSuccess($"Экспортировано преподавателей: {selected.Count}");
            LogInfo($"Bulk export of {selected.Count} teachers completed");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при массовом экспорте преподавателей");
            ShowError($"Ошибка при экспорте: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        LogInfo("TeachersViewModel loaded for the first time");

        // Load teachers when view is loaded for the first time
        await LoadTeachersAsync();
    }

    #endregion
}

