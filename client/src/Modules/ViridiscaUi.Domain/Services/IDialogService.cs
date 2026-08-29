using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Library;

namespace ViridiscaUi.Domain.Services;

/// <summary>
/// Сервис для работы с диалогами пользовательского интерфейса
/// </summary>
public interface IDialogService
{
    // === БАЗОВЫЕ ДИАЛОГИ ===
    
    /// <summary>
    /// Показать информационное сообщение
    /// </summary>
    Task ShowInfoAsync(string title, string message);
    
    /// <summary>
    /// Показать сообщение об ошибке
    /// </summary>
    Task ShowErrorAsync(string title, string message);
    
    /// <summary>
    /// Показать предупреждение
    /// </summary>
    Task ShowWarningAsync(string title, string message);
    
    /// <summary>
    /// Показать обычное сообщение
    /// </summary>
    Task ShowMessageAsync(string title, string message);
    
    /// <summary>
    /// Показать диалог подтверждения
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет");
    
    /// <summary>
    /// Показать диалог подтверждения
    /// </summary>
    Task<bool> ShowConfirmationDialogAsync(string title, string message);
    
    /// <summary>
    /// Показать ошибки валидации
    /// </summary>
    Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors);
    
    /// <summary>
    /// Показать диалог ввода текста
    /// </summary>
    Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "");
    
    // === ФАЙЛОВЫЕ ДИАЛОГИ ===
    
    /// <summary>
    /// Показать диалог выбора файла
    /// </summary>
    Task<string?> ShowFilePickerAsync(string title, string[] extensions);
    
    /// <summary>
    /// Показать диалог открытия файла
    /// </summary>
    Task<string?> ShowFileOpenDialogAsync(string title, string[] extensions);
    
    // === ДИАЛОГИ РЕДАКТИРОВАНИЯ СУЩНОСТЕЙ ===
    
    /// <summary>
    /// Показать диалог редактирования студента
    /// </summary>
    Task<Student?> ShowStudentEditDialogAsync(Student student);
    
    /// <summary>
    /// Показать диалог редактирования преподавателя
    /// </summary>
    Task<Teacher?> ShowTeacherEditDialogAsync(Teacher teacher);
    
    /// <summary>
    /// Показать диалог редактирования группы
    /// </summary>
    Task<Group?> ShowGroupEditDialogAsync(Group group);
    
    /// <summary>
    /// Показать диалог редактирования предмета
    /// </summary>
    Task<Subject?> ShowSubjectEditDialogAsync(Subject subject);
    
    /// <summary>
    /// Показать диалог редактирования курса
    /// </summary>
    Task<Course?> ShowCourseEditDialogAsync(Course course);
    
    /// <summary>
    /// Показать диалог редактирования экземпляра курса
    /// </summary>
    Task<CourseInstance?> ShowCourseInstanceEditDialogAsync(CourseInstance courseInstance);
    
    /// <summary>
    /// Показать диалог редактирования задания
    /// </summary>
    Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment);
    
    /// <summary>
    /// Показать диалог редактирования оценки
    /// </summary>
    Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments);
    
    /// <summary>
    /// Показать диалог редактирования экзамена
    /// </summary>
    Task<Exam?> ShowExamEditDialogAsync(Exam exam);
    
    /// <summary>
    /// Показать диалог редактирования департамента
    /// </summary>
    Task<Department?> ShowDepartmentEditDialogAsync(Department department);
    
    /// <summary>
    /// Показать диалог редактирования слота расписания
    /// </summary>
    Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot slot);
    
    /// <summary>
    /// Показать диалог редактирования ресурса библиотеки
    /// </summary>
    Task<LibraryResource?> ShowLibraryResourceEditDialogAsync(LibraryResource resource);
    
    /// <summary>
    /// Показать диалог редактирования учебного плана
    /// </summary>
    Task<Curriculum?> ShowCurriculumEditDialogAsync(Curriculum curriculum);
    
    // === ДИАЛОГИ ПРОСМОТРА ДЕТАЛЕЙ ===
    
    /// <summary>
    /// Показать детали студента
    /// </summary>
    Task ShowStudentDetailsDialogAsync(Student student);
    
    /// <summary>
    /// Показать детали преподавателя
    /// </summary>
    Task<Teacher?> ShowTeacherDetailsDialogAsync(Teacher teacher);
    
    /// <summary>
    /// Показать детали задания
    /// </summary>
    Task ShowAssignmentDetailsDialogAsync(Assignment assignment);
    
    // === ДИАЛОГИ ВЫБОРА ===
    
    /// <summary>
    /// Показать диалог выбора преподавателя
    /// </summary>
    Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers);
    
    /// <summary>
    /// Показать диалог выбора групп
    /// </summary>
    Task<IEnumerable<Group>?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups);
    
    /// <summary>
    /// Показать диалог выбора экземпляров курсов
    /// </summary>
    Task<IEnumerable<CourseInstance>?> ShowCourseInstanceSelectionDialogAsync(IEnumerable<CourseInstance> courseInstances);
    
    // === СПЕЦИАЛИЗИРОВАННЫЕ ДИАЛОГИ ===
    
    /// <summary>
    /// Показать диалог статистики преподавателя
    /// </summary>
    Task ShowTeacherStatisticsDialogAsync(string title, object statistics);
    
    /// <summary>
    /// Показать диалог статистики курса
    /// </summary>
    Task ShowCourseStatisticsDialogAsync(CourseInstance courseInstance);
    
    /// <summary>
    /// Показать диалог управления курсами преподавателя
    /// </summary>
    Task<bool> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<CourseInstance> allCourses);
    
    /// <summary>
    /// Показать диалог управления группами преподавателя
    /// </summary>
    Task<bool> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> allGroups);
    
    /// <summary>
    /// Показать диалог записи на курс
    /// </summary>
    Task<bool> ShowCourseEnrollmentDialogAsync(CourseInstance courseInstance, IEnumerable<Student> students);
    
    /// <summary>
    /// Показать диалог управления контентом курса
    /// </summary>
    Task<bool> ShowCourseContentManagementDialogAsync(Course course);
    
    /// <summary>
    /// Показать диалог просмотра работ
    /// </summary>
    Task<bool> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<object> submissions);
    
    /// <summary>
    /// Показать диалог массового оценивания
    /// </summary>
    Task<object?> ShowBulkGradingDialogAsync(IEnumerable<object> submissions);
    
    /// <summary>
    /// Показать диалог массового редактирования
    /// </summary>
    Task<object?> ShowBulkEditDialogAsync(object options);
    
    /// <summary>
    /// Показать диалог создания займа
    /// </summary>
    Task<object?> ShowCreateLoanDialogAsync();
    
    /// <summary>
    /// Показать диалог продления займа
    /// </summary>
    Task<object?> ShowExtendLoanDialogAsync(object loanViewModel);
    
    /// <summary>
    /// Показать диалог просроченных займов
    /// </summary>
    Task ShowOverdueLoansDialogAsync();
} 