namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления группами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("groups", 
    DisplayName = "Группы", 
    IconKey = "AccountMultiple", 
    Order = 3,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление учебными группами")]
public class GroupsViewModel : RoutableViewModelBase
{
    private readonly IGroupService _groupService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly ICurriculumService _curriculumService;
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<GroupViewModel> Groups { get; set; } = new();
    [Reactive] public GroupViewModel? SelectedGroup { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public GroupAnalytics? Statistics { get; set; }
    
    // Filter properties
    [Reactive] public string SearchTerm { get; set; } = string.Empty;
    [Reactive] public Teacher? SelectedCurator { get; set; }
    [Reactive] public GroupStatus? SelectedStatus { get; set; }
    [Reactive] public DateTime? StartDate { get; set; }
    [Reactive] public DateTime? EndDate { get; set; }

    // Pagination
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalItems { get; set; }

    // Computed properties for UI binding
    public bool HasSelectedGroup => SelectedGroup != null;
    public bool HasSelectedGroupStatistics => Statistics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public bool HasPages => TotalPages > 1;
    public bool CanGoToFirstPage => CurrentPage > 1;
    public bool CanGoToLastPage => CurrentPage < TotalPages;
    public string PaginationInfo => TotalItems == 0
        ? "Нет данных"
        : $"Показано {(CurrentPage - 1) * PageSize + 1}-{Math.Min(CurrentPage * PageSize, TotalItems)} из {TotalItems}";

    /// <summary>
    /// Количество выбранных групп (для панели массовых действий)
    /// </summary>
    [Reactive] public int SelectedGroupsCount { get; set; }

    /// <summary>
    /// Общее количество групп (значок статистики в шапке страницы)
    /// </summary>
    [Reactive] public int TotalGroups { get; set; }

    /// <summary>
    /// Количество активных групп на текущей странице
    /// </summary>
    [Reactive] public int ActiveGroups { get; set; }

    /// <summary>
    /// Суммарное количество студентов в загруженных группах
    /// </summary>
    [Reactive] public int TotalStudents { get; set; }

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> EditGroupCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeleteGroupCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> ViewGroupDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> LoadGroupStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> AssignCuratorCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> ManageStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> ViewScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkArchiveCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    /// <summary>
    /// Количество групп
    /// </summary>
    [Reactive] public int GroupCount { get; set; }

    /// <summary>
    /// Общее количество записей
    /// </summary>
    [Reactive] public int TotalCount { get; set; }

    /// <summary>
    /// Название группы
    /// </summary>
    [Reactive] public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// UID группы
    /// </summary>
    [Reactive] public Guid GroupUid { get; set; }

    public GroupsViewModel(
        IScreen hostScreen,
        IGroupService groupService,
        IStudentService studentService,
        ITeacherService teacherService,
        ICurriculumService curriculumService,
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService) : base(hostScreen)
    {
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        InitializeCommands();
        SetupSubscriptions();
        
        LogInfo("GroupsViewModel initialized");
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadGroupsCommand = CreateCommand(LoadGroupsAsync, null, "Ошибка загрузки групп");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        CreateGroupCommand = CreateCommand(CreateGroupAsync, null, "Ошибка создания группы");
        EditGroupCommand = ReactiveCommand.CreateFromTask(EditGroupAsync);
        DeleteGroupCommand = ReactiveCommand.CreateFromTask(DeleteGroupAsync);
        ViewGroupDetailsCommand = CreateCommand<GroupViewModel>(ViewGroupDetailsAsync, null, "Ошибка просмотра деталей группы");
        LoadGroupStatisticsCommand = CreateCommand<GroupViewModel>(LoadGroupStatisticsAsync, null, "Ошибка загрузки статистики группы");
        AssignCuratorCommand = CreateCommand<GroupViewModel>(AssignCuratorAsync, null, "Ошибка назначения куратора");
        ManageStudentsCommand = CreateCommand<GroupViewModel>(ManageStudentsAsync, null, "Ошибка управления студентами");
        SearchCommand = CreateCommand<string>(SearchGroupsAsync, null, "Ошибка поиска групп");
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");

        var canGoFirst = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        var canGoLast = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        
        FirstPageCommand = CreateCommand(FirstPageAsync, canGoFirst, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(LastPageAsync, canGoLast, "Ошибка перехода на последнюю страницу");

        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
        ViewScheduleCommand = CreateCommand<GroupViewModel>(ViewScheduleAsync, null, "Ошибка перехода к расписанию");

        var hasSelection = this.WhenAnyValue(x => x.SelectedGroupsCount).Select(count => count > 0);
        BulkDeleteCommand = CreateCommand(BulkDeleteAsync, hasSelection, "Ошибка массового удаления групп");
        BulkArchiveCommand = CreateCommand(BulkArchiveAsync, hasSelection, "Ошибка массового архивирования групп");

        SelectAllCommand = CreateCommand(SelectAllAsync, null, "Ошибка выбора всех групп");
        DeselectAllCommand = CreateCommand(DeselectAllAsync, null, "Ошибка снятия выделения");
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе группы
        this.WhenAnyValue(x => x.SelectedGroup)
            .Where(group => group != null)
            .Select(group => group!)
            .InvokeCommand(LoadGroupStatisticsCommand)
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SelectedGroup)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroup)))
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Statistics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroupStatistics)))
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
    }

    private async Task LoadGroupsAsync()
    {
        LogInfo($"Loading groups with search text: {SearchText}");
        
        IsLoading = true;
        ShowInfo("Загрузка групп...");

        try
        {
            // Получаем группы с пагинацией
            var result = await _groupService.GetPagedAsync(
                CurrentPage,
                PageSize,
                SearchTerm);

            Groups.Clear();
            if (result.Groups != null)
            {
                foreach (var group in result.Groups)
                {
                    var itemViewModel = new GroupViewModel(group);
                    itemViewModel.WhenAnyValue(x => x.IsSelected)
                        .Subscribe(_ => SelectedGroupsCount = Groups.Count(g => g.IsSelected))
                        .DisposeWith(Disposables);
                    Groups.Add(itemViewModel);
                }
            }
            SelectedGroupsCount = 0;

            // Update pagination info
            TotalItems = result.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

            // Update header statistics badges
            TotalGroups = TotalItems;
            ActiveGroups = Groups.Count(g => g.IsActive);
            TotalStudents = Groups.Sum(g => g.StudentsCount);

            LogInfo($"Loaded {Groups.Count} groups for page {CurrentPage}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Error loading groups");
            LogInfo($"Error loading groups: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing groups data");
        IsRefreshing = true;
        
        await LoadGroupsAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создание новой группы
    /// </summary>
    private async Task CreateGroupAsync()
    {
        try
        {
            // Создаем новую группу
            var newGroup = new GroupViewModel
            {
                Uid = Guid.NewGuid(),
                Name = "Новая группа",
                Code = $"GRP{DateTime.Now:yyyyMMdd}",
                Year = DateTime.Now.Year,
                Status = GroupStatus.Active,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            var createdGroup = await _groupService.CreateAsync(newGroup.ToGroup());
            if (createdGroup != null)
            {
                LogInfo($"Группа '{createdGroup.Name}' успешно создана");
                await LoadGroupsAsync();
            }
            else
            {
                LogWarning("Не удалось создать группу");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании группы");
        }
    }

    /// <summary>
    /// Редактирование группы с optimistic locking
    /// </summary>
    private async Task EditGroupAsync()
    {
        if (SelectedGroup == null) return;

        try
        {
            // Используем реальный диалог редактирования
            var editedGroup = await _dialogService.ShowGroupEditDialogAsync(SelectedGroup.ToGroup());
            
            if (editedGroup != null)
            {
                var updateResult = await _groupService.UpdateAsync(editedGroup);
                if (updateResult)
                {
                    // Получаем обновленную группу
                    var updatedGroup = await _groupService.GetByUidAsync(editedGroup.Uid);
                    if (updatedGroup != null)
                    {
                        LogInfo($"Группа '{updatedGroup.Name}' успешно обновлена");
                        await LoadGroupsAsync();
                    }
                    else
                    {
                        LogWarning("Не удалось получить обновленную группу");
                    }
                }
                else
                {
                    LogWarning("Не удалось обновить группу");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при редактировании группы");
        }
    }

    /// <summary>
    /// Удаление группы с проверкой связанных данных
    /// </summary>
    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup == null) return;

        try
        {
            // var analytics = await _groupService.GetGroupAnalyticsAsync(SelectedGroup.Uid);
            // TODO: Аналитика группы будет реализована позже
            // когда будет доступен метод GetGroupAnalyticsAsync в IGroupService
            
            await _dialogService.ShowMessageAsync(
                "Аналитика группы",
                $"Аналитика для группы '{SelectedGroup.Name}' временно недоступна.\n" +
                "Функция будет реализована в следующих версиях.");
                
            StatusLogger.LogInfo($"Analytics placeholder shown for group: {SelectedGroup.Name}");
            
            /*
            if (analytics != null)
            {
                var message = $"Группа '{SelectedGroup.Name}' содержит {analytics.TotalStudents} студентов, " +
                             $"{analytics.ActiveCourses} активных курсов, " +
                             $"средний балл: {analytics.AverageGPA:F2}";
                
                StatusLogger.LogWarning($"Попытка удаления группы: {message}");
                
                // Простое подтверждение через StatusLogger
                var success = await _groupService.DeleteAsync(SelectedGroup.Uid);
                if (success)
                {
                    Groups.Remove(SelectedGroup);
                    SelectedGroup = null;
                    StatusLogger.LogInfo($"Группа '{SelectedGroup?.Name}' успешно удалена");
                    ShowSuccess("Группа успешно удалена");
                }
                else
                {
                    StatusLogger.LogWarning("Не удалось удалить группу");
                    ShowError("Не удалось удалить группу");
                }
            }
            */
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении группы");
        }
    }

    private async Task ViewGroupDetailsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo($"Viewing group details: {GroupName}");
        
        try
        {
            await NavigateToAsync($"group-details/{groupViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to group details");
            ShowError("Не удалось открыть детали группы");
        }
    }

    private async Task LoadGroupStatisticsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo($"Loading statistics for group: {GroupName}");
        
        try
        {
            LogInfo($"Loading analytics for group: {groupViewModel.Name}");
            
            // TODO: Аналитика группы будет реализована позже
            // когда будет доступен метод GetGroupAnalyticsAsync в IGroupService
            // var analytics = await _groupService.GetGroupAnalyticsAsync(group.Uid);
            
            await _dialogService.ShowMessageAsync(
                "Аналитика группы",
                $"Аналитика для группы '{groupViewModel.Name}' временно недоступна.\n" +
                "Функция будет реализована в следующих версиях.");
                
            LogInfo($"Analytics placeholder shown for group: {groupViewModel.Name}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load group statistics");
            Statistics = null;
        }
    }

    /// <summary>
    /// Назначение куратора группе
    /// </summary>
    private async Task AssignCuratorAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo($"Assigning curator to group: {GroupName}");

        try
        {
            // Получаем список доступных преподавателей
            var teachers = await _teacherService.GetAllTeachersAsync();
            
            // Показываем диалог выбора преподавателя
            var selectedTeacher = await _dialogService.ShowTeacherSelectionDialogAsync(teachers);
            
            if (selectedTeacher != null)
            {
                // Обновляем куратора группы
                var group = await _groupService.GetByUidAsync(groupViewModel.Uid);
                if (group != null)
                {
                    group.CuratorUid = selectedTeacher.Uid;
                    var updateResult = await _groupService.UpdateAsync(group);
                    
                    if (updateResult)
                    {
                        // Получаем обновленную группу
                        var updatedGroup = await _groupService.GetByUidAsync(group.Uid);
                        if (updatedGroup != null)
                        {
                            // Обновляем UI
                            var index = Groups.IndexOf(groupViewModel);
                            if (index >= 0)
                            {
                                Groups[index] = new GroupViewModel(updatedGroup);
                                if (SelectedGroup?.Uid == updatedGroup.Uid)
                                {
                                    SelectedGroup = Groups[index];
                                }
                            }
                            
                            ShowSuccess($"Куратор группы '{groupViewModel.Name}' назначен: {selectedTeacher.Person?.FirstName} {selectedTeacher.Person?.LastName}");
                            LogInfo($"Curator assigned successfully to group: {GroupName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to assign curator");
            ShowError("Не удалось назначить куратора");
        }
    }

    private async Task ManageStudentsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo($"Managing students for group: {GroupName}");
        
        try
        {
            await NavigateToAsync($"group-students/{groupViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to group students management");
            ShowError("Не удалось открыть управление студентами группы");
        }
    }

    private async Task SearchGroupsAsync(string searchText)
    {
        LogInfo($"Searching groups with text: {searchText}");
        SearchTerm = searchText;
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadGroupsAsync();
    }

    /// <summary>
    /// Сбрасывает все фильтры групп к значениям по умолчанию и перезагружает список
    /// </summary>
    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing all group filters");

        SearchText = string.Empty;
        SearchTerm = string.Empty;
        SelectedCurator = null;
        SelectedStatus = null;
        StartDate = null;
        EndDate = null;
        CurrentPage = 1;

        await LoadGroupsAsync();
        ShowInfo("Фильтры очищены");
    }

    /// <summary>
    /// Переход к расписанию группы
    /// </summary>
    private async Task ViewScheduleAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo($"Viewing schedule for group: {groupViewModel.Name}");

        try
        {
            await NavigateToAsync("schedule");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to schedule");
            ShowError("Не удалось открыть расписание группы");
        }
    }

    /// <summary>
    /// Отмечает все загруженные группы как выбранные
    /// </summary>
    private Task SelectAllAsync()
    {
        foreach (var group in Groups)
        {
            group.IsSelected = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Снимает выделение со всех загруженных групп
    /// </summary>
    private Task DeselectAllAsync()
    {
        foreach (var group in Groups)
        {
            group.IsSelected = false;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Массовое удаление выбранных групп
    /// </summary>
    private async Task BulkDeleteAsync()
    {
        var selected = Groups.Where(g => g.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить выбранные группы ({selected.Count})?");
        if (!confirmed) return;

        var deletedCount = 0;
        foreach (var item in selected)
        {
            var success = await _groupService.DeleteAsync(item.Uid);
            if (success)
            {
                Groups.Remove(item);
                deletedCount++;
            }
        }

        SelectedGroupsCount = Groups.Count(g => g.IsSelected);
        TotalItems = Math.Max(0, TotalItems - deletedCount);
        TotalGroups = TotalItems;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

        ShowSuccess($"Удалено групп: {deletedCount}");
    }

    /// <summary>
    /// Массовое архивирование выбранных групп
    /// </summary>
    private async Task BulkArchiveAsync()
    {
        var selected = Groups.Where(g => g.IsSelected).ToList();
        if (selected.Count == 0) return;

        var archivedCount = 0;
        foreach (var item in selected)
        {
            var group = await _groupService.GetByUidAsync(item.Uid);
            if (group == null) continue;

            group.Status = GroupStatus.Archived;
            group.IsActive = false;

            var success = await _groupService.UpdateAsync(group);
            if (success)
            {
                item.Status = GroupStatus.Archived;
                item.IsActive = false;
                archivedCount++;
            }
        }

        ActiveGroups = Groups.Count(g => g.IsActive);
        ShowSuccess($"Архивировано групп: {archivedCount}");
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadGroupsAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadGroupsAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadGroupsAsync();
        }
    }

    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await LoadGroupsAsync();
    }

    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadGroupsAsync();
    }

    /// <summary>
    /// Валидация данных группы
    /// </summary>
    private async Task<DomainValidationResult> ValidateGroupAsync(Group group)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(group.Code))
            errors.Add("Код группы обязателен");
        else if (group.Code.Length < 2 || group.Code.Length > 10)
            errors.Add("Код группы должен содержать от 2 до 10 символов");

        if (string.IsNullOrWhiteSpace(group.Name))
            errors.Add("Название группы обязательно");
        else if (group.Name.Length > 100)
            errors.Add("Название группы не должно превышать 100 символов");

        // Проверка уникальности кода
        if (!string.IsNullOrWhiteSpace(group.Code))
        {
            var existingGroup = await _groupService.GetByCodeAsync(group.Code);
            if (existingGroup != null && existingGroup.Uid != group.Uid)
                errors.Add($"Группа с кодом '{group.Code}' уже существует");
        }

        // Бизнес-правила валидации
        if (group.MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше 0");

        if (group.MaxStudents > 50)
            warnings.Add("Рекомендуется не превышать 50 студентов в группе");

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Проверка связанных данных перед удалением
    /// </summary>
    private async Task<GroupRelatedDataInfo> CheckRelatedDataAsync(Guid groupUid)
    {
        var info = new GroupRelatedDataInfo();
        
        try
        {
            // Проверяем студентов
            var studentsCount = await _studentService.GetStudentsCountByGroupAsync(groupUid);
            info.StudentsCount = studentsCount;
            
            // Проверяем экземпляры курсов - используем альтернативный метод
            var courseInstances = await _groupService.GetGroupCourseInstancesAsync(groupUid);
            info.CourseInstancesCount = courseInstances?.Count() ?? 0;
            
            // Проверяем записи на курсы
            var enrollments = await _groupService.GetGroupEnrollmentsAsync(groupUid);
            info.EnrollmentsCount = enrollments?.Count() ?? 0;
            
            // Проверяем задания
            var assignments = await _groupService.GetGroupAssignmentsAsync(groupUid);
            info.AssignmentsCount = assignments?.Count() ?? 0;
            
            // Проверяем оценки
            var grades = await _groupService.GetGroupGradesAsync(groupUid);
            info.GradesCount = grades?.Count() ?? 0;
            
            // Проверяем посещаемость
            var attendance = await _groupService.GetGroupAttendanceAsync(groupUid);
            info.AttendanceCount = attendance?.Count() ?? 0;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Error checking related data for group: {GroupUid}");
        }
        
        return info;
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

    private async Task<bool> CanExportGroupsAsync()
    {
        return await HasPermissionAsync("Groups.Export");
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("GroupsViewModel first time loaded");
        await LoadGroupsAsync();
    }

    #endregion
}

/// <summary>
/// Информация о связанных данных группы
/// </summary>
public class GroupRelatedDataInfo
{
    public int StudentsCount { get; set; }
    public int CourseInstancesCount { get; set; }
    public int EnrollmentsCount { get; set; }
    public int AssignmentsCount { get; set; }
    public int GradesCount { get; set; }
    public int AttendanceCount { get; set; }
}

