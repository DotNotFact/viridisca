using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Navigations;
using ViridiscaUi.Navigations;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Models;
using ViridiscaUi.Models.ViewModels;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная страница приложения
/// Отображает общую информацию, учет и быстрый доступ к основным функциям
/// </summary>
[Route("home",
    DisplayName = "Главная",
    IconKey = "Home",
    Order = 0,
    Group = "Основное",
    ShowInMenu = true,
    Description = "Главная страница системы",
    Tags = new[] { "main", "dashboard", "overview" })]
public class HomeViewModel : RoutableViewModelBase
{
    private readonly IPersonService _personService;
    private readonly IAuthService _authService;
    private readonly INotificationService _notificationService;
    private readonly IScheduleSlotService _scheduleSlotService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGradeService _gradeService; 

    #region Properties

    /// <summary>
    /// Заголовок главной страницы
    /// </summary>
    [Reactive] public string Title { get; set; } = "Добро пожаловать в ViridiscaUi LMS";

    /// <summary>
    /// Описание главной страницы
    /// </summary>
    [Reactive] public string Description { get; set; } = "Система управления обучением для современного образования";

    /// <summary>
    /// Информация о текущем пользователе
    /// </summary>
    [Reactive] public CurrentUserInfo? CurrentUser { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Reactive] public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Reactive] public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// Статистические карточки для отображения
    /// </summary>
    [Reactive] public ObservableCollection<StatisticCardViewModel> SystemOverview { get; set; } = new();

    /// <summary>
    /// Быстрые ссылки для навигации
    /// </summary>
    public ObservableCollection<QuickLinkViewModel> QuickLinks { get; } = new();

    /// <summary>
    /// Последние новости
    /// </summary>
    public ObservableCollection<NewsItemViewModel> LatestNews { get; } = new();

    /// <summary>
    /// Последние новости (алиас для совместимости)
    /// </summary>
    public ObservableCollection<NewsItemViewModel> News => LatestNews;

    /// <summary>
    /// Уведомления пользователя
    /// </summary>
    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

    /// <summary>
    /// Предстоящие события
    /// </summary>
    public ObservableCollection<EventItemViewModel> UpcomingEvents { get; } = new();

    /// <summary>
    /// Статистические карточки для ролей
    /// </summary>
    [Reactive] public ObservableCollection<StatisticCardViewModel> RoleStats { get; set; } = new();

    /// <summary>
    /// Последние активности
    /// </summary>
    public ObservableCollection<ActivityItemViewModel> RecentActivities { get; } = new();

    /// <summary>
    /// Количество непрочитанных уведомлений
    /// </summary>
    [Reactive] public int UnreadNotificationsCount { get; set; }

    /// <summary>
    /// Аналитика студента (если текущий пользователь - студент)
    /// </summary>
    [Reactive] public StudentAnalytics? CurrentStudentAnalytics { get; set; }

    /// <summary>
    /// Аналитика преподавателя (если текущий пользователь - преподаватель)
    /// </summary>
    [Reactive] public TeacherAnalytics? CurrentTeacherAnalytics { get; set; }

    /// <summary>
    /// Команда навигации к курсам
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToCoursesCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к студентам
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToStudentsCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к заданиям
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToAssignmentsCommand { get; private set; } = null!;

    // Свойства для статистических карточек используют Analytics
    public string TotalStudents => SystemOverview.FirstOrDefault(s => s.Title == "Студенты")?.Value ?? "0";
    public string ActiveCourses => SystemOverview.FirstOrDefault(s => s.Title == "Курсы")?.Value ?? "0";
    public string TotalTeachers => SystemOverview.FirstOrDefault(s => s.Title == "Преподаватели")?.Value ?? "0";
    public string PendingAssignments => CurrentStudentAnalytics?.PendingAssignments.ToString() ?? CurrentTeacherAnalytics?.TotalCourses.ToString() ?? "0";
    public string WelcomeMessage => CurrentUser?.WelcomeMessage ?? "Добро пожаловать в систему!";

    /// <summary>
    /// Счетчик для быстрых ссылок
    /// </summary>
    [Reactive] public int Count { get; set; }

    /// <summary>
    /// Маршрут для навигации
    /// </summary>
    [Reactive] public string Route { get; set; } = string.Empty;

    #endregion

    #region Commands

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к быстрой ссылке
    /// </summary>
    public ReactiveCommand<QuickLinkViewModel, Unit> NavigateToQuickLinkCommand { get; private set; } = null!;

    /// <summary>
    /// Команда просмотра новости
    /// </summary>
    public ReactiveCommand<NewsItemViewModel, Unit> ViewNewsCommand { get; private set; } = null!;

    /// <summary>
    /// Команда отметки уведомления как прочитанного
    /// </summary>
    public ReactiveCommand<NotificationItemViewModel, Unit> MarkNotificationReadCommand { get; private set; } = null!;

    #endregion

    /// <summary>
    /// Конструктор
    /// </summary>
    public HomeViewModel(
        IScreen hostScreen,
        IPersonService personService,
        IAuthService authService,
        INotificationService notificationService,
        IScheduleSlotService scheduleSlotService,
        IStudentService studentService,
        ITeacherService teacherService,
        ICourseInstanceService courseInstanceService,
        IEnrollmentService enrollmentService,
        IGradeService gradeService ) : base(hostScreen)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _scheduleSlotService = scheduleSlotService ?? throw new ArgumentNullException(nameof(scheduleSlotService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService)); 

        InitializeCommands();
        SetupPropertyNotifications();
        StatusLogger.LogInfo($"HomeViewModel инициализирована");
    }

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await LoadHomeDataAsync();
        StatusLogger.LogInfo($"Данные главной страницы загружены");
    }

    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        RefreshCommand = CreateCommand(LoadHomeDataAsync, null, "Ошибка при обновлении данных");

        NavigateToQuickLinkCommand = CreateCommand<QuickLinkViewModel>(NavigateToQuickLinkAsync, null, "Ошибка навигации");

        ViewNewsCommand = CreateCommand<NewsItemViewModel>(ViewNewsAsync, null, "Ошибка при просмотре новости");

        MarkNotificationReadCommand = CreateCommand<NotificationItemViewModel>(MarkNotificationReadAsync, null, "Ошибка при отметке уведомления как прочитанного");

        NavigateToCoursesCommand = CreateCommand(NavigateToCoursesAsync, null, "Ошибка при навигации к курсам");

        NavigateToStudentsCommand = CreateCommand(NavigateToStudentsAsync, null, "Ошибка при навигации к студентам");

        NavigateToAssignmentsCommand = CreateCommand(NavigateToAssignmentsAsync, null, "Ошибка при навигации к заданиям");
    }

    private void SetupPropertyNotifications()
    {
        // Уведомления об изменении computed properties
        // WhenAnyValue(x => x.SystemOverview) only fires on reference reassignment (this is a
        // plain [Reactive] ObservableCollection, never reassigned after construction) - it does
        // NOT fire on SystemOverview.Clear()/Add(...), so TotalStudents/ActiveCourses/
        // TotalTeachers were only ever raised once at startup with an empty collection and then
        // never again, leaving the dashboard cards permanently stuck at "0" even after
        // LoadSystemOverviewAsync successfully populated real data. Subscribe to the
        // collection's own CollectionChanged instead.
        SystemOverview.CollectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(TotalStudents));
            this.RaisePropertyChanged(nameof(ActiveCourses));
            this.RaisePropertyChanged(nameof(TotalTeachers));
        };

        this.WhenAnyValue(x => x.CurrentStudentAnalytics, x => x.CurrentTeacherAnalytics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(PendingAssignments)));

        this.WhenAnyValue(x => x.CurrentUser)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(WelcomeMessage)));
    }

    private async Task LoadHomeDataAsync()
    {
        try
        {
            SetLoading(true, "Загрузка данных главной страницы...");

            // Sequential, not Task.WhenAll: most of these ultimately share the same
            // scoped ApplicationDbContext (EF DbContext isn't thread-safe for concurrent
            // operations), and running them in parallel was throwing "A second operation
            // was started on this context instance before a previous operation completed"
            // - silently swallowed by each method's own try/catch, which is why the
            // dashboard's student/teacher/course counts intermittently showed 0 even with
            // real data present.
            await LoadUserDataAsync();
            await LoadSystemOverviewAsync();
            await LoadQuickLinksAsync();
            await LoadNewsAsync();
            await LoadNotificationsAsync();
            await LoadUpcomingEventsAsync();
            await LoadRoleStatsAsync();
            await LoadRecentActivitiesAsync();

            ShowSuccess("Данные главной страницы обновлены");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке данных: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                CurrentUser = new Models.ViewModels.CurrentUserInfo
                {
                    Uid = currentPerson.Uid,
                    FirstName = currentPerson.FirstName,
                    LastName = currentPerson.LastName,
                    Email = currentPerson.Email ?? string.Empty,
                    Role = currentPerson.PersonRoles?.FirstOrDefault()?.Role?.Name ?? "Пользователь",
                    LastLoginAt = DateTime.UtcNow
                };

                UserName = $"{currentPerson.FirstName} {currentPerson.LastName}";
                UserRole = currentPerson.PersonRoles?.FirstOrDefault()?.Role?.Name ?? "Пользователь";

                StatusLogger.LogInfo($"Данные пользователя загружены: {UserName}");
            }
            else
            {
                UserName = "Гость";
                UserRole = "Не авторизован";
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке данных пользователя: {ex.Message}");
            UserName = "Ошибка загрузки";
            UserRole = "Неизвестно";
        }
    }

    private async Task LoadSystemOverviewAsync()
    {
        try
        {
            // Загружаем основную статистику системы
            var studentsCount = await GetStudentsCountAsync();
            var teachersCount = await GetTeachersCountAsync();
            var coursesCount = await GetCoursesCountAsync();
            var activeSessionsCount = await GetActiveSessionsCountAsync();

            // Обновляем статистические карточки напрямую
            SystemOverview.Clear();
            SystemOverview.Add(StatisticCardViewModel.Create("Студенты", studentsCount.ToString(), "Общее количество студентов", "AccountMultiple"));
            SystemOverview.Add(StatisticCardViewModel.Create("Преподаватели", teachersCount.ToString(), "Общее количество преподавателей", "AccountTie"));
            SystemOverview.Add(StatisticCardViewModel.Create("Курсы", coursesCount.ToString(), "Общее количество курсов", "BookOpenPageVariant"));
            SystemOverview.Add(StatisticCardViewModel.Create("Активные сессии", activeSessionsCount.ToString(), "Количество активных пользователей", "AccountClock"));

            StatusLogger.LogInfo($"Обзор системы загружен");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке системной статистики: {ex.Message}");
        }
    }

    private async Task LoadQuickLinksAsync()
    {
        try
        {
            QuickLinks.Clear();
            
            var links = new List<QuickLinkViewModel>
            {
                new("Студенты", "Управление студентами", "AccountMultiple", "students"),
                new("Преподаватели", "Управление преподавателями", "AccountTie", "teachers"),
                new("Курсы", "Управление курсами", "BookOpenPageVariant", "courses"),
                new("Расписание", "Просмотр расписания", "Calendar", "schedule")
            };

            foreach (var link in links)
            {
                QuickLinks.Add(link);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке быстрых ссылок: {ex.Message}");
        }
    }

    private async Task LoadNewsAsync()
    {
        try
        {
            News.Clear();

            // Загружаем последние новости из системы уведомлений
            var notifications = await _notificationService.GetRecentNotificationsAsync(10);
            foreach (var notification in notifications.Take(5))
            {
                News.Add(new NewsItemViewModel
                {
                    Title = notification.Title,
                    Content = notification.Message,
                    Author = "Система"
                });
            }

            StatusLogger.LogInfo($"Новости загружены: {News.Count}");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке новостей: {ex.Message}");
        }
    }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                // Заглушка для уведомлений - создаем тестовые данные
                Notifications.Clear();

                // Добавляем несколько тестовых уведомлений
                Notifications.Add(new NotificationItemViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Добро пожаловать!",
                    Message = "Добро пожаловать в систему ViridiscaUi LMS",
                    Date = DateTime.Now.AddHours(-1),
                    Type = NotificationType.Info,
                    Priority = NotificationPriority.Normal,
                    IsRead = false
                });

                UnreadNotificationsCount = Notifications.Count(n => !n.IsRead);
            }

            StatusLogger.LogInfo("Уведомления загружены");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка при загрузке уведомлений: {ex.Message}");
        }
    }

    private async Task LoadUpcomingEventsAsync()
    {
        try
        {
            UpcomingEvents.Clear();

            // Загружаем предстоящие события из расписания
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                var upcomingSlots = await _scheduleSlotService.GetUpcomingSlotsAsync(currentPerson.Uid, 5);
                foreach (var slot in upcomingSlots)
                {
                    UpcomingEvents.Add(new EventItemViewModel
                    {
                        Title = slot.CourseInstance?.Subject?.Name ?? "Занятие",
                        Description = $"Аудитория: {slot.Room}",
                        Date = GetNextOccurrence(slot.DayOfWeek, slot.StartTime),
                        Type = "Занятие"
                    });
                }
            }

            StatusLogger.LogInfo($"Предстоящие события загружены: {UpcomingEvents.Count}");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка загрузки предстоящих событий: {ex.Message}", "HomeViewModel");
        }
    }

    private async Task LoadRoleStatsAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                var userRoles = currentPerson.PersonRoles?.Select(pr => pr.Role?.Name).Where(r => r != null).ToArray() ?? new string[0];

                RoleStats.Clear();

                if (userRoles.Contains("Student"))
                {
                    // Найдем студента по PersonUid
                    var students = await _studentService.GetAllAsync();
                    var currentStudent = students.FirstOrDefault(s => s.PersonUid == currentPerson.Uid);
                    
                    if (currentStudent?.Analytics?.Any() == true)
                    {
                        var studentAnalytics = currentStudent.Analytics.OrderByDescending(a => a.LastCalculated).First();
                        CurrentStudentAnalytics = studentAnalytics;
                        
                        RoleStats.Add(StatisticCardViewModel.Create("Мои курсы", studentAnalytics.ActiveCourses.ToString(), "Количество активных курсов", "BookOpenPageVariant"));
                        RoleStats.Add(StatisticCardViewModel.Create("Средний балл", studentAnalytics.GPA.ToString("F2"), "Текущий GPA", "StarCircle"));
                        RoleStats.Add(StatisticCardViewModel.Create("Задания", studentAnalytics.PendingAssignments.ToString(), "Ожидающие выполнения", "ClipboardText"));
                        RoleStats.Add(StatisticCardViewModel.Create("Посещаемость", $"{studentAnalytics.AttendancePercentage:F1}%", "Процент посещений", "CalendarCheck"));
                    }
                }

                if (userRoles.Contains("Teacher"))
                {
                    // Найдем преподавателя по PersonUid
                    var teachers = await _teacherService.GetAllAsync();
                    var currentTeacher = teachers.FirstOrDefault(t => t.PersonUid == currentPerson.Uid);
                    
                    if (currentTeacher?.Analytics?.Any() == true)
                    {
                        var teacherAnalytics = currentTeacher.Analytics.OrderByDescending(a => a.LastCalculated).First();
                        CurrentTeacherAnalytics = teacherAnalytics;
                        
                        RoleStats.Add(StatisticCardViewModel.Create("Мои курсы", teacherAnalytics.TotalCourses.ToString(), "Количество курсов", "BookOpenPageVariant"));
                        RoleStats.Add(StatisticCardViewModel.Create("Студенты", teacherAnalytics.TotalStudents.ToString(), "Общее количество студентов", "Account"));
                        RoleStats.Add(StatisticCardViewModel.Create("Средний балл", teacherAnalytics.AverageGrade.ToString("F1"), "Средний балл студентов", "StarCircle"));
                    }
                }
            }

            StatusLogger.LogInfo($"Статистика роли загружена");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка загрузки статистики роли: {ex.Message}", "HomeViewModel");
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            // Создание тестовых данных активности
            var activities = new List<ActivityItemViewModel>
            {
                new(Guid.NewGuid(), "Вход в систему", DateTime.Now.AddMinutes(-30), "login"),
                new(Guid.NewGuid(), "Создание задания", DateTime.Now.AddHours(-2), "assignment"),
                new(Guid.NewGuid(), "Обновление профиля", DateTime.Now.AddHours(-4), "profile"),
                new(Guid.NewGuid(), "Просмотр отчета", DateTime.Now.AddDays(-1), "report")
            };

            RecentActivities.Clear();
            RecentActivities.AddRange(activities);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка загрузки активности: {ex.Message}", "HomeViewModel");
        }
    }

    private async Task MarkNotificationReadAsync(NotificationItemViewModel notification)
    {
        try
        {
            SetLoading(true, "Отметка уведомления как прочитанного...");

            // TODO: Implement actual notification marking
            await Task.Delay(500);
            notification.IsRead = true;

            ShowInfo($"Уведомление '{notification.Title}' отмечено как прочитанное");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при отметке уведомления '{notification.Title}'", ex);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task NavigateToCoursesAsync()
    {
        try
        {
            await NavigateToAsync("courses");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к курсам", ex);
        }
    }

    private async Task NavigateToStudentsAsync()
    {
        try
        {
            await NavigateToAsync("students");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к студентам", ex);
        }
    }

    private async Task NavigateToAssignmentsAsync()
    {
        try
        {
            await NavigateToAsync("assignments");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к заданиям", ex);
        }
    }

    // Вспомогательные методы для получения статистики
    private async Task<int> GetStudentsCountAsync()
    {
        try
        {
            var students = await _studentService.GetAllAsync();
            return students.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetTeachersCountAsync()
    {
        try
        {
            var teachers = await _teacherService.GetAllAsync();
            return teachers.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetCoursesCountAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllAsync();
            return courses.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetActiveSessionsCountAsync()
    {
        try
        {
            // Примерная реализация - можно расширить
            return await Task.FromResult(Random.Shared.Next(10, 50));
        }
        catch
        {
            return 0;
        }
    }

    private DateTime GetNextOccurrence(DayOfWeek dayOfWeek, TimeSpan time)
    {
        var today = DateTime.Today;
        var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && DateTime.Now.TimeOfDay > time)
        {
            daysUntilTarget = 7;
        }

        return today.AddDays(daysUntilTarget).Add(time);
    }

    /// <summary>
    /// Навигация к быстрой ссылке
    /// </summary>
    private async Task NavigateToQuickLinkAsync(QuickLinkViewModel quickLink)
    {
        if (quickLink == null) return;

        try
        {
            await NavigateToAsync(quickLink.Route);
            StatusLogger.LogInfo($"Навигация к быстрой ссылке: {quickLink.Route}");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при навигации к быстрой ссылке: {quickLink.Route} - {ex.Message}", "HomeViewModel");
            ShowError($"Не удалось перейти к {quickLink.Title}");
        }
    }

    /// <summary>
    /// Просмотр новости
    /// </summary>
    private async Task ViewNewsAsync(NewsItemViewModel newsItem)
    {
        if (newsItem == null) return;

        try
        {
            // Здесь можно открыть диалог с подробностями новости
            // или перейти к странице новостей
            await NavigateToAsync("news");
            StatusLogger.LogInfo($"Просмотр новости: {newsItem.Title}");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при просмотре новости: {newsItem.Title} - {ex.Message}", "HomeViewModel");
            ShowError("Не удалось открыть новость");
        }
    }

    #endregion
}

