using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.System;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования преподавателя
/// Работает с композицией Person + Teacher
/// </summary>
public class TeacherDialogViewModel : RoutableViewModelBase
{
    private readonly ITeacherService _teacherService;
    private readonly IPersonService _personService;
    private readonly IDepartmentService _departmentService; 
    private readonly IDialogService _dialogService;
    private readonly ILogger<TeacherDialogViewModel> _logger;

    #region Person Properties (из связанной модели Person)

    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public DateTime? DateOfBirth { get; set; }
    [Reactive] public string Address { get; set; } = string.Empty;

    #endregion

    #region Teacher Properties (из модели Teacher)

    [Reactive] public string EmployeeCode { get; set; } = string.Empty;
    [Reactive] public DateTime HireDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime? TerminationDate { get; set; }
    [Reactive] public string Qualification { get; set; } = string.Empty;
    [Reactive] public string Specialization { get; set; } = string.Empty;
    [Reactive] public decimal Salary { get; set; }
    [Reactive] public Guid? DepartmentUid { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;

    #endregion

    #region UI Properties

    [Reactive] public Department? SelectedDepartment { get; set; }
    [Reactive] public string DepartmentSearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoadingDepartments { get; set; }

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<Department> AvailableDepartments { get; set; } = new();
    [Reactive] public ObservableCollection<Person> AvailablePersons { get; set; } = new();
    [Reactive] public ObservableCollection<Person> Persons { get; set; } = [];
    [Reactive] public Person? SelectedPerson { get; set; }

    #endregion

    #region Internal State

    private Person? _currentPerson;
    private Teacher? _currentTeacher;

    #endregion

    #region Constructor

    public TeacherDialogViewModel(
        IScreen? hostScreen,
        ITeacherService teacherService,
        IDepartmentService departmentService,
        IPersonService personService,
        IDialogService dialogService,
        ILogger<TeacherDialogViewModel> logger) : base(hostScreen)
    {
        _teacherService = teacherService;
        _departmentService = departmentService;
        _personService = personService;
        _dialogService = dialogService;
        _logger = logger;

        LoadDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
        LoadPersonsCommand = ReactiveCommand.CreateFromTask(LoadPersonsAsync);
        CreatePersonCommand = ReactiveCommand.CreateFromTask(CreatePersonAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);

        InitializeCollections();
        InitializeValidation();
    }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadPersonsCommand { get; }
    public ReactiveCommand<Unit, Unit> CreatePersonCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchDepartmentsCommand { get; private set; } = null!;

    #endregion

    #region Initialization

    private void SetupCommands()
    {
        SearchDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
    }

    private async void LoadDataAsync()
    {
        await LoadDepartmentsAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания нового преподавателя
    /// </summary>
    public async Task InitializeAsync()
    {
        InitializeNew();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующего преподавателя
    /// </summary>
    public async Task InitializeAsync(Teacher teacher)
    {
        if (teacher?.Person == null)
        {
            // Если Person не загружен, загружаем его
            var fullTeacher = await _teacherService.GetByUidAsync(teacher.Uid);
            if (fullTeacher?.Person == null)
            {
                throw new InvalidOperationException($"Преподаватель с ID {teacher.Uid} не найден или не имеет связанного Person");
            }
            teacher = fullTeacher;
        }

        _currentTeacher = teacher;
        _currentPerson = teacher.Person;
        
        PopulateFromEntities(teacher, teacher.Person);
        
        Title = $"Редактирование преподавателя: {teacher.Person.FullName}";
        await Task.CompletedTask;
    }

    public void InitializeNew()
    {
        TeacherUid = Guid.NewGuid();
        Title = "Новый преподаватель";
        IsEditMode = false;
        
        Initialize();
    }

    #endregion

    #region Data Loading

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

    private async Task LoadPersonsAsync()
    {
        try
        {
            SetLoading(true, "Загрузка персон...");
            
            var persons = await _personService.GetAllAsync();
            
            AvailablePersons.Clear();
            foreach (var person in persons.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
            {
                AvailablePersons.Add(person);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading persons for teacher dialog");
            await _dialogService.ShowErrorAsync("Ошибка загрузки данных", ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task CreatePersonAsync()
    {
        try
        {
            SetLoading(true, "Создание персоны...");
            
            var newPerson = new Person
            {
                Uid = Guid.NewGuid(),
                FirstName = FirstName,
                LastName = LastName,
                MiddleName = MiddleName,
                Email = Email,
                Phone = Phone,
                DateOfBirth = DateOfBirth,
                Address = Address,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            var createdPerson = await _personService.CreateAsync(newPerson);
            if (createdPerson != null)
            {
                AvailablePersons.Add(createdPerson);
                _currentPerson = createdPerson;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person for teacher dialog");
            await _dialogService.ShowErrorAsync("Ошибка создания персоны", ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    #endregion

    #region Entity Management

    /// <summary>
    /// Загружает данные преподавателя для редактирования
    /// </summary>
    public async Task LoadTeacherAsync(Guid teacherUid)
    {
        try
        {
            var teacher = await _teacherService.GetByUidAsync(teacherUid);
            if (teacher?.Person == null)
            {
                throw new InvalidOperationException($"Преподаватель с ID {teacherUid} не найден или не имеет связанного Person");
            }

            _currentTeacher = teacher;
            _currentPerson = teacher.Person;
            
            PopulateFromEntities(teacher, teacher.Person);
            
            Title = $"Редактирование преподавателя: {teacher.Person.FullName}";
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки преподавателя {TeacherUid}");
            throw;
        }
    }

    /// <summary>
    /// Загружает данные преподавателя для редактирования (перегрузка для объекта Teacher)
    /// </summary>
    public async Task LoadTeacherAsync(Teacher teacher)
    {
        try
        {
            if (teacher == null)
            {
                throw new ArgumentNullException(nameof(teacher));
            }

            // A blank "new teacher" template (from the "Добавить преподавателя" button) has no
            // PersonUid yet and was never persisted, so GetByUidAsync(teacher.Uid) legitimately
            // returns null - that's the expected "create" case, not an error.
            if (teacher.PersonUid == Guid.Empty)
            {
                Title = "Новый преподаватель";
                return;
            }

            if (teacher.Person == null)
            {
                // Если Person не загружен, загружаем его
                var fullTeacher = await _teacherService.GetByUidAsync(teacher.Uid);
                if (fullTeacher?.Person == null)
                {
                    throw new InvalidOperationException($"Преподаватель с ID {teacher.Uid} не найден или не имеет связанного Person");
                }
                teacher = fullTeacher;
            }

            _currentTeacher = teacher;
            _currentPerson = teacher.Person;
            
            PopulateFromEntities(teacher, teacher.Person);
            
            Title = $"Редактирование преподавателя: {teacher.Person.FullName}";
        }
        catch (Exception ex)
        {
            LogError(ex, $"Ошибка загрузки преподавателя {TeacherUid}");
            throw;
        }
    }

    /// <summary>
    /// Заполняет форму данными из сущностей Person и Teacher
    /// </summary>
    private void PopulateFromEntities(Teacher teacher, Person person)
    {
        // Данные Person
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName ?? string.Empty;
        Email = person.Email;
        Phone = person.Phone ?? string.Empty;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address ?? string.Empty;

        // Данные Teacher
        EmployeeCode = teacher.EmployeeCode;
        HireDate = teacher.HireDate;
        TerminationDate = teacher.TerminationDate;
        Qualification = teacher.Qualification ?? string.Empty;
        Specialization = teacher.Specialization ?? string.Empty;
        Salary = teacher.Salary;
        DepartmentUid = teacher.DepartmentUid;

        // Установка выбранных элементов
        SelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Uid == teacher.DepartmentUid);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Валидирует данные преподавателя и Person
    /// </summary>
    protected bool Validate()
    {
        var errors = new List<string>();
        
        // Валидация Person
        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("Необходимо указать имя преподавателя");
            
        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Необходимо указать фамилию преподавателя");
            
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Необходимо указать email преподавателя");
        else if (!IsValidEmail(Email))
            errors.Add("Некорректный формат email");
            
        if (!string.IsNullOrWhiteSpace(Phone) && !IsValidPhone(Phone))
            errors.Add("Некорректный формат телефона");

        // Валидация Teacher
        if (string.IsNullOrWhiteSpace(EmployeeCode))
            errors.Add("Необходимо указать код сотрудника");

        if (HireDate > DateTime.Now)
            errors.Add("Дата найма не может быть в будущем");

        if (TerminationDate.HasValue && TerminationDate.Value <= HireDate)
            errors.Add("Дата увольнения должна быть позже даты найма");

        if (Salary < 0)
            errors.Add("Зарплата не может быть отрицательной");

        if (SelectedDepartment == null)
            errors.Add("Необходимо выбрать кафедру");
        
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
    /// Сохраняет преподавателя (создание или обновление)
    /// </summary>
    protected async Task<Teacher> SaveEntityAsync()
    {
        if (!Validate())
        {
            throw new ValidationException(ValidationError);
        }

        if (_currentTeacher == null)
        {
            // Создание нового преподавателя
            if (_currentPerson == null)
            {
                // Создать новую персону
                await CreatePersonAsync();
                if (_currentPerson == null)
                {
                    throw new InvalidOperationException("Не удалось создать персону для преподавателя");
                }
            }

            _currentTeacher = new Teacher
            {
                Uid = TeacherUid,
                PersonUid = _currentPerson.Uid,
                EmployeeCode = EmployeeCode,
                HireDate = HireDate,
                TerminationDate = TerminationDate,
                Qualification = Qualification,
                Specialization = Specialization,
                Salary = Salary,
                DepartmentUid = SelectedDepartment?.Uid,
                IsActive = IsActive,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
        }
        else
        {
            // Обновление существующего преподавателя
            _currentTeacher.EmployeeCode = EmployeeCode;
            _currentTeacher.HireDate = HireDate;
            _currentTeacher.TerminationDate = TerminationDate;
            _currentTeacher.Qualification = Qualification;
            _currentTeacher.Specialization = Specialization;
            _currentTeacher.Salary = Salary;
            _currentTeacher.DepartmentUid = SelectedDepartment?.Uid;
            _currentTeacher.IsActive = IsActive;
            _currentTeacher.LastModifiedAt = DateTime.UtcNow;
        }

        return _currentTeacher;
    }

    #endregion

    private void InitializeCollections()
    {
        // Инициализация коллекций
        AvailablePersons.Clear();
        AvailableDepartments.Clear();
    }

    private void InitializeValidation()
    {
        // Настройка валидации
        // TODO: Добавить правила валидации
    }

    private async Task SaveAsync()
    {
        try
        {
            if (Entity == null)
            {
                // Создание нового преподавателя
                var newTeacher = new Teacher
                {
                    Uid = TeacherUid,
                    PersonUid = SelectedPerson?.Uid ?? Guid.Empty,
                    DepartmentUid = SelectedDepartment?.Uid,
                    HireDate = HireDate,
                    IsActive = IsActive
                };

                await _teacherService.CreateAsync(newTeacher);
                ShowSuccess("Преподаватель успешно создан");
            }
            else
            {
                // Обновление существующего
                Entity.PersonUid = SelectedPerson?.Uid ?? Entity.PersonUid;
                Entity.DepartmentUid = SelectedDepartment?.Uid;
                Entity.HireDate = HireDate;
                Entity.IsActive = IsActive;

                await _teacherService.UpdateAsync(Entity);
                ShowSuccess("Преподаватель успешно обновлен");
            }

            // Закрыть диалог
            Cancel();
        }
        catch (Exception ex)
        {
            LogError($"Ошибка сохранения преподавателя: {ex.Message}");
            ShowError("Ошибка при сохранении преподавателя");
        }
    }

    private void Cancel()
    {
        // Закрыть диалог
        if (HostScreen?.Router != null)
        {
            _ = HostScreen.Router.NavigateBack.Execute();
        }
    }

    private void Initialize()
    {
        IsEditMode = true;
        Title = "Редактирование преподавателя";

        Initialize();
    }

    [Reactive] public string? ValidationError { get; set; }
    [Reactive] public string? Title { get; set; }
    [Reactive] public bool IsEditMode { get; set; }

    /// <summary>
    /// Текущая редактируемая сущность
    /// </summary>
    [Reactive] public Teacher? Entity { get; set; }

    /// <summary>
    /// UID преподавателя
    /// </summary>
    [Reactive] public Guid TeacherUid { get; set; }
} 

