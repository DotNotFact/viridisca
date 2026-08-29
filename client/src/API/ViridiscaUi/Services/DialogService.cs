using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ViridiscaUi.Domain.Services;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Library;
using ViridiscaUi.Views.Common;
using ViridiscaUi.Views.Education;
using ViridiscaUi.ViewModels.Education;
using Microsoft.Extensions.DependencyInjection;
using Material.Icons;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Services;

/// <summary>
/// Реализация сервиса диалогов для Avalonia UI
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    private Window? GetActiveWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }

    // Dialog/editor ViewModels extend RoutableViewModelBase (IScreen hostScreen constructor
    // param), but IScreen isn't registered in the DI container - a plain
    // _serviceProvider.GetRequiredService<T>() throws "No service for type 'IScreen'" here,
    // same root cause as the sidebar navigation bug fixed in UnifiedNavigationService. Supply
    // the real screen (the actual MainViewModel, via IUnifiedNavigationService.CurrentScreen)
    // explicitly and let ActivatorUtilities resolve everything else from DI.
    private T CreateRoutable<T>() where T : class
    {
        var screen = _serviceProvider.GetRequiredService<ViridiscaUi.Navigations.IUnifiedNavigationService>().CurrentScreen;
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, screen!);
    }

    // === БАЗОВЫЕ ДИАЛОГИ ===

    public async Task ShowInfoAsync(string title, string message)
    {
        try
        {
            StatusLogger.LogInfo($"Showing info dialog: {title}", nameof(DialogService));
            
            Window dialog = null!;
            
            dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new Button 
                        { 
                            Content = "OK", 
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Margin = new Avalonia.Thickness(0, 20, 0, 0),
                            Command = ReactiveUI.ReactiveCommand.Create(() => dialog.Close())
                        }
                    }
                }
            };
            
            var owner = GetActiveWindow();
            if (owner != null)
            {
                await dialog.ShowDialog(owner);
            }
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing info dialog: {ex.Message}", nameof(DialogService));
        }
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            StatusLogger.LogError($"Showing error dialog: {title} - {message}", nameof(DialogService));
            await ShowInfoAsync($"❌ {title}", message);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing error dialog: {ex.Message}", nameof(DialogService));
        }
    }

    public async Task ShowWarningAsync(string title, string message)
    {
        try
        {
            StatusLogger.LogWarning($"Showing warning dialog: {title} - {message}", nameof(DialogService));
            await ShowInfoAsync($"⚠️ {title}", message);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing warning dialog: {ex.Message}", nameof(DialogService));
        }
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        await ShowInfoAsync(title, message);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет")
    {
        try
        {
            StatusLogger.LogInfo($"Showing confirmation dialog: {title}", nameof(DialogService));
            
            bool result = false;
            Window dialog = null!;
            
            dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Margin = new Avalonia.Thickness(0, 20, 0, 0),
                            Spacing = 10,
                            Children =
                            {
                                new Button 
                                { 
                                    Content = confirmText,
                                    Command = ReactiveUI.ReactiveCommand.Create(() => { result = true; dialog.Close(); })
                                },
                                new Button 
                                { 
                                    Content = cancelText,
                                    Command = ReactiveUI.ReactiveCommand.Create(() => { result = false; dialog.Close(); })
                                }
                            }
                        }
                    }
                }
            };
            
            var owner = GetActiveWindow();
            if (owner != null)
            {
                await dialog.ShowDialog(owner);
            }
            
            StatusLogger.LogInfo($"Confirmation dialog result: {result}", nameof(DialogService));
            return result;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing confirmation dialog: {ex.Message}", nameof(DialogService));
            return false;
        }
    }

    public async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        return await ShowConfirmationAsync(title, message);
    }

    public async Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors)
    {
        var message = string.Join("\n• ", errors.Prepend("Обнаружены следующие ошибки:"));
        await ShowErrorAsync(title, message);
    }

    public async Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "")
    {
        // TODO: Implement proper text input dialog
        // For now, return default value
        await ShowInfoAsync(title, $"{message}\nВведенное значение: {defaultValue}");
        return defaultValue;
    }

    // === ФАЙЛОВЫЕ ДИАЛОГИ ===

    public async Task<string?> ShowFilePickerAsync(string title, string[] extensions)
    {
        var window = GetActiveWindow();
        if (window?.StorageProvider != null)
        {
            var fileTypes = extensions.Select(ext => new FilePickerFileType(ext)
            {
                Patterns = new[] { ext }
            }).ToArray();

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            };

            var result = await window.StorageProvider.OpenFilePickerAsync(options);
            return result?.FirstOrDefault()?.Path.LocalPath;
        }
        
        return null;
    }

    public async Task<string?> ShowFileOpenDialogAsync(string title, string[] extensions)
    {
        return await ShowFilePickerAsync(title, extensions);
    }

    // === ДИАЛОГИ РЕДАКТИРОВАНИЯ СУЩНОСТЕЙ ===

    public async Task<Student?> ShowStudentEditDialogAsync(Student student)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            // Create ViewModel with DI
            var viewModel = CreateRoutable<StudentDialogViewModel>();
            await viewModel.LoadStudentAsync(student);

            // Create Dialog Window
            var dialog = new StudentDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<Student?>(owner);
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования студента: {ex.Message}");
            return null;
        }
    }

    public async Task<Teacher?> ShowTeacherEditDialogAsync(Teacher teacher)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            var viewModel = CreateRoutable<TeacherDialogViewModel>();
            await viewModel.LoadTeacherAsync(teacher);

            var dialog = new TeacherDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<Teacher?>(owner);
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования преподавателя: {ex.Message}");
            return null;
        }
    }

    public async Task<Group?> ShowGroupEditDialogAsync(Group group)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            var viewModel = CreateRoutable<GroupDialogViewModel>();
            await viewModel.LoadGroupAsync(group);

            var dialog = new GroupDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<Group?>(owner);
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования группы: {ex.Message}");
            return null;
        }
    }

    public async Task<Subject?> ShowSubjectEditDialogAsync(Subject subject)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            var viewModel = CreateRoutable<SubjectDialogViewModel>();
            await viewModel.LoadSubjectAsync(subject);

            var dialog = new SubjectDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<Subject?>(owner);
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования предмета: {ex.Message}");
            return null;
        }
    }

    public async Task<Course?> ShowCourseEditDialogAsync(Course course)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            var viewModel = CreateRoutable<CourseEditorViewModel>();
            await viewModel.LoadCourseAsync(course);

            var dialog = new CourseEditDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<Course?>(owner);
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования курса: {ex.Message}");
            return null;
        }
    }

    public async Task<CourseInstance?> ShowCourseInstanceEditDialogAsync(CourseInstance courseInstance)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            // TODO: Implement CourseInstanceDialog when created
            await ShowInfoAsync("Редактирование экземпляра курса", $"Редактирование экземпляра курса: {courseInstance.Name}");
            return courseInstance; // Return unchanged for now
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования экземпляра курса: {ex.Message}");
            return null;
        }
    }

    public async Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment)
    {
        // TODO: Implement AssignmentDialog when created
        await ShowInfoAsync("Редактирование задания", $"Редактирование задания: {assignment.Title}");
        return assignment; // Return unchanged for now
    }

    public async Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments)
    {
        try
        {
            var owner = GetActiveWindow();
            if (owner == null) return null;

            var viewModel = CreateRoutable<GradeDialogViewModel>();
            viewModel.LoadGradeAsync(grade);

            var dialog = new GradeDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<bool>(owner);
            
            return result ? viewModel.GetUpdatedGrade() : null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования оценки: {ex.Message}");
            return null;
        }
    }

    public async Task<Exam?> ShowExamEditDialogAsync(Exam exam)
    {
        // TODO: Implement ExamDialog when created
        await ShowInfoAsync("Редактирование экзамена", $"Редактирование экзамена: {exam.Title}");
        return exam; // Return unchanged for now
    }

    public async Task<Department?> ShowDepartmentEditDialogAsync(Department department)
    {
        // TODO: Implement DepartmentDialog when created
        await ShowInfoAsync("Редактирование департамента", $"Редактирование департамента: {department.Name}");
        return department; // Return unchanged for now
    }

    public async Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot slot)
    {
        // TODO: Implement ScheduleSlotDialog when created
        await ShowInfoAsync("Редактирование слота расписания", $"Редактирование слота: {slot.StartTime} - {slot.EndTime}");
        return slot; // Return unchanged for now
    }

    public async Task<LibraryResource?> ShowLibraryResourceEditDialogAsync(LibraryResource resource)
    {
        // TODO: Implement library resource edit dialog
        return await Task.FromResult(resource);
    }

    public async Task<Curriculum?> ShowCurriculumEditDialogAsync(Curriculum curriculum)
    {
        // TODO: Implement curriculum edit dialog
        return await Task.FromResult(curriculum);
    }

    // === ДИАЛОГИ ПРОСМОТРА ДЕТАЛЕЙ ===

    public async Task ShowStudentDetailsDialogAsync(Student student)
    {
        var details = $"Студент: {student.Person?.FirstName} {student.Person?.LastName}\n" +
                     $"Код: {student.StudentCode}\n" +
                     $"Дата поступления: {student.EnrollmentDate:dd.MM.yyyy}\n" +
                     $"Статус: {student.Status}";
        
        await ShowInfoAsync("Детали студента", details);
    }

    public async Task<Teacher?> ShowTeacherDetailsDialogAsync(Teacher teacher)
    {
        var details = $"Преподаватель: {teacher.Person?.FirstName} {teacher.Person?.LastName}\n" +
                     $"Код: {teacher.EmployeeCode}\n" +
                     $"Дата найма: {teacher.HireDate:dd.MM.yyyy}\n" +
                     $"Статус: {teacher.IsActive}";
        
        await ShowInfoAsync("Детали преподавателя", details);
        return teacher;
    }

    public async Task ShowAssignmentDetailsDialogAsync(Assignment assignment)
    {
        var details = $"Задание: {assignment.Title}\n" +
                     $"Описание: {assignment.Description}\n" +
                     $"Срок сдачи: {assignment.DueDate:dd.MM.yyyy HH:mm}\n" +
                     $"Максимальный балл: {assignment.MaxScore}";
        
        await ShowInfoAsync("Детали задания", details);
    }

    // === ДИАЛОГИ ВЫБОРА ===

    public async Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers)
    {
        // TODO: Implement proper selection dialog
        await ShowInfoAsync("Выбор преподавателя", $"Доступно преподавателей: {teachers.Count()}");
        return teachers.FirstOrDefault();
    }

    public async Task<IEnumerable<Group>?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups)
    {
        // TODO: Implement proper selection dialog
        await ShowInfoAsync("Выбор групп", $"Доступно групп: {groups.Count()}");
        return groups.Take(1);
    }

    public async Task<IEnumerable<CourseInstance>?> ShowCourseInstanceSelectionDialogAsync(IEnumerable<CourseInstance> courseInstances)
    {
        // TODO: Implement proper selection dialog
        await ShowInfoAsync("Выбор экземпляров курсов", $"Доступно экземпляров: {courseInstances.Count()}");
        return courseInstances.Take(1);
    }

    // === ОСТАЛЬНЫЕ МЕТОДЫ (ЗАГЛУШКИ) ===

    public async Task ShowTeacherStatisticsDialogAsync(string title, object statistics)
    {
        await ShowInfoAsync(title, "Статистика преподавателя");
    }

    public async Task ShowCourseStatisticsDialogAsync(CourseInstance courseInstance)
    {
        var course = new Course
        {
            Uid = courseInstance.SubjectUid,
            Name = courseInstance.Subject?.Name ?? courseInstance.Name,
            Code = courseInstance.Subject?.Code ?? courseInstance.Code,
            Description = courseInstance.Subject?.Description ?? courseInstance.Description,
            Credits = courseInstance.Subject?.Credits ?? 0,
            Type = courseInstance.Subject?.Type != null ? (CourseType)courseInstance.Subject.Type : CourseType.Core,
            IsActive = courseInstance.IsActive,
            DepartmentUid = courseInstance.Subject?.DepartmentUid
        };

        await ShowInfoAsync("Статистика курса", $"Статистика для курса: {course.Name}");
    }

    public async Task<bool> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<CourseInstance> allCourses)
    {
        return await ShowConfirmationAsync("Управление курсами", $"Управление курсами для {teacher.Person?.FirstName} {teacher.Person?.LastName}");
    }

    public async Task<bool> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> allGroups)
    {
        return await ShowConfirmationAsync("Управление группами", $"Управление группами для {teacher.Person?.FirstName} {teacher.Person?.LastName}");
    }

    public async Task<bool> ShowCourseEnrollmentDialogAsync(CourseInstance courseInstance, IEnumerable<Student> students)
    {
        return await ShowConfirmationAsync("Запись на курс", $"Записать студентов на курс: {courseInstance.Subject?.Name ?? courseInstance.Name}");
    }

    public async Task<bool> ShowCourseContentManagementDialogAsync(Course course)
    {
        return await ShowConfirmationAsync("Управление содержимым курса", $"Управление содержимым курса: {course.Name}");
    }

    public async Task<bool> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<object> submissions)
    {
        return await ShowConfirmationAsync("Просмотр работ", $"Просмотр работ по заданию: {assignment.Title}");
    }

    public async Task<object?> ShowBulkGradingDialogAsync(IEnumerable<object> submissions)
    {
        await ShowInfoAsync("Массовое оценивание", $"Массовое оценивание {submissions.Count()} работ");
        return null;
    }

    public async Task<object?> ShowBulkEditDialogAsync(object options)
    {
        await ShowInfoAsync("Массовое редактирование", "Массовое редактирование записей");
        return null;
    }

    public async Task<object?> ShowCreateLoanDialogAsync()
    {
        await ShowInfoAsync("Создание займа", "Создание нового займа");
        return null;
    }

    public async Task<object?> ShowExtendLoanDialogAsync(object loanViewModel)
    {
        await ShowInfoAsync("Продление займа", "Продление существующего займа");
        return null;
    }

    public async Task ShowOverdueLoansDialogAsync()
    {
        await ShowInfoAsync("Просроченные займы", "Просмотр просроченных займов");
    }

    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        try
        {
            StatusLogger.LogInfo($"Showing input dialog: {title}", nameof(DialogService));
            
            // For now, show info dialog with the message and return default value
            // TODO: Implement proper input dialog when available
            await ShowInfoAsync(title, $"{message}\n\nЗначение по умолчанию: {defaultValue}");
            
            StatusLogger.LogInfo($"Input dialog completed", nameof(DialogService));
            return defaultValue;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error showing input dialog: {ex.Message}", nameof(DialogService));
            return null;
        }
    }
} 