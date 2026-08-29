using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования предмета
    /// </summary>
    public class SubjectEditorViewModel : ViewModelBase
    {
        private readonly IDepartmentService? _departmentService;

        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Code { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public bool IsActive { get; set; } = true;
        [Reactive] public int Credits { get; set; } = 3;
        [Reactive] public int LessonsPerWeek { get; set; } = 2;
        [Reactive] public Guid? DepartmentUid { get; set; }
        [Reactive] public Department? SelectedDepartment { get; set; }
        [Reactive] public SubjectType SelectedSubjectType { get; set; } = SubjectType.Required;
        [Reactive] public string? SelectedKnowledgeArea { get; set; }
        [Reactive] public string? ValidationError { get; set; }

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public bool CanSave { get; }

        public string Title => Subject == null ? "Добавить предмет" : "Редактировать предмет";

        private Subject? Subject { get; set; }
        public Subject? Result { get; set; }

        public ReactiveCommand<Unit, Subject?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; private set; }

        // Список доступных департаментов
        public ObservableCollection<Department> AvailableDepartments { get; } = new();

        // Типы предметов из enum
        public ObservableCollection<SubjectType> SubjectTypes { get; } = new()
        {
            SubjectType.Required,
            SubjectType.Elective,
            SubjectType.Specialized,
            SubjectType.Practicum,
            SubjectType.Seminar,
            SubjectType.Laboratory,
            SubjectType.Lecture
        };

        // Предопределенные области знаний
        public ObservableCollection<string> KnowledgeAreas { get; } = new()
        {
            "Информационные технологии",
            "Математика",
            "Физика",
            "Химия",
            "Биология",
            "Гуманитарные науки",
            "Экономика",
            "Менеджмент",
            "Иностранные языки",
            "Искусство и дизайн"
        };

        public SubjectEditorViewModel(IDepartmentService? departmentService = null, Subject? subject = null)
        {
            _departmentService = departmentService;
            Subject = subject;

            // Инициализация из существующего предмета
            if (subject != null)
            {
                Name = subject.Name;
                Code = subject.Code;
                Description = subject.Description;
                IsActive = subject.IsActive;
                Credits = subject.Credits;
                LessonsPerWeek = subject.LessonsPerWeek;
                DepartmentUid = subject.DepartmentUid;
                SelectedSubjectType = subject.Type;
            }
            else
            {
                // Значения по умолчанию для нового предмета
                Code = GenerateSubjectCode();
                SelectedSubjectType = SubjectType.Required;
            }

            InitializeCommands();
            InitializeValidation();

            // Загружаем департаменты при инициализации, если сервис доступен
            if (_departmentService != null)
            {
                LoadDepartmentsCommand.Execute().Subscribe(_ => { }, _ => { });
            }
        }

        private void InitializeCommands()
        {
            // Команда загрузки департаментов
            LoadDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
            LoadDepartmentsCommand.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

            // Команда сохранения
            var canSave = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.Credits,
                x => x.LessonsPerWeek,
                x => x.IsValid,
                (name, code, credits, lessons, isValid) =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.IsNullOrWhiteSpace(code) &&
                    credits > 0 &&
                    lessons > 0 &&
                    isValid
            );

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
            canSave.ToPropertyEx(this, x => x.CanSave);

            // Команда отмены
            CancelCommand = ReactiveCommand.Create(() => { Result = null; });

            // Обновляем выбранный департамент при изменении DepartmentUid
            this.WhenAnyValue(x => x.DepartmentUid)
                .Subscribe(uid =>
                {
                    SelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Uid == uid);
                });

            // Обновляем DepartmentUid при изменении выбранного департамента
            this.WhenAnyValue(x => x.SelectedDepartment)
                .Subscribe(department =>
                {
                    DepartmentUid = department?.Uid == Guid.Empty ? null : department?.Uid;
                });
        }

        private void InitializeValidation()
        {
            // Валидация полей
            var isNameValid = this.WhenAnyValue(x => x.Name)
                .Select(name => !string.IsNullOrWhiteSpace(name) && name.Length >= 2);

            var isCodeValid = this.WhenAnyValue(x => x.Code)
                .Select(code => !string.IsNullOrWhiteSpace(code) && code.Length >= 2);

            var areCreditsValid = this.WhenAnyValue(x => x.Credits)
                .Select(credits => credits > 0 && credits <= 20);

            var areLessonsValid = this.WhenAnyValue(x => x.LessonsPerWeek)
                .Select(lessons => lessons > 0 && lessons <= 40);

            // Общая валидация
            Observable.CombineLatest(
                isNameValid,
                isCodeValid,
                areCreditsValid,
                areLessonsValid,
                (nameValid, codeValid, creditsValid, lessonsValid) => 
                    nameValid && codeValid && creditsValid && lessonsValid)
                .ToPropertyEx(this, x => x.IsValid);
        }

        private async Task LoadDepartmentsAsync()
        {
            if (_departmentService == null) return;

            try
            {
                var departments = await _departmentService.GetActiveDepartmentsAsync();
                AvailableDepartments.Clear();
                
                // Добавляем опцию "Без департамента"
                AvailableDepartments.Add(new Department 
                { 
                    Uid = Guid.Empty, 
                    Name = "Без департамента",
                    Code = "NONE",
                    Description = "Предмет не привязан к конкретному департаменту"
                });

                foreach (var department in departments)
                {
                    AvailableDepartments.Add(department);
                }

                // Устанавливаем выбранный департамент если редактируем существующий предмет
                if (Subject?.DepartmentUid != null)
                {
                    SelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Uid == Subject.DepartmentUid);
                }
                else if (Subject?.DepartmentUid == null)
                {
                    // Если департамент не указан, выбираем "Без департамента"
                    SelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Uid == Guid.Empty);
                }
            }
            catch (Exception ex)
            {
                SetError("Ошибка загрузки департаментов", ex);
            }
        }

        private async Task<Subject?> SaveAsync()
        {
            try
            {
                // Валидация перед сохранением
                ValidationError = null;
                
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ValidationError = "Название предмета обязательно";
                    return null;
                }
                
                if (string.IsNullOrWhiteSpace(Code))
                {
                    ValidationError = "Код предмета обязателен";
                    return null;
                }
                
                if (Credits <= 0 || Credits > 20)
                {
                    ValidationError = "Количество кредитов должно быть от 1 до 20";
                    return null;
                }
                
                if (LessonsPerWeek <= 0 || LessonsPerWeek > 40)
                {
                    ValidationError = "Количество занятий в неделю должно быть от 1 до 40";
                    return null;
                }

                var subject = Subject ?? new Subject();
                
                subject.Name = Name.Trim();
                subject.Code = Code.Trim();
                subject.Description = Description?.Trim() ?? string.Empty;
                subject.IsActive = IsActive;
                subject.Credits = Credits;
                subject.LessonsPerWeek = LessonsPerWeek;
                subject.DepartmentUid = DepartmentUid == Guid.Empty ? null : DepartmentUid;
                subject.Type = SelectedSubjectType;

                if (Subject == null)
                {
                    subject.Uid = Guid.NewGuid();
                    subject.CreatedAt = DateTime.UtcNow;
                }
                
                subject.LastModifiedAt = DateTime.UtcNow;

                Result = subject;
                return subject;
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
                return null;
            }
        }

        private string GenerateSubjectCode()
        {
            // Генерируем код предмета в формате S + год + случайное число
            var year = DateTime.Now.Year.ToString().Substring(2);
            var random = new Random().Next(100, 999);
            return $"S{year}{random}";
        }
    }
} 

