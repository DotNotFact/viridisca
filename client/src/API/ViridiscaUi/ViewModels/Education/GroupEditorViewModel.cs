using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования группы
    /// </summary>
    public class GroupEditorViewModel : ViewModelBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ObservableCollection<Teacher> _teachers = [];
        private readonly ReadOnlyObservableCollection<Teacher> _teachersReadOnly;
        private readonly ObservableCollection<Student> _students = new();

        public ReadOnlyObservableCollection<Teacher> Teachers => _teachersReadOnly;
        public ReadOnlyObservableCollection<Student> Students { get; }

        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Code { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public int Year { get; set; } = DateTime.Now.Year;
        [Reactive] public DateTime StartDate { get; set; } = DateTime.Today;
        [Reactive] public DateTime? EndDate { get; set; }
        [Reactive] public int MaxStudents { get; set; } = 30;
        [Reactive] public GroupStatus Status { get; set; } = GroupStatus.Forming;
        [Reactive] public Guid? CuratorUid { get; set; }
        [Reactive] public Teacher? SelectedCurator { get; set; }
        [Reactive] public Guid DepartmentUid { get; set; } = Guid.NewGuid();

        // Дополнительные свойства для отображения деталей
        [Reactive] public DateTime? PlannedEndDate { get; set; }
        [Reactive] public DateTime? ActualEndDate { get; set; }
        [Reactive] public bool IsActive { get; set; } = true;
        [Reactive] public CourseInstance? CourseInstance { get; set; }
        [Reactive] public Teacher? Curator { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }
        [Reactive] public DateTime UpdatedAt { get; set; }
        [Reactive] public Guid Uid { get; set; }

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }

        public string Title => Group == null ? "Добавить группу" : "Редактировать группу";

        public Group? Group { get; }

        public ReactiveCommand<Unit, Group?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> EditCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; private set; }

        public GroupEditorViewModel(ITeacherService teacherService, Group? group = null)
        {
            _teacherService = teacherService;
            Group = group;

            // Bind teachers collection first
            _teachersReadOnly = new ReadOnlyObservableCollection<Teacher>(_teachers);

            // Initialize properties from existing group if editing
            if (group != null)
            {
                Name = group.Name;
                Code = group.Code;
                Description = group.Description;
                Year = group.Year;
                StartDate = group.StartDate;
                EndDate = group.EndDate;
                MaxStudents = group.MaxStudents;
                Status = group.Status;
                CuratorUid = group.CuratorUid;
                DepartmentUid = group.DepartmentUid;
                
                // Дополнительные свойства для диалога деталей
                PlannedEndDate = group.EndDate;
                ActualEndDate = group.Status == GroupStatus.Completed ? group.EndDate : null;
                IsActive = group.Status == GroupStatus.Active || group.Status == GroupStatus.Forming;
                Curator = group.Curator;
                if (group.Students != null)
                {
                    _students.AddRange(group.Students);
                }
                CreatedAt = group.CreatedAt;
                UpdatedAt = group.LastModifiedAt;
                Uid = group.Uid;
            }

            Students = new ReadOnlyObservableCollection<Student>(_students);

            InitializeCommands();
            SetupSubscriptions();
            
            // Load teachers
            _ = LoadTeachersAsync();
        }

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Validation
            var canSave = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.MaxStudents,
                (name, code, maxStudents) =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.IsNullOrWhiteSpace(code) &&
                    maxStudents > 0 && maxStudents <= 100
            );

            canSave.ToPropertyEx(this, x => x.IsValid);

            // Создаем команды с обработкой ошибок
            SaveCommand = CreateCommand(SaveAsync, canSave, "Ошибка сохранения группы");
            CancelCommand = CreateSyncCommand(() => { }, null, "Ошибка отмены");
            EditCommand = CreateSyncCommand(() => { }, null, "Ошибка редактирования");
            CloseCommand = CreateSyncCommand(() => { }, null, "Ошибка закрытия");
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Sync CuratorUid and SelectedCurator
            this.WhenAnyValue(x => x.SelectedCurator)
                .Subscribe(curator => CuratorUid = curator?.Uid);
        }

        /// <summary>
        /// Загружает список преподавателей
        /// </summary>
        private async Task LoadTeachersAsync()
        {
            try
            {
                var teachers = await _teacherService.GetAllAsync();
                _teachers.AddRange(teachers);
                
                // Set selected curator if editing existing group
                if (Group?.CuratorUid != null)
                {
                    SelectedCurator = teachers.FirstOrDefault(t => t.Uid == Group.CuratorUid);
                }
            }
            catch (Exception ex)
            {
                SetError("Ошибка загрузки преподавателей", ex);
            }
        }

        private async Task<Group?> SaveAsync()
        {
            try
            {
                var group = Group ?? new Group();
                
                group.Name = Name;
                group.Code = Code;
                group.Description = Description;
                group.Year = Year;
                group.StartDate = StartDate;
                group.EndDate = EndDate;
                group.MaxStudents = MaxStudents;
                group.CuratorUid = SelectedCurator?.Uid;
                group.Status = Status;
                group.DepartmentUid = DepartmentUid;

                if (Group == null)
                {
                    group.Uid = Guid.NewGuid();
                    group.CreatedAt = DateTime.UtcNow;
                }
                
                group.LastModifiedAt = DateTime.UtcNow;

                return group;
            }
            catch (Exception ex)
            {
                SetError("Ошибка при сохранении группы", ex);
                return null;
            }
        }
    }
} 

