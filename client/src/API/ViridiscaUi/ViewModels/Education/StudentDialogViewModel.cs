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
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using DomainGroup = ViridiscaUi.Domain.Entities.Education.Group;
using System.Net.Mail;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования студента
/// Работает с композицией Person + Student
/// </summary>
public class StudentDialogViewModel : RoutableViewModelBase
{
    private readonly IStudentService _studentService;
    private readonly IPersonService _personService;
    private readonly IGroupService _groupService;
    private readonly ICurriculumService _curriculumService;
    private readonly IDepartmentService _departmentService;

    #region Person Properties (из связанной модели Person)

    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);
    [Reactive] public string Address { get; set; } = string.Empty;

    #endregion

    #region Student Properties (из модели Student)

    [Reactive] public string StudentCode { get; set; } = string.Empty;
    [Reactive] public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime? GraduationDate { get; set; }
    [Reactive] public StudentStatus Status { get; set; } = StudentStatus.Active;
    [Reactive] public double GPA { get; set; }
    [Reactive] public int AcademicYear { get; set; } = DateTime.Now.Year;
    [Reactive] public Guid? GroupUid { get; set; }
    [Reactive] public Guid? CurriculumUid { get; set; }

    #endregion

    #region UI Properties

    [Reactive] public DomainGroup? SelectedGroup { get; set; }
    [Reactive] public Curriculum? SelectedCurriculum { get; set; }
    [Reactive] public Department? SelectedDepartment { get; set; }
    
    [Reactive] public string GroupSearchText { get; set; } = string.Empty;
    [Reactive] public string CurriculumSearchText { get; set; } = string.Empty;
    [Reactive] public string DepartmentSearchText { get; set; } = string.Empty;
    
    [Reactive] public bool IsLoadingGroups { get; set; }
    [Reactive] public bool IsLoadingCurricula { get; set; }
    [Reactive] public bool IsLoadingDepartments { get; set; }
    
    [Reactive] public string? ValidationError { get; set; }
    [Reactive] public string Title { get; set; } = "Новый студент";
    [Reactive] public bool IsEditMode { get; set; }

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<DomainGroup> AvailableGroups { get; set; } = new();
    [Reactive] public ObservableCollection<Curriculum> AvailableCurricula { get; set; } = new();
    [Reactive] public ObservableCollection<Department> AvailableDepartments { get; set; } = new();
    [Reactive] public ObservableCollection<StudentStatus> AvailableStatuses { get; set; } = new();

    #endregion

    #region Internal State

    private Person? _currentPerson;
    private Student? _currentStudent;

    #endregion

    #region Constructor

    public StudentDialogViewModel(
        IScreen? hostScreen,
        IStudentService studentService,
        IPersonService personService,
        IGroupService groupService,
        ICurriculumService curriculumService,
        IDepartmentService departmentService)
        : base(hostScreen)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));

        Title = "Новый студент";
        
        InitializeCollections();
        SetupCommands();
        SetupValidation();
        
        // Загружаем справочники
        LoadDataAsync();
    }

    #endregion

    #region Initialization

    private void InitializeCollections()
    {
        // Инициализация статусов
        AvailableStatuses.Clear();
        foreach (var status in Enum.GetValues<StudentStatus>())
        {
            AvailableStatuses.Add(status);
        }
    }

    private void SetupCommands()
    {
        // Команды поиска
        SearchGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
        SearchCurriculaCommand = ReactiveCommand.CreateFromTask(LoadCurriculaAsync);
        SearchDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
        
        // Команды диалога
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void SetupValidation()
    {
        // Настройка реактивной валидации будет добавлена позже
    }

    private async void LoadDataAsync()
    {
        // Sequential, not Task.WhenAll: these share one scoped EF DbContext,
        // which throws "A second operation was started on this context instance" under concurrent awaits.
        await LoadGroupsAsync();
        await LoadCurriculaAsync();
        await LoadDepartmentsAsync();
    }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> SearchGroupsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SearchCurriculaCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SearchDepartmentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; } = null!;

    #endregion

    #region Data Loading

    private async Task LoadGroupsAsync()
    {
        try
        {
            IsLoadingGroups = true;
            var groups = await _groupService.GetAllAsync();
            
            AvailableGroups.Clear();
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                AvailableGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки групп");
        }
        finally
        {
            IsLoadingGroups = false;
        }
    }

    private async Task LoadCurriculaAsync()
    {
        try
        {
            IsLoadingCurricula = true;
            var curricula = await _curriculumService.GetAllAsync();
            
            AvailableCurricula.Clear();
            foreach (var curriculum in curricula.OrderBy(c => c.Name))
            {
                AvailableCurricula.Add(curriculum);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки учебных планов");
        }
        finally
        {
            IsLoadingCurricula = false;
        }
    }

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            IsLoadingDepartments = true;
            var departments = await _departmentService.GetAllAsync();
            
            AvailableDepartments.Clear();
            foreach (var department in departments.OrderBy(d => d.Name))
            {
                AvailableDepartments.Add(department);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки кафедр");
        }
        finally
        {
            IsLoadingDepartments = false;
        }
    }

    #endregion

    #region Entity Management

    /// <summary>
    /// Загружает данные студента для редактирования
    /// </summary>
    public async Task LoadStudentAsync(Guid studentUid)
    {
        try
        {
            var student = await _studentService.GetByUidAsync(studentUid);
            if (student?.Person == null)
            {
                throw new InvalidOperationException($"Студент с ID {studentUid} не найден или не имеет связанного Person");
            }

            _currentStudent = student;
            _currentPerson = student.Person;
            
            PopulateFromEntities(student, student.Person);
            
            Title = $"Редактирование студента: {student.Person.FullName}";
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки студента {studentUid}");
            throw;
        }
    }

    /// <summary>
    /// Загружает данные студента для редактирования (перегрузка для объекта Student)
    /// </summary>
    public async Task LoadStudentAsync(Student student)
    {
        try
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            // A blank "new student" template (from the "Добавить студента" button) has no
            // PersonUid yet and was never persisted, so GetByUidAsync(student.Uid) legitimately
            // returns null - that's the expected "create" case, not an error. Only a student
            // that claims an existing PersonUid but can't be found is a genuine problem.
            if (student.PersonUid == Guid.Empty)
            {
                Title = "Новый студент";
                return;
            }

            // Если нужна полная загрузка связанных данных, загружаем из сервиса
            var fullStudent = await _studentService.GetByUidAsync(student.Uid);
            if (fullStudent == null)
            {
                throw new InvalidOperationException($"Студент с ID {student.Uid} не найден");
            }

            // Загружаем связанную Person
            var person = await _personService.GetByUidAsync(fullStudent.PersonUid);
            if (person == null)
            {
                throw new InvalidOperationException($"Person с ID {fullStudent.PersonUid} не найден");
            }

            PopulateFromEntities(fullStudent, person);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки студента {student.Uid}");
            throw;
        }
    }

    /// <summary>
    /// Заполняет форму данными из сущностей Person и Student
    /// </summary>
    private void PopulateFromEntities(Student student, Person person)
    {
        // Данные Person
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName ?? string.Empty;
        Email = person.Email;
        Phone = person.Phone ?? string.Empty;
        DateOfBirth = person.DateOfBirth ?? DateTime.Today.AddYears(-18);
        Address = person.Address ?? string.Empty;

        // Данные Student
        StudentCode = student.StudentCode;
        EnrollmentDate = student.EnrollmentDate;
        GraduationDate = student.GraduationDate;
        Status = student.Status;
        GPA = student.GPA;
        AcademicYear = student.AcademicYear;
        GroupUid = student.GroupUid;
        CurriculumUid = student.CurriculumUid;

        // Установка выбранных элементов
        SelectedGroup = AvailableGroups.FirstOrDefault(g => g.Uid == student.GroupUid);
        SelectedCurriculum = AvailableCurricula.FirstOrDefault(c => c.Uid == student.CurriculumUid);
    }

    /// <summary>
    /// Получает обновленного студента после сохранения
    /// </summary>
    public Student? GetUpdatedStudent()
    {
        return _currentStudent;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Валидирует данные студента и Person
    /// </summary>
    protected bool Validate()
    {
        var errors = new List<string>();
        
        // Валидация Person
        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("Необходимо указать имя студента");
            
        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Необходимо указать фамилию студента");
            
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Необходимо указать email студента");
        else if (!IsValidEmail(Email))
            errors.Add("Некорректный формат email");
            
        if (!string.IsNullOrWhiteSpace(Phone) && !IsValidPhone(Phone))
            errors.Add("Некорректный формат телефона");
            
        if (DateOfBirth > DateTime.Today.AddYears(-16))
            errors.Add("Студент должен быть старше 16 лет");
        
        // Валидация Student
        if (string.IsNullOrWhiteSpace(StudentCode))
            errors.Add("Необходимо указать студенческий код");

        if (EnrollmentDate > DateTime.Now)
            errors.Add("Дата поступления не может быть в будущем");

        if (SelectedGroup == null)
            errors.Add("Необходимо выбрать группу");
        
        ValidationError = errors.Count > 0 ? string.Join(Environment.NewLine, errors) : null;
        return string.IsNullOrEmpty(ValidationError);
    }

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

    private static bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^[\+]?[1-9][\d]{0,15}$");
    }

    #endregion

    #region Save Logic

    /// <summary>
    /// Сохраняет студента (создание или обновление)
    /// </summary>
    protected async Task<Student> SaveEntityAsync()
    {
        if (!Validate())
        {
            throw new ValidationException(ValidationError);
        }

        if (_currentStudent == null)
        {
            return await CreateNewStudentAsync();
        }
        else
        {
            return await UpdateExistingStudentAsync();
        }
    }

    /// <summary>
    /// Создает нового студента с Person
    /// </summary>
    private async Task<Student> CreateNewStudentAsync()
    {
        // 1. Создаем Person
        var person = new Person
        {
            Uid = Guid.NewGuid(),
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Email = Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
            DateOfBirth = DateOfBirth,
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdPerson = await _personService.CreateAsync(person);

        // 2. Создаем Student
        var student = new Student
        {
            Uid = Guid.NewGuid(),
            PersonUid = createdPerson.Uid,
            StudentCode = StudentCode.Trim(),
            EnrollmentDate = EnrollmentDate,
            GraduationDate = GraduationDate,
            Status = Status,
            GPA = GPA,
            AcademicYear = AcademicYear,
            GroupUid = SelectedGroup?.Uid,
            CurriculumUid = SelectedCurriculum?.Uid,
            CreatedAt = DateTime.UtcNow
        };

        var createdStudent = await _studentService.CreateAsync(student);
        
        _currentStudent = createdStudent;
        _currentPerson = createdPerson;

        LogInfo($"Created new student {createdStudent.StudentCode} for Person {createdPerson.FullName}");
            
        return createdStudent;
    }

    /// <summary>
    /// Обновляет существующего студента и связанного Person
    /// </summary>
    private async Task<Student> UpdateExistingStudentAsync()
    {
        if (_currentStudent == null || _currentPerson == null)
            throw new InvalidOperationException("Нет данных для обновления");

        // 1. Обновляем Person
        _currentPerson.FirstName = FirstName.Trim();
        _currentPerson.LastName = LastName.Trim();
        _currentPerson.MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim();
        _currentPerson.Email = Email.Trim();
        _currentPerson.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
        _currentPerson.DateOfBirth = DateOfBirth;
        _currentPerson.Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim();
        _currentPerson.LastModifiedAt = DateTime.UtcNow;

        await _personService.UpdateAsync(_currentPerson);

        // 2. Обновляем Student
        _currentStudent.StudentCode = StudentCode.Trim();
        _currentStudent.EnrollmentDate = EnrollmentDate;
        _currentStudent.GraduationDate = GraduationDate;
        _currentStudent.Status = Status;
        _currentStudent.GPA = GPA;
        _currentStudent.AcademicYear = AcademicYear;
        _currentStudent.GroupUid = SelectedGroup?.Uid;
        _currentStudent.CurriculumUid = SelectedCurriculum?.Uid;
        _currentStudent.LastModifiedAt = DateTime.UtcNow;

        await _studentService.UpdateAsync(_currentStudent);

        LogInfo($"Updated student {_currentStudent.StudentCode} for Person {_currentPerson.FullName}");
            
        return _currentStudent;
    }

    #endregion

    private async Task<bool> ValidateStudentAsync()
    {
        try
        {
            Validate();
            return string.IsNullOrEmpty(ValidationError);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при валидации студента");
            return false;
        }
    }

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания нового студента
    /// </summary>
    public async Task InitializeAsync()
    {
        Initialize(null);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующего студента
    /// </summary>
    public async Task InitializeAsync(Student student)
    {
        Initialize(student);
        await Task.CompletedTask;
    }

    private void Initialize(Student? student)
    {
        if (student == null)
        {
            IsEditMode = false;
            Title = "Новый студент";
            // Очистить все поля для нового студента
            ClearFields();
        }
        else
        {
            IsEditMode = true;
            Title = "Редактирование студента";
            // Загрузить данные студента
            LoadStudentAsync(student).ConfigureAwait(false);
        }
    }

    private void ClearFields()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        DateOfBirth = DateTime.Today.AddYears(-18);
        Address = string.Empty;
        StudentCode = string.Empty;
        EnrollmentDate = DateTime.Now;
        GraduationDate = null;
        Status = StudentStatus.Active;
        GPA = 0;
        AcademicYear = DateTime.Now.Year;
        SelectedGroup = null;
        SelectedCurriculum = null;
        SelectedDepartment = null;
    }

    private async Task SaveAsync()
    {
        try
        {
            if (!Validate())
            {
                return;
            }

            SetLoading(true, "Сохранение студента...");
            var savedStudent = await SaveEntityAsync();
            
            ShowSuccess("Студент успешно сохранен");
            
            if (HostScreen is { } screen)
            {
                await screen.Router.NavigateBack.Execute();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving student");
            LogInfo($"Ошибка сохранения студента: {ex.Message}");
            throw;
        }
    }

    private void Cancel()
    {
        LogInfo("Отмена создания/редактирования студента");
    }

    #endregion
} 

