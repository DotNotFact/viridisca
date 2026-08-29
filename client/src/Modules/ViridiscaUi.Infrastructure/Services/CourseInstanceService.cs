using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с экземплярами курсов
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class CourseInstanceService : ICourseInstanceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CourseInstanceService> _logger;

    public CourseInstanceService(ApplicationDbContext dbContext, ILogger<CourseInstanceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Analytics Methods

    /// <summary>
    /// Получает аналитику экземпляра курса
    /// </summary>
    public async Task<CourseAnalytics> GetAnalyticsAsync(Guid courseInstanceUid)
    {
        try
        {
            var analytics = await _dbContext.CourseAnalytics
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Teacher)
                        .ThenInclude(t => t!.Person)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Enrollments)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Assignments)
                .FirstOrDefaultAsync(ca => ca.CourseInstanceUid == courseInstanceUid);

            if (analytics == null)
            {
                analytics = new CourseAnalytics
                {
                    CourseInstanceUid = courseInstanceUid,
                    LastCalculated = DateTime.UtcNow
                };

                _dbContext.CourseAnalytics.Add(analytics);
                await _dbContext.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _dbContext.CourseAnalytics
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Subject)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Teacher)
                            .ThenInclude(t => t!.Person)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Enrollments)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Assignments)
                    .FirstAsync(ca => ca.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно (каждый час)
            if (analytics.LastCalculated < DateTime.UtcNow.AddHours(-1))
            {
                analytics.RefreshCalculatedFields();
                await _dbContext.SaveChangesAsync();
            }

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instance analytics {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику экземпляра курса (исправленный тип возврата)
    /// </summary>
    public async Task<(int TotalInstances, int ActiveInstances, int TotalStudents, decimal AverageGrade)> GetCourseStatisticsAsync(Guid courseInstanceUid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances
                .Include(ci => ci.Enrollments)
                .Include(ci => ci.Subject)
                .FirstOrDefaultAsync(ci => ci.Uid == courseInstanceUid);

            if (courseInstance == null)
            {
                return (0, 0, 0, 0m);
            }

            // Получаем статистику для всех экземпляров этого курса
            var allInstances = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == courseInstance.SubjectUid)
                .ToListAsync();

            var totalInstances = allInstances.Count;
            var activeInstances = allInstances.Count(ci => ci.Status == CourseStatus.Active);

            // Статистика для конкретного экземпляра
            var totalStudents = courseInstance.Enrollments?.Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0;

            // Средняя оценка для этого экземпляра курса
            var averageGrade = 0m;
            if (totalStudents > 0)
            {
                var grades = await _dbContext.Grades
                    .Where(g => g.Assignment!.CourseInstanceUid == courseInstanceUid)
                    .Select(g => g.Value)
                    .ToListAsync();

                if (grades.Any())
                {
                    averageGrade = grades.Average();
                }
            }

            return (totalInstances, activeInstances, totalStudents, averageGrade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course statistics {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает прогресс экземпляра курса
    /// </summary>
    public async Task<(decimal ProgressPercentage, int CompletedLessons, int TotalLessons, int CompletedAssignments, int TotalAssignments)> GetProgressAsync(Guid courseInstanceUid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances
                .Include(ci => ci.Enrollments)
                .Include(ci => ci.Assignments)
                .Include(ci => ci.Lessons)
                .FirstOrDefaultAsync(ci => ci.Uid == courseInstanceUid);

            if (courseInstance == null)
            {
                return (0, 0, 0, 0, 0);
            }

            var totalLessons = courseInstance.Lessons?.Count ?? 0;
            var completedLessons = courseInstance.Lessons?.Count(l => l.Status == LessonStatus.Completed) ?? 0;

            var totalAssignments = courseInstance.Assignments?.Count ?? 0;
            var completedAssignments = await _dbContext.Submissions
                .Where(s => courseInstance.Assignments!.Select(a => a.Uid).Contains(s.AssignmentUid))
                .Where(s => s.Status == SubmissionStatus.Graded)
                .Select(s => s.AssignmentUid)
                .Distinct()
                .CountAsync();

            var progressPercentage = 0m;
            if (totalLessons > 0 || totalAssignments > 0)
            {
                var lessonProgress = totalLessons > 0 ? (decimal)completedLessons / totalLessons : 0;
                var assignmentProgress = totalAssignments > 0 ? (decimal)completedAssignments / totalAssignments : 0;
                progressPercentage = (lessonProgress + assignmentProgress) / 2 * 100;
            }

            return (progressPercentage, completedLessons, totalLessons, completedAssignments, totalAssignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course progress {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    #endregion

    #region Базовые CRUD операции

    /// <summary>
    /// Получает экземпляр курса по идентификатору
    /// </summary>
    public async Task<CourseInstance?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Include(ci => ci.Enrollments)
                .FirstOrDefaultAsync(ci => ci.Uid == uid && !ci.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instance {CourseInstanceUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все экземпляры курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetAllAsync()
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all course instances");
            throw;
        }
    }

    /// <summary>
    /// Создает новый экземпляр курса
    /// </summary>
    public async Task<CourseInstance> CreateAsync(CourseInstance courseInstance)
    {
        ArgumentNullException.ThrowIfNull(courseInstance);

        try
        {
            // Валидация
            await ValidateCourseInstanceAsync(courseInstance, true);

            courseInstance.Uid = Guid.NewGuid();
            courseInstance.CreatedAt = DateTime.UtcNow;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            _dbContext.CourseInstances.Add(courseInstance);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created course instance for subject {SubjectUid} and group {GroupUid}", 
                courseInstance.SubjectUid, courseInstance.GroupUid);
            return courseInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course instance");
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующий экземпляр курса
    /// </summary>
    public async Task<bool> UpdateAsync(CourseInstance courseInstance)
    {
        ArgumentNullException.ThrowIfNull(courseInstance);

        try
        {
            var existingCourseInstance = await _dbContext.CourseInstances.FindAsync(courseInstance.Uid);
            if (existingCourseInstance == null || existingCourseInstance.IsDeleted)
                return false;

            // Валидация
            await ValidateCourseInstanceAsync(courseInstance, false);

            // Обновляем поля
            existingCourseInstance.SubjectUid = courseInstance.SubjectUid;
            existingCourseInstance.GroupUid = courseInstance.GroupUid;
            existingCourseInstance.AcademicPeriodUid = courseInstance.AcademicPeriodUid;
            existingCourseInstance.TeacherUid = courseInstance.TeacherUid;
            existingCourseInstance.StartDate = courseInstance.StartDate;
            existingCourseInstance.EndDate = courseInstance.EndDate;
            existingCourseInstance.IsActive = courseInstance.IsActive;
            existingCourseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated course instance {CourseInstanceUid}", courseInstance.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course instance {CourseInstanceUid}", courseInstance.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет экземпляр курса
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances.FindAsync(uid);
            if (courseInstance == null || courseInstance.IsDeleted)
                return false;

            // Проверяем связанные данные
            var hasEnrollments = await _dbContext.Enrollments
                .AnyAsync(e => e.CourseInstanceUid == uid);

            if (hasEnrollments)
                throw new InvalidOperationException("Cannot delete course instance as it has enrollments");

            // Мягкое удаление
            courseInstance.IsDeleted = true;
            courseInstance.DeletedAt = DateTime.UtcNow;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted course instance {CourseInstanceUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course instance {CourseInstanceUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для экземпляров курсов

    /// <summary>
    /// Получает экземпляры курсов по группе
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByGroupAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.GroupUid == groupUid && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по преподавателю
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid)
    {
        try
        {
            var progress = await _dbContext.CourseInstances
                .Include(ci => ci.Enrollments)
                    .ThenInclude(e => e.Student)
                .Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherUid && !ci.IsDeleted)
                .Select(ci => new
                {
                    CourseInstance = ci,
                    CompletedEnrollments = ci.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                    TotalEnrollments = ci.Enrollments.Count
                })
                .FirstOrDefaultAsync();

            if (progress == null)
                return new List<CourseInstance>();

            return new List<CourseInstance> { progress.CourseInstance };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по предмету
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesBySubjectAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.SubjectUid == subjectUid && !ci.IsDeleted)
                .OrderBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for subject {SubjectUid}", subjectUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по академическому периоду
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByPeriodAsync(Guid academicPeriodUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.AcademicPeriodUid == academicPeriodUid && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for academic period {AcademicPeriodUid}", academicPeriodUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по студенту
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.Enrollments.Any(e => e.StudentUid == studentUid) && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает активные экземпляры курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetActiveCourseInstancesAsync()
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.IsActive && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active course instances");
            throw;
        }
    }

    /// <summary>
    /// Поиск экземпляров курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> SearchCourseInstancesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => !ci.IsDeleted && (
                    ci.Subject.Name.ToLower().Contains(lowerSearchTerm) ||
                    ci.Subject.Code.ToLower().Contains(lowerSearchTerm) ||
                    ci.Group.Name.ToLower().Contains(lowerSearchTerm) ||
                    ci.Group.Code.ToLower().Contains(lowerSearchTerm) ||
                    ci.Teacher != null && (
                        ci.Teacher.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                        ci.Teacher.Person.LastName.ToLower().Contains(lowerSearchTerm)
                    )
                ))
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching course instances with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов с пагинацией
    /// </summary>
    public async Task<(IEnumerable<CourseInstance> CourseInstances, int TotalCount)> GetCourseInstancesPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? groupUid = null, Guid? teacherUid = null, Guid? subjectUid = null)
    {
        try
        {
            var query = _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => !ci.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(ci => 
                    ci.Subject.Name.ToLower().Contains(lowerSearchTerm) ||
                    ci.Subject.Code.ToLower().Contains(lowerSearchTerm) ||
                    ci.Group.Name.ToLower().Contains(lowerSearchTerm) ||
                    ci.Group.Code.ToLower().Contains(lowerSearchTerm)
                );
            }

            if (groupUid.HasValue)
            {
                query = query.Where(ci => ci.GroupUid == groupUid.Value);
            }

            if (teacherUid.HasValue)
            {
                query = query.Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherUid.Value);
            }

            if (subjectUid.HasValue)
            {
                query = query.Where(ci => ci.SubjectUid == subjectUid.Value);
            }

            var totalCount = await query.CountAsync();

            var courseInstances = await query
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (courseInstances, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged course instances");
            throw;
        }
    }

    /// <summary>
    /// Запускает экземпляр курса
    /// </summary>
    public async Task<bool> StartCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances.FindAsync(courseInstanceUid);
            if (courseInstance == null || courseInstance.IsDeleted)
                return false;

            courseInstance.IsActive = true;
            courseInstance.StartDate = DateTime.UtcNow;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Started course instance {CourseInstanceUid}", courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting course instance {CourseInstanceUid}", courseInstanceUid);
            return false;
        }
    }

    /// <summary>
    /// Завершает экземпляр курса
    /// </summary>
    public async Task<bool> CompleteCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances.FindAsync(courseInstanceUid);
            if (courseInstance == null || courseInstance.IsDeleted)
                return false;

            courseInstance.IsActive = false;
            courseInstance.EndDate = DateTime.UtcNow;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Completed course instance {CourseInstanceUid}", courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing course instance {CourseInstanceUid}", courseInstanceUid);
            return false;
        }
    }

    /// <summary>
    /// Приостанавливает экземпляр курса
    /// </summary>
    public async Task<bool> SuspendCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            var courseInstance = await _dbContext.CourseInstances.FindAsync(courseInstanceUid);
            if (courseInstance == null || courseInstance.IsDeleted)
                return false;

            courseInstance.IsActive = false;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Suspended course instance {CourseInstanceUid}", courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending course instance {CourseInstanceUid}", courseInstanceUid);
            return false;
        }
    }

    /// <summary>
    /// Получает записанных студентов
    /// </summary>
    public async Task<IEnumerable<Student>> GetEnrolledStudentsAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Where(e => e.CourseInstanceUid == courseInstanceUid)
                .Select(e => e.Student)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrolled students for course instance {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Записывает студента на курс
    /// </summary>
    public async Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid)
    {
        try
        {
            // Проверяем, не записан ли уже студент
            var existingEnrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.CourseInstanceUid == courseInstanceUid && e.StudentUid == studentUid);

            if (existingEnrollment != null)
                return false; // Уже записан

            var enrollment = new Enrollment
            {
                Uid = Guid.NewGuid(),
                CourseInstanceUid = courseInstanceUid,
                StudentUid = studentUid,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            _dbContext.Enrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Enrolled student {StudentUid} to course instance {CourseInstanceUid}", studentUid, courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student {StudentUid} to course instance {CourseInstanceUid}", studentUid, courseInstanceUid);
            return false;
        }
    }

    /// <summary>
    /// Отписывает студента от курса
    /// </summary>
    public async Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid)
    {
        try
        {
            var enrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.CourseInstanceUid == courseInstanceUid && e.StudentUid == studentUid);

            if (enrollment == null)
                return false;

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unenrolled student {StudentUid} from course instance {CourseInstanceUid}", studentUid, courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unenrolling student {StudentUid} from course instance {CourseInstanceUid}", studentUid, courseInstanceUid);
            return false;
        }
    }

    /// <summary>
    /// Получает количество записанных студентов
    /// </summary>
    public async Task<int> GetEnrollmentCountAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .CountAsync(e => e.CourseInstanceUid == courseInstanceUid && e.Status == EnrollmentStatus.Enrolled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollment count for course instance {CourseInstanceUid}", courseInstanceUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает статистику экземпляра курса
    /// </summary>
    public async Task<CourseAnalytics> GetCourseInstanceStatisticsAsync(Guid courseInstanceUid)
    {
        try
        {
            // Получаем аналитику курса через StatisticsService
            var statisticsService = new StatisticsService(_dbContext);
            return await statisticsService.GetCourseAnalyticsAsync(courseInstanceUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики экземпляра курса {CourseInstanceUid}", courseInstanceUid);
            
            // Возвращаем пустую аналитику в случае ошибки
            return new CourseAnalytics
            {
                CourseInstanceUid = courseInstanceUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }
 
    /// <summary>
    /// Получает все экземпляры курсов с фильтрами
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync(
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null)
    {
        try
        {
            var query = _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => !ci.IsDeleted);

            if (subjectFilter.HasValue)
                query = query.Where(ci => ci.SubjectUid == subjectFilter.Value);

            if (teacherFilter.HasValue)
                query = query.Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherFilter.Value);

            if (groupFilter.HasValue)
                query = query.Where(ci => ci.GroupUid == groupFilter.Value);

            if (academicPeriodFilter.HasValue)
                query = query.Where(ci => ci.AcademicPeriodUid == academicPeriodFilter.Value);

            return await query
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all course instances with filters");
            throw;
        }
    }

    /// <summary>
    /// Клонирует экземпляр курса
    /// </summary>
    public async Task<CourseInstance?> CloneCourseInstanceAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid)
    {
        try
        {
            var originalCourseInstance = await GetByUidAsync(courseInstanceUid);
            if (originalCourseInstance == null)
                return null;

            var clonedCourseInstance = new CourseInstance
            {
                SubjectUid = originalCourseInstance.SubjectUid,
                GroupUid = originalCourseInstance.GroupUid,
                AcademicPeriodUid = newAcademicPeriodUid,
                TeacherUid = originalCourseInstance.TeacherUid,
                Name = originalCourseInstance.Name,
                Code = $"{originalCourseInstance.Code}_CLONE_{DateTime.UtcNow:yyyyMMdd}",
                Description = originalCourseInstance.Description,
                Notes = originalCourseInstance.Notes,
                MaxEnrollments = originalCourseInstance.MaxEnrollments,
                Status = CourseStatus.Draft, // Клон создается в статусе Draft
                IsActive = false, // Клон создается неактивным
                StartDate = DateTime.UtcNow,
                EndDate = null
            };

            return await CreateAsync(clonedCourseInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning course instance {CourseInstanceUid}", courseInstanceUid);
            return null;
        }
    }

    /// <summary>
    /// Получает доступные экземпляры курсов для студента
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.IsActive && !ci.IsDeleted &&
                           !ci.Enrollments.Any(e => e.StudentUid == studentUid))
                .OrderBy(ci => ci.Subject.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available course instances for student {StudentUid}", studentUid);
            throw;
        }
    }
 
    /// <summary>
    /// Получает рекомендованные экземпляры курсов для студента
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid)
    {
        try
        {
            var student = await _dbContext.Students
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .FirstOrDefaultAsync(s => s.Uid == studentUid && !s.IsDeleted);

            if (student == null)
                return new List<CourseInstance>();

            // Рекомендуем курсы для группы студента
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.AcademicPeriod)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(ci => ci.GroupUid == student.GroupUid && 
                           ci.IsActive && 
                           !ci.IsDeleted &&
                           !ci.Enrollments.Any(e => e.StudentUid == studentUid))
                .OrderBy(ci => ci.Subject.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended course instances for student {StudentUid}", studentUid);
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация экземпляра курса
    /// </summary>
    private async Task ValidateCourseInstanceAsync(CourseInstance courseInstance, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (courseInstance.SubjectUid == Guid.Empty)
            errors.Add("Subject is required for course instance");

        if (courseInstance.GroupUid == Guid.Empty)
            errors.Add("Group is required for course instance");

        if (courseInstance.AcademicPeriodUid == Guid.Empty)
            errors.Add("Academic period is required for course instance");

        // Проверка существования связанных сущностей
        if (courseInstance.SubjectUid != Guid.Empty)
        {
            var subjectExists = await _dbContext.Subjects
                .AnyAsync(s => s.Uid == courseInstance.SubjectUid && !s.IsDeleted);

            if (!subjectExists)
                errors.Add($"Subject with Uid {courseInstance.SubjectUid} not found");
        }

        if (courseInstance.GroupUid != Guid.Empty)
        {
            var groupExists = await _dbContext.Groups
                .AnyAsync(g => g.Uid == courseInstance.GroupUid && !g.IsDeleted);

            if (!groupExists)
                errors.Add($"Group with Uid {courseInstance.GroupUid} not found");
        }

        if (courseInstance.AcademicPeriodUid != Guid.Empty)
        {
            var periodExists = await _dbContext.AcademicPeriods
                .AnyAsync(ap => ap.Uid == courseInstance.AcademicPeriodUid && !ap.IsDeleted);

            if (!periodExists)
                errors.Add($"Academic period with Uid {courseInstance.AcademicPeriodUid} not found");
        }

        if (courseInstance.TeacherUid.HasValue && courseInstance.TeacherUid != Guid.Empty)
        {
            var teacherExists = await _dbContext.Teachers
                .AnyAsync(t => t.Uid == courseInstance.TeacherUid.Value && !t.IsDeleted);

            if (!teacherExists)
                errors.Add($"Teacher with Uid {courseInstance.TeacherUid.Value} not found");
        }

        // Проверка уникальности комбинации Subject + Group + AcademicPeriod
        if (courseInstance.SubjectUid != Guid.Empty && courseInstance.GroupUid != Guid.Empty && courseInstance.AcademicPeriodUid != Guid.Empty)
        {
            var duplicateExists = await _dbContext.CourseInstances
                .Where(ci => ci.Uid != courseInstance.Uid && 
                            ci.SubjectUid == courseInstance.SubjectUid &&
                            ci.GroupUid == courseInstance.GroupUid &&
                            ci.AcademicPeriodUid == courseInstance.AcademicPeriodUid &&
                            !ci.IsDeleted)
                .AnyAsync();

            if (duplicateExists)
                errors.Add("Course instance for this combination of subject, group and academic period already exists");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    #endregion

    /// <summary>
    /// Получает экземпляр курса по идентификатору (алиас)
    /// </summary>
    public async Task<CourseInstance?> GetCourseInstanceAsync(Guid uid)
    {
        return await GetByUidAsync(uid);
    }

    /// <summary>
    /// Получает курсы студента (алиас)
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid)
    {
        return await GetCourseInstancesByStudentAsync(studentUid);
    }

    /// <summary>
    /// Получает курс по идентификатору (алиас)
    /// </summary>
    public async Task<CourseInstance?> GetCourseAsync(Guid uid)
    {
        return await GetByUidAsync(uid);
    }

    /// <summary>
    /// Получает все курсы (алиас)
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetAllCoursesAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Клонирует курс (алиас)
    /// </summary>
    public async Task<CourseInstance?> CloneCourseAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid)
    {
        return await CloneCourseInstanceAsync(courseInstanceUid, newAcademicPeriodUid);
    }

    /// <summary>
    /// Удаляет экземпляр курса (алиас)
    /// </summary>
    public async Task<bool> DeleteCourseInstanceAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    /// <summary>
    /// Получает экземпляры курсов по преподавателю (алиас для GetCourseInstancesByTeacherAsync)
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid)
    {
        return await GetCourseInstancesByTeacherAsync(teacherUid);
    }

    /// <summary>
    /// Получает неназначенные экземпляры курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetUnassignedAsync()
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.TeacherUid == null && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unassigned course instances");
            throw;
        }
    }
} 