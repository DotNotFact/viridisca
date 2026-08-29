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
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.System.Enums;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Navigations;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Domain.Services;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления расписанием
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("schedule", 
    DisplayName = "Расписание", 
    IconKey = "CalendarClock", 
    Order = 9,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление расписанием занятий")]
public class ScheduleViewModel : RoutableViewModelBase
{
    private readonly IScheduleSlotService _scheduleSlotService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAcademicPeriodService _academicPeriodService;
    private readonly ITeacherService _teacherService;
    private readonly IGroupService _groupService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly ISubjectService _subjectService;
    private readonly IDialogService _dialogService;

    private readonly SourceList<ScheduleSlotViewModel> _scheduleSlotsSource = new();
    private readonly ReadOnlyObservableCollection<ScheduleSlotViewModel> _scheduleSlots;

    // === СВОЙСТВА ===
    
    [Reactive] public ScheduleSlotViewModel? SelectedSlot { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public int TotalSlots { get; set; }
    [Reactive] public DateTime SelectedDate { get; set; } = DateTime.Today;
    [Reactive] public DayOfWeek SelectedDayOfWeek { get; set; } = DateTime.Today.DayOfWeek;

    // Фильтры
    [Reactive] public Guid? SelectedTeacherUid { get; set; }
    [Reactive] public Guid? SelectedGroupUid { get; set; }
    [Reactive] public Guid? SelectedPeriodUid { get; set; }
    [Reactive] public string? SelectedRoom { get; set; }

    // Фильтры, привязанные напрямую к элементам ComboBox во View
    [Reactive] public Teacher? SelectedTeacherFilter { get; set; }
    [Reactive] public Group? SelectedGroupFilter { get; set; }
    [Reactive] public AcademicPeriod? SelectedAcademicPeriodFilter { get; set; }
    [Reactive] public string? SelectedRoomFilter { get; set; }

    // Режим отображения (день/неделя/месяц)
    [Reactive] public bool IsDayView { get; set; } = true;
    [Reactive] public bool IsWeekView { get; set; }
    [Reactive] public bool IsMonthView { get; set; }

    public extern string CurrentPeriodDisplay { [ObservableAsProperty] get; }

    // Коллекции для фильтров
    public ObservableCollection<Teacher> AvailableTeachers { get; } = new();
    public ObservableCollection<Group> AvailableGroups { get; } = new();
    public ObservableCollection<AcademicPeriod> AvailablePeriods { get; } = new();
    public ObservableCollection<AcademicPeriod> AvailableAcademicPeriods => AvailablePeriods;
    public ObservableCollection<string> AvailableRooms { get; } = new();

    public ReadOnlyObservableCollection<ScheduleSlotViewModel> ScheduleSlots => _scheduleSlots;

    // === КОМАНДЫ ===

    public ReactiveCommand<Unit, Unit> LoadScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> AddSlotCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateScheduleSlotCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> AutoGenerateScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<ScheduleSlotViewModel, Unit> EditSlotCommand { get; private set; } = null!;
    public ReactiveCommand<ScheduleSlotViewModel, Unit> DeleteSlotCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SetDayViewCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SetWeekViewCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SetMonthViewCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPeriodCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPeriodCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoToTodayCommand { get; private set; } = null!;

    // Дата начала для фильтрации
    [Reactive]
    public DateTime StartDate { get; set; } = DateTime.Today;

    // Дата окончания для фильтрации
    [Reactive]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

    public ScheduleViewModel(
        IScreen hostScreen,
        IScheduleSlotService scheduleSlotService,
        ICourseInstanceService courseInstanceService,
        IAcademicPeriodService academicPeriodService,
        ITeacherService teacherService,
        IGroupService groupService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        ISubjectService subjectService,
        IDialogService dialogService) : base(hostScreen)
    {
        _scheduleSlotService = scheduleSlotService ?? throw new ArgumentNullException(nameof(scheduleSlotService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _academicPeriodService = academicPeriodService ?? throw new ArgumentNullException(nameof(academicPeriodService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService)); 
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Настройка фильтрации и сортировки
        // Split into two WhenAnyValue chains (ReactiveUI's no-selector overload tops out below 8 properties)
        // and recombine with CombineLatest.
        var coreFilters = this.WhenAnyValue(
            x => x.SearchText,
            x => x.SelectedTeacherFilter,
            x => x.SelectedGroupFilter,
            x => x.SelectedAcademicPeriodFilter,
            x => x.SelectedRoomFilter,
            x => x.SelectedDayOfWeek);

        var viewModeFilters = this.WhenAnyValue(x => x.IsWeekView, x => x.IsMonthView);

        var filterPredicate = coreFilters.CombineLatest(viewModeFilters, (core, view) =>
            CreateFilterPredicate((core.Item1, core.Item2, core.Item3, core.Item4, core.Item5, core.Item6, view.Item1, view.Item2)));

        this.WhenAnyValue(x => x.SelectedDate, x => x.IsDayView, x => x.IsWeekView, x => x.IsMonthView)
            .Select(t => ComputeCurrentPeriodDisplay(t.Item1, t.Item2, t.Item3, t.Item4))
            .ToPropertyEx(this, x => x.CurrentPeriodDisplay);

        _scheduleSlotsSource
            .Connect()
            .Filter(filterPredicate)
            .Sort(SortExpressionComparer<ScheduleSlotViewModel>.Ascending(x => x.DayOfWeek)
                .ThenByAscending(x => x.StartTime))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _scheduleSlots)
            .Subscribe();

        InitializeCommands();
    }

    private void InitializeCommands()
    {
        LoadScheduleCommand = CreateCommand(LoadScheduleAsync, null, "Ошибка загрузки расписания");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        
        AddSlotCommand = CreateCommand(AddSlotAsync, 
            this.WhenAnyValue(x => x.IsLoading).Select(loading => !loading),
            "Ошибка создания слота расписания");
            
        // CanExecute doesn't gate on SelectedSlot: the row buttons pass the slot
        // directly via CommandParameter and never set SelectedSlot first.
        EditSlotCommand = CreateCommand<ScheduleSlotViewModel>(EditSlotAsync, null, "Ошибка редактирования слота");

        DeleteSlotCommand = CreateCommand<ScheduleSlotViewModel>(DeleteSlotAsync, null, "Ошибка удаления слота");
            
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
        ExportScheduleCommand = CreateCommand(ExportScheduleAsync, null, "Ошибка экспорта расписания");
        ImportScheduleCommand = CreateCommand(ImportScheduleAsync, null, "Ошибка импорта расписания");

        CreateScheduleSlotCommand = CreateCommand(AddSlotAsync,
            this.WhenAnyValue(x => x.IsLoading).Select(loading => !loading),
            "Ошибка создания слота расписания");

        AutoGenerateScheduleCommand = CreateCommand(AutoGenerateScheduleAsync,
            this.WhenAnyValue(x => x.IsLoading).Select(loading => !loading),
            "Ошибка автосоставления расписания");

        SetDayViewCommand = ReactiveCommand.Create(() => { IsDayView = true; IsWeekView = false; IsMonthView = false; });
        SetWeekViewCommand = ReactiveCommand.Create(() => { IsDayView = false; IsWeekView = true; IsMonthView = false; });
        SetMonthViewCommand = ReactiveCommand.Create(() => { IsDayView = false; IsWeekView = false; IsMonthView = true; });

        PreviousPeriodCommand = ReactiveCommand.Create(() =>
        {
            SelectedDate = IsWeekView ? SelectedDate.AddDays(-7) : IsMonthView ? SelectedDate.AddMonths(-1) : SelectedDate.AddDays(-1);
            SelectedDayOfWeek = SelectedDate.DayOfWeek;
        });
        NextPeriodCommand = ReactiveCommand.Create(() =>
        {
            SelectedDate = IsWeekView ? SelectedDate.AddDays(7) : IsMonthView ? SelectedDate.AddMonths(1) : SelectedDate.AddDays(1);
            SelectedDayOfWeek = SelectedDate.DayOfWeek;
        });
        GoToTodayCommand = ReactiveCommand.Create(() =>
        {
            SelectedDate = DateTime.Today;
            SelectedDayOfWeek = DateTime.Today.DayOfWeek;
        });
    }

    private async Task AutoGenerateScheduleAsync()
    {
        var period = SelectedAcademicPeriodFilter ?? AvailablePeriods.FirstOrDefault();
        if (period == null)
        {
            ShowWarning("Нет доступных академических периодов для автосоставления расписания");
            return;
        }

        try
        {
            IsLoading = true;
            var success = await _scheduleSlotService.GenerateAutoScheduleAsync(period.Uid);
            if (success)
            {
                await LoadScheduleAsync();
                ShowSuccess("Расписание автоматически составлено");
            }
            else
            {
                ShowWarning("Не удалось автоматически составить расписание");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка автосоставления расписания");
            ShowError("Ошибка автосоставления расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override async void OnFirstTimeLoaded()
    {
        await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Sequential, not Task.WhenAll: these share one scoped EF DbContext,
            // which throws "A second operation was started on this context instance" under concurrent awaits.
            await LoadTeachersAsync();
            await LoadGroupsAsync();
            await LoadPeriodsAsync();
            await LoadRoomsAsync();
            await LoadScheduleAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки начальных данных расписания");
            ShowError("Ошибка загрузки данных расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadScheduleAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            HasErrors = false;

            var slots = await _scheduleSlotService.GetAllAsync();
            var slotViewModels = slots.Select(s => new ScheduleSlotViewModel(s)).ToList();

            _scheduleSlotsSource.Clear();
            _scheduleSlotsSource.AddRange(slotViewModels);
            
            TotalSlots = slotViewModels.Count;

            LogInfo($"Загружено {ScheduleSlots.Count} слотов расписания");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки расписания");
            ErrorMessage = "Не удалось загрузить расписание";
            HasErrors = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTeachersAsync()
    {
        try
        {
            var teachers = await _teacherService.GetAllAsync();
            AvailableTeachers.Clear();
            foreach (var teacher in teachers)
            {
                AvailableTeachers.Add(teacher);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки преподавателей");
        }
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            var groups = await _groupService.GetAllAsync();
            AvailableGroups.Clear();
            foreach (var group in groups)
            {
                AvailableGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки групп");
        }
    }

    private async Task LoadPeriodsAsync()
    {
        try
        {
            var periods = await _academicPeriodService.GetAllAsync();
            AvailablePeriods.Clear();
            foreach (var period in periods)
            {
                AvailablePeriods.Add(period);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки академических периодов");
        }
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            var slots = await _scheduleSlotService.GetAllAsync();
            var rooms = slots.Where(s => !string.IsNullOrEmpty(s.Room))
                           .Select(s => s.Room!)
                           .Distinct()
                           .OrderBy(r => r)
                           .ToList();
            
            AvailableRooms.Clear();
            foreach (var room in rooms)
            {
                AvailableRooms.Add(room);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки аудиторий");
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await LoadInitialDataAsync();
            await _notificationService.SendNotificationAsync(
                "Расписание обновлено",
                $"Расписание было обновлено",
                NotificationType.Success);

            ShowSuccess($"Расписание обновлено: {ScheduleSlots.Count} изменений");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка обновления данных");
            ShowError("Ошибка обновления данных");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task AddSlotAsync()
    {
        try
        {
            IsLoading = true;

            var newSlot = new ScheduleSlot
            {
                Uid = Guid.NewGuid(),
                DayOfWeek = SelectedDayOfWeek,
                StartTime = TimeSpan.FromHours(9), // Значение по умолчанию
                EndTime = TimeSpan.FromHours(10),
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddMonths(6)
            };

            var result = await _dialogService.ShowScheduleSlotEditDialogAsync(newSlot);
            if (result != null && result is ScheduleSlot createdSlot)
            {
                var savedSlot = await _scheduleSlotService.CreateAsync(createdSlot);
                var slotViewModel = new ScheduleSlotViewModel(savedSlot);
                _scheduleSlotsSource.Add(slotViewModel);
                TotalSlots++;

                var currentPerson = await _authService.GetCurrentPersonAsync();
                //await _notificationService.SendNotificationAsync(
                //    currentPerson?.Uid ?? Guid.Empty,
                //    "Слот расписания создан",
                //    $"Новый слот расписания успешно создан",
                //    NotificationType.Success,
                //    NotificationPriority.Normal);

                ShowSuccess("Слот расписания создан");
                LogInfo("Schedule slot created successfully");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create schedule slot");
            ShowError("Не удалось создать слот расписания. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EditSlotAsync(ScheduleSlotViewModel slotViewModel)
    {
        if (slotViewModel == null) return;

        try
        {
            IsLoading = true;

            var slot = new ScheduleSlot
            {
                Uid = slotViewModel.Uid,
                CourseInstanceUid = slotViewModel.CourseInstanceUid,
                DayOfWeek = slotViewModel.DayOfWeek,
                StartTime = slotViewModel.StartTime,
                EndTime = slotViewModel.EndTime,
                Room = slotViewModel.Room,
                ValidFrom = slotViewModel.ValidFrom ?? DateTime.UtcNow,
                ValidTo = slotViewModel.ValidTo
            };

            var result = await _dialogService.ShowScheduleSlotEditDialogAsync(slot);
            if (result != null && result is ScheduleSlot updatedSlot)
            {
                await _scheduleSlotService.UpdateAsync(updatedSlot);
                await LoadScheduleAsync();

                var currentPerson = await _authService.GetCurrentPersonAsync();
                //await _notificationService.SendNotificationAsync(
                //    currentPerson?.Uid ?? Guid.Empty,
                //    "Слот расписания обновлен",
                //    $"Слот расписания успешно обновлен",
                //    NotificationType.Success,
                //    NotificationPriority.Normal);

                ShowSuccess("Слот расписания обновлен");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating schedule slot");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update schedule slot");
            ShowError("Не удалось обновить слот расписания. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteSlotAsync(ScheduleSlotViewModel slotViewModel)
    {
        if (slotViewModel == null) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить слот расписания?");

            if (confirmed)
            {
                await _scheduleSlotService.DeleteAsync(slotViewModel.Uid);
                _scheduleSlotsSource.Remove(slotViewModel);
                TotalSlots--;

                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.SendNotificationAsync(
                    "Слот расписания удален",
                    $"Слот расписания успешно удален",
                    NotificationType.Success);

                ShowSuccess("Слот расписания удален");
                LogInfo("Schedule slot deleted successfully");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete schedule slot");
            ShowError("Не удалось удалить слот расписания. Попробуйте еще раз.");
        }
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedTeacherUid = null;
        SelectedGroupUid = null;
        SelectedPeriodUid = null;
        SelectedRoom = null;
        SelectedDayOfWeek = DateTime.Today.DayOfWeek;
        
        await Task.CompletedTask;
    }

    private async Task ExportScheduleAsync()
    {
        try
        {
            IsLoading = true;
            
            var fileName = await _scheduleSlotService.ExportScheduleAsync(StartDate, EndDate);
            
            var currentPerson = await _authService.GetCurrentPersonAsync();
            //await _notificationService.SendNotificationAsync(
            //    currentPerson?.Uid ?? Guid.Empty,
            //    "Экспорт завершен",
            //    $"Расписание экспортировано в файл {fileName}",
            //    NotificationType.Success,
            //    NotificationPriority.Normal);

            ShowSuccess($"Расписание экспортировано в файл {fileName}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка экспорта расписания");
            ShowError("Ошибка экспорта расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportScheduleAsync()
    {
        try
        {
            IsLoading = true;
            
            var filePath = await _dialogService.ShowFilePickerAsync("Выберите файл расписания", new[] { "*.xlsx", "*.csv" });
            if (!string.IsNullOrEmpty(filePath))
            {
                var importResult = await _scheduleSlotService.ImportScheduleAsync<ScheduleSlot>(filePath);
                
                if (importResult.IsSuccess)
                {
                    await LoadScheduleAsync();
                    
                    var currentPerson = await _authService.GetCurrentPersonAsync();
                    await _notificationService.SendNotificationAsync(
                        "Расписание обновлено",
                        "Импорт расписания завершен успешно",
                        NotificationType.Success);

                    ShowSuccess($"Импортировано {importResult.ImportedCount} слотов расписания");
                }
                else
                {
                    ShowError($"Ошибка импорта: {string.Join(", ", importResult.Errors)}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка импорта расписания");
            ShowError("Ошибка импорта расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Func<ScheduleSlotViewModel, bool> CreateFilterPredicate(
        (string searchText, Teacher? teacher, Group? group, AcademicPeriod? period, string? room, DayOfWeek dayOfWeek, bool isWeekView, bool isMonthView) filters)
    {
        return slot =>
        {
            // Поиск по тексту
            if (!string.IsNullOrWhiteSpace(filters.searchText))
            {
                var searchLower = filters.searchText.ToLowerInvariant();
                var haystack = $"{slot.Room} {slot.CourseName} {slot.GroupName} {slot.TeacherFullName}".ToLowerInvariant();
                if (!haystack.Contains(searchLower))
                    return false;
            }

            // Фильтр по преподавателю
            if (filters.teacher != null && slot.TeacherUid != filters.teacher.Uid)
                return false;

            // Фильтр по группе
            if (filters.group != null && slot.GroupUid != filters.group.Uid)
                return false;

            // Фильтр по аудитории
            if (!string.IsNullOrWhiteSpace(filters.room))
            {
                if (!slot.Room.Equals(filters.room, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // В режиме "День" показываем только выбранный день недели; неделя/месяц показывают все дни
            if (!filters.isWeekView && !filters.isMonthView && slot.DayOfWeek != filters.dayOfWeek)
                return false;

            return true;
        };
    }

    private static string ComputeCurrentPeriodDisplay(DateTime date, bool isDayView, bool isWeekView, bool isMonthView)
    {
        var ru = global::System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

        if (isWeekView)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = date.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(6);
            return $"{startOfWeek:dd.MM} - {endOfWeek:dd.MM.yyyy}";
        }

        if (isMonthView)
        {
            return date.ToString("MMMM yyyy", ru);
        }

        return date.ToString("dddd, dd MMMM yyyy", ru);
    }

    /// <summary>
    /// Логирует информационное сообщение
    /// </summary>
    private void LogInfo(string message)
    {
        StatusLogger.LogInfo(message, "ScheduleViewModel");
    }

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    private void LogError(Exception ex, string message)
    {
        StatusLogger.LogError($"{message}: {ex.Message}", "ScheduleViewModel");
    }
} 

