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
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования оценки
    /// </summary>
    public class GradeEditorViewModel : ViewModelBase
    {
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly ITeacherService _teacherService;
        private readonly ObservableCollection<Student> _students = [];
        private readonly ObservableCollection<Assignment> _assignments = [];
        private readonly ObservableCollection<Teacher> _teachers = [];
        private readonly ObservableCollection<CourseInstance> _courseInstances = [];

        public ReadOnlyObservableCollection<Student> Students { get; }
        public ReadOnlyObservableCollection<Assignment> Assignments { get; }
        public ReadOnlyObservableCollection<Teacher> Teachers { get; }
        public ReadOnlyObservableCollection<CourseInstance> CourseInstances { get; }

        [Reactive] public decimal Value { get; set; }
        [Reactive] public string? Comment { get; set; }
        [Reactive] public DateTime IssuedAt { get; set; } = DateTime.Now;
        [Reactive] public Guid StudentUid { get; set; }
        [Reactive] public Guid AssignmentUid { get; set; }
        [Reactive] public Guid TeacherUid { get; set; }
        [Reactive] public Student? SelectedStudent { get; set; }
        [Reactive] public Assignment? SelectedAssignment { get; set; }
        [Reactive] public Teacher? SelectedTeacher { get; set; }

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public string LetterGrade { get; } = string.Empty;

        public string Title => Grade == null ? "Добавить оценку" : "Редактировать оценку";

        public Grade? Grade { get; set; }

        public ReactiveCommand<Unit, Grade?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        public GradeEditorViewModel(
            IStudentService studentService,
            IAssignmentService assignmentService,
            ITeacherService teacherService,
            Grade? grade = null)
        {
            _studentService = studentService;
            _assignmentService = assignmentService;
            _teacherService = teacherService;
            Grade = grade;

            Students = new ReadOnlyObservableCollection<Student>(_students);
            Assignments = new ReadOnlyObservableCollection<Assignment>(_assignments);
            Teachers = new ReadOnlyObservableCollection<Teacher>(_teachers);
            CourseInstances = new ReadOnlyObservableCollection<CourseInstance>(_courseInstances);

            // Инициализация из существующей оценки
            if (grade != null)
            {
                Value = grade.Value;
                Comment = grade.Comment ?? string.Empty;
                StudentUid = grade.StudentUid;
                AssignmentUid = grade.AssignmentUid ?? Guid.Empty;
                TeacherUid = grade.TeacherUid;
            }

            InitializeCommands();
            SetupSubscriptions();
            
            // Load data
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Validation
            var canSave = this.WhenAnyValue(
                x => x.Value,
                x => x.StudentUid,
                x => x.AssignmentUid,
                x => x.TeacherUid,
                (value, studentUid, assignmentUid, teacherUid) =>
                    value >= 0 &&
                    studentUid != Guid.Empty &&
                    assignmentUid != Guid.Empty &&
                    teacherUid != Guid.Empty
            );

            canSave.ToPropertyEx(this, x => x.IsValid);

            // Commands
            SaveCommand = CreateCommand(SaveAsync, canSave, "Ошибка сохранения оценки");
            CancelCommand = CreateSyncCommand(() => { }, null, "Ошибка отмены");
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Sync selected items with Uids
            this.WhenAnyValue(x => x.SelectedStudent)
                .Subscribe(student => StudentUid = student?.Uid ?? Guid.Empty);

            this.WhenAnyValue(x => x.SelectedAssignment)
                .Subscribe(assignment => 
                {
                    AssignmentUid = assignment?.Uid ?? Guid.Empty;
                });

            this.WhenAnyValue(x => x.SelectedTeacher)
                .Subscribe(teacher => TeacherUid = teacher?.Uid ?? Guid.Empty);

            // Вычисляемые свойства
            this.WhenAnyValue(x => x.Value, value => GetLetterGrade(value))
                .ToPropertyEx(this, x => x.LetterGrade);
        }

        /// <summary>
        /// Загружает данные для выпадающих списков
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                var students = await _studentService.GetAllAsync();
                _students.AddRange(students);

                var assignments = await _assignmentService.GetAllAsync();
                _assignments.AddRange(assignments);

                var teachers = await _teacherService.GetAllAsync();
                _teachers.AddRange(teachers);

                // Set selected items after data is loaded
                if (Grade != null)
                {
                    SelectedStudent = Students.FirstOrDefault(s => s.Uid == Grade.StudentUid);
                    SelectedAssignment = Assignments.FirstOrDefault(a => a.Uid == Grade.AssignmentUid);
                    SelectedTeacher = Teachers.FirstOrDefault(t => t.Uid == Grade.TeacherUid);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при загрузке данных");
            }
        }

        private async Task<Grade?> SaveAsync()
        {
            try
            {
                if (Grade == null)
                {
                    Grade = new Grade();
                }

                Grade.Value = Value;
                Grade.Comment = Comment;
                Grade.StudentUid = SelectedStudent?.Uid ?? Guid.Empty;
                Grade.AssignmentUid = SelectedAssignment?.Uid;
                Grade.TeacherUid = SelectedTeacher?.Uid ?? Guid.Empty;
                Grade.IssuedAt = DateTime.UtcNow;

                return Grade;
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при сохранении оценки");
                return null;
            }
        }

        /// <summary>
        /// Преобразует оценку в буквенную
        /// </summary>
        private static string GetLetterGrade(decimal value)
        {
            return value switch
            {
                >= 4.5m => "A",
                >= 3.5m => "B", 
                >= 2.5m => "C",
                >= 1.5m => "D",
                _ => "F"
            };
        }
    }
} 

