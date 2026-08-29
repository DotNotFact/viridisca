using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Entities.Library;

namespace ViridiscaUi.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Auth
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<PersonRole> PersonRoles { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    // Academic
    public DbSet<AcademicPeriod> AcademicPeriods { get; set; } = null!;
    public DbSet<Curriculum> Curricula { get; set; } = null!;
    public DbSet<CurriculumSubject> CurriculumSubjects { get; set; } = null!;
    public DbSet<CourseInstance> CourseInstances { get; set; } = null!;
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; } = null!;

    // Education
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<LessonProgress> LessonProgresses { get; set; } = null!;

    // Exam
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<ExamResult> ExamResults { get; set; } = null!;

    // Quiz System
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
    public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
    public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
    public DbSet<QuizStudentAnswer> QuizStudentAnswers { get; set; } = null!;

    // Discussion System
    public DbSet<Discussion> Discussions { get; set; } = null!;
    public DbSet<DiscussionPost> DiscussionPosts { get; set; } = null!;
    public DbSet<PostLike> PostLikes { get; set; } = null!;

    // Analytics
    public DbSet<StudentAnalytics> StudentAnalytics { get; set; } = null!;
    public DbSet<CourseAnalytics> CourseAnalytics { get; set; } = null!;
    public DbSet<AssignmentAnalytics> AssignmentAnalytics { get; set; } = null!;
    public DbSet<GroupAnalytics> GroupAnalytics { get; set; } = null!;
    public DbSet<TeacherAnalytics> TeacherAnalytics { get; set; } = null!;
    public DbSet<SubjectAnalytics> SubjectAnalytics { get; set; } = null!;
    public DbSet<LessonAnalytics> LessonAnalytics { get; set; } = null!;
    public DbSet<ExamAnalytics> ExamAnalytics { get; set; } = null!;
    public DbSet<QuizAnalytics> QuizAnalytics { get; set; } = null!;

    // Library
    public DbSet<LibraryResource> LibraryResources { get; set; } = null!;
    public DbSet<LibraryLoan> LibraryLoans { get; set; } = null!;
    
    // System
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Faculty> Faculties { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationSettings> NotificationSettings { get; set; } = null!;
    public DbSet<FileRecord> FileRecords { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Подавляем предупреждение о динамических значениях в seed данных
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =================================================================================
        // === AUTH MODELS CONFIGURATION =================================================
        // =================================================================================

        #region Auth Models

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasOne(a => a.Person)
                .WithOne(p => p.Account)
                .HasForeignKey<Account>(a => a.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        modelBuilder.Entity<PersonRole>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.PersonUid, e.RoleUid, e.ContextEntityUid, e.ContextEntityType });

            entity.HasOne(pr => pr.Person)
                .WithMany(p => p.PersonRoles)
                .HasForeignKey(pr => pr.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pr => pr.Role)
                .WithMany(r => r.PersonRoles)
                .HasForeignKey(pr => pr.RoleUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleUid, rp.PermissionUid });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        // =================================================================================
        // === ACADEMIC MODELS CONFIGURATION ==============================================
        // =================================================================================
        
        #region Academic Models

        modelBuilder.Entity<AcademicPeriod>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.AcademicYear, e.Type });
        });

        modelBuilder.Entity<Curriculum>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(c => c.Department)
                .WithMany()
                .HasForeignKey(c => c.DepartmentUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CurriculumSubject>(entity =>
        {
            entity.HasKey(cs => new { cs.CurriculumUid, cs.SubjectUid });
            
            entity.HasOne(cs => cs.Curriculum)
                .WithMany(c => c.CurriculumSubjects)
                .HasForeignKey(cs => cs.CurriculumUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cs => cs.Subject)
                .WithMany(s => s.CurriculumSubjects)
                .HasForeignKey(cs => cs.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<CourseInstance>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.SubjectUid, e.GroupUid, e.AcademicPeriodUid });
            
            entity.HasOne(ci => ci.Subject)
                .WithMany(s => s.CourseInstances)
                .HasForeignKey(ci => ci.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ci => ci.Group)
                .WithMany()
                .HasForeignKey(ci => ci.GroupUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ci => ci.AcademicPeriod)
                .WithMany(ap => ap.CourseInstances)
                .HasForeignKey(ci => ci.AcademicPeriodUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Teacher)
                .WithMany(t => t.CourseInstances)
                .HasForeignKey(ci => ci.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ScheduleSlot>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.CourseInstanceUid, e.DayOfWeek, e.StartTime });
            
            entity.HasOne(ss => ss.CourseInstance)
                .WithMany(ci => ci.ScheduleSlots)
                .HasForeignKey(ss => ss.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        // =================================================================================
        // === EDUCATION MODELS CONFIGURATION ==============================================
        // =================================================================================
        
        #region Education Models

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.StudentCode).IsUnique();

            entity.HasOne(s => s.Person)
                .WithOne()
                .HasForeignKey<Student>(s => s.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Group)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.GroupUid)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(s => s.Curriculum)
                .WithMany()
                .HasForeignKey(s => s.CurriculumUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            
            entity.HasOne(t => t.Person)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(t => t.Department)
                .WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentUid)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(g => g.Department)
                .WithMany(d => d.Groups)
                .HasForeignKey(g => g.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(g => g.CuratorUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(s => s.Department)
                .WithMany(d => d.Subjects)
                .HasForeignKey(s => s.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Uid);
            
            entity.HasOne(l => l.CourseInstance)
                .WithMany(ci => ci.Lessons)
                .HasForeignKey(l => l.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с тестами
            entity.HasMany(l => l.Quizzes)
                .WithOne(q => q.Lesson)
                .HasForeignKey(q => q.LessonUid)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            
            entity.HasOne(a => a.CourseInstance)
                .WithMany(ci => ci.Assignments)
                .HasForeignKey(a => a.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Uid);

            entity.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.StudentUid, e.CourseInstanceUid }).IsUnique();
            
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CourseInstance)
                .WithMany(ci => ci.Enrollments)
                .HasForeignKey(e => e.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Uid);

            entity.HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(g => g.CourseInstance)
                .WithMany()
                .HasForeignKey(g => g.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Assignment)
                .WithMany()
                .HasForeignKey(g => g.AssignmentUid)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(g => g.Teacher)
                .WithMany()
                .HasForeignKey(g => g.TeacherUid)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(g => g.Subject)
                .WithMany()
                .HasForeignKey(g => g.SubjectUid)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(g => g.Exam)
                .WithMany()
                .HasForeignKey(g => g.ExamUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.StudentUid, e.LessonUid }).IsUnique();

            entity.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Lesson)
                .WithMany()
                .HasForeignKey(a => a.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.StudentUid, e.LessonUid }).IsUnique();

            entity.HasOne(lp => lp.Student)
                .WithMany()
                .HasForeignKey(lp => lp.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgress)
                .HasForeignKey(lp => lp.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        // =================================================================================
        // === EXAM MODELS CONFIGURATION ================================================
        // =================================================================================
        
        #region Exam Models

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Uid);
            
            entity.HasOne(e => e.CourseInstance)
                .WithMany()
                .HasForeignKey(e => e.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AcademicPeriod)
                .WithMany(ap => ap.Exams)
                .HasForeignKey(e => e.AcademicPeriodUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.ExamUid, e.StudentUid }).IsUnique();

            entity.HasOne(er => er.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExamUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        // =================================================================================
        // === ANALYTICS CONFIGURATION ====================================================
        // =================================================================================
        
        #region Analytics

        modelBuilder.Entity<StudentAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.StudentUid, e.AcademicPeriodUid }).IsUnique();
            
            entity.HasOne(sa => sa.Student)
                .WithMany(s => s.Analytics)
                .HasForeignKey(sa => sa.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.AcademicPeriod)
                .WithMany()
                .HasForeignKey(sa => sa.AcademicPeriodUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.CourseInstanceUid).IsUnique();
            
            entity.HasOne(ca => ca.CourseInstance)
                .WithMany(ci => ci.Analytics)
                .HasForeignKey(ca => ca.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssignmentAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.AssignmentUid).IsUnique();
            
            entity.HasOne(aa => aa.Assignment)
                .WithMany(a => a.Analytics)
                .HasForeignKey(aa => aa.AssignmentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.GroupUid).IsUnique();
            
            entity.HasOne(ga => ga.Group)
                .WithMany(g => g.Analytics)
                .HasForeignKey(ga => ga.GroupUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeacherAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.TeacherUid, e.AcademicPeriodUid }).IsUnique();
            
            entity.HasOne(ta => ta.Teacher)
                .WithMany(t => t.Analytics)
                .HasForeignKey(ta => ta.TeacherUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ta => ta.AcademicPeriod)
                .WithMany()
                .HasForeignKey(ta => ta.AcademicPeriodUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubjectAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.SubjectUid).IsUnique();
            
            entity.HasOne(sa => sa.Subject)
                .WithMany(s => s.Analytics)
                .HasForeignKey(sa => sa.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.LessonUid).IsUnique();
            
            entity.HasOne(la => la.Lesson)
                .WithMany(l => l.Analytics)
                .HasForeignKey(la => la.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.ExamUid).IsUnique();
            
            entity.HasOne(ea => ea.Exam)
                .WithMany(e => e.Analytics)
                .HasForeignKey(ea => ea.ExamUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAnalytics>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.QuizUid).IsUnique();
            
            entity.HasOne(qa => qa.Quiz)
                .WithMany(q => q.Analytics)
                .HasForeignKey(qa => qa.QuizUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        // =================================================================================
        // === LIBRARY MODELS CONFIGURATION ================================================
        // =================================================================================
        
        #region Library Models

        modelBuilder.Entity<LibraryResource>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.ISBN);
        });

        modelBuilder.Entity<LibraryLoan>(entity =>
        {
            entity.HasKey(e => e.Uid);

            entity.HasOne(ll => ll.Resource)
                .WithMany(lr => lr.Loans)
                .HasForeignKey(ll => ll.ResourceUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ll => ll.Person)
                .WithMany() // У человека может быть много займов
                .HasForeignKey(ll => ll.PersonUid)
                .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить человека, если у него есть активные займы
        });

        #endregion

        // =================================================================================
        // === SYSTEM MODELS CONFIGURATION =================================================
        // =================================================================================
        
        #region System Models

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
        });
        
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(f => f.Dean)
                .WithMany()
                .HasForeignKey(f => f.DeanUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.PersonUid, e.CreatedAt });
            
            entity.HasOne(n => n.Person)
                .WithMany()
                .HasForeignKey(n => n.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Template)
                .WithMany()
                .HasForeignKey(n => n.TemplateUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<NotificationSettings>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.PersonUid).IsUnique();
            
            entity.HasOne(ns => ns.Person)
                .WithOne()
                .HasForeignKey<NotificationSettings>(ns => ns.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.FilePath);
            
            entity.HasOne(fr => fr.UploadedBy)
                .WithMany()
                .HasForeignKey(fr => fr.UploadedByUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        #endregion

        // =================================================================================
        // === SEED DATA CONFIGURATION ===================================================
        // =================================================================================
        
        #region Seed Data

        SeedRoles(modelBuilder);
        SeedPermissions(modelBuilder);
        SeedRolePermissions(modelBuilder);
        SeedDepartments(modelBuilder);
        SeedSystemSettings(modelBuilder);
        SeedNotificationTemplates(modelBuilder);
        SeedSystemAdministrator(modelBuilder);
        SeedAcademicPeriods(modelBuilder);
        SeedSampleData(modelBuilder);

        #endregion

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(c => c.Department)
                .WithMany()
                .HasForeignKey(c => c.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =================================================================================
        // === QUIZ SYSTEM CONFIGURATION ==================================================
        // =================================================================================
        
        #region Quiz System

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.CourseInstanceUid, e.Title });
            
            entity.HasOne(q => q.CourseInstance)
                .WithMany()
                .HasForeignKey(q => q.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(q => q.Lesson)
                .WithMany()
                .HasForeignKey(q => q.LessonUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.QuizUid, e.Order });
            
            entity.HasOne(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAnswer>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.QuestionUid, e.Order });
            
            entity.HasOne(qa => qa.Question)
                .WithMany(qq => qq.Answers)
                .HasForeignKey(qa => qa.QuestionUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.QuizUid, e.StudentUid, e.AttemptNumber }).IsUnique();
            
            entity.HasOne(qa => qa.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(qa => qa.QuizUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizStudentAnswer>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.AttemptUid, e.QuestionUid }).IsUnique();
            
            entity.HasOne(qsa => qsa.Attempt)
                .WithMany(qa => qa.StudentAnswers)
                .HasForeignKey(qsa => qsa.AttemptUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(qsa => qsa.Question)
                .WithMany()
                .HasForeignKey(qsa => qsa.QuestionUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(qsa => qsa.SelectedAnswer)
                .WithMany()
                .HasForeignKey(qsa => qsa.SelectedAnswerUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        // =================================================================================
        // === DISCUSSION SYSTEM CONFIGURATION ============================================
        // =================================================================================
        
        #region Discussion System

        modelBuilder.Entity<Discussion>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.CourseInstanceUid, e.Title });
            
            entity.HasOne(d => d.CourseInstance)
                .WithMany()
                .HasForeignKey(d => d.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DiscussionPost>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.DiscussionUid, e.CreatedAt });
            
            entity.HasOne(dp => dp.Discussion)
                .WithMany(d => d.Posts)
                .HasForeignKey(dp => dp.DiscussionUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dp => dp.Author)
                .WithMany()
                .HasForeignKey(dp => dp.AuthorUid)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dp => dp.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(dp => dp.ParentPostUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => new { e.PostUid, e.PersonUid }).IsUnique();
            
            entity.HasOne(pl => pl.Post)
                .WithMany(dp => dp.Likes)
                .HasForeignKey(pl => pl.PostUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pl => pl.Person)
                .WithMany()
                .HasForeignKey(pl => pl.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion
    }

    #region Seed Data Methods

    /// <summary>
    /// Создание начальных ролей системы
    /// </summary>
    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var roles = new[]
        {
            new Role { Uid = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "SystemAdmin", DisplayName = "Системный администратор", Description = "Полный доступ ко всем функциям системы", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "AcademicAffairsHead", DisplayName = "Начальник учебной части", Description = "Управление академическими процессами", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "DepartmentHead", DisplayName = "Заведующий кафедрой", Description = "Управление кафедрой и её ресурсами", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Teacher", DisplayName = "Преподаватель", Description = "Ведение занятий и оценивание студентов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "AssistantTeacher", DisplayName = "Ассистент преподавателя", Description = "Помощь в ведении занятий", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Student", DisplayName = "Студент", Description = "Обучение в системе", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Parent", DisplayName = "Родитель", Description = "Просмотр успеваемости ребёнка", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Librarian", DisplayName = "Библиотекарь", Description = "Управление библиотечными ресурсами", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "ITSupport", DisplayName = "IT поддержка", Description = "Техническая поддержка системы", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Role { Uid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Accountant", DisplayName = "Бухгалтер", Description = "Финансовые операции", CreatedAt = baseDate, LastModifiedAt = baseDate }
        };

        modelBuilder.Entity<Role>().HasData(roles);
    }

    /// <summary>
    /// Создание начальных прав доступа
    /// </summary>
    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var permissions = new[]
        {
            // Системные права
            new Permission { Uid = Guid.Parse("10000001-0000-0000-0000-000000000001"), Name = "SystemAdmin.FullAccess", DisplayName = "Полный доступ к системе", Description = "Неограниченный доступ ко всем функциям", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("10000002-0000-0000-0000-000000000002"), Name = "Users.View", DisplayName = "Просмотр пользователей", Description = "Просмотр списка пользователей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("10000003-0000-0000-0000-000000000003"), Name = "Users.Create", DisplayName = "Создание пользователей", Description = "Создание новых пользователей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("10000004-0000-0000-0000-000000000004"), Name = "Users.Edit", DisplayName = "Редактирование пользователей", Description = "Изменение данных пользователей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("10000005-0000-0000-0000-000000000005"), Name = "Users.Delete", DisplayName = "Удаление пользователей", Description = "Удаление пользователей из системы", CreatedAt = baseDate, LastModifiedAt = baseDate },

            // Права на студентов
            new Permission { Uid = Guid.Parse("20000001-0000-0000-0000-000000000001"), Name = "Students.View", DisplayName = "Просмотр студентов", Description = "Просмотр списка студентов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("20000002-0000-0000-0000-000000000002"), Name = "Students.Create", DisplayName = "Создание студентов", Description = "Регистрация новых студентов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("20000003-0000-0000-0000-000000000003"), Name = "Students.Edit", DisplayName = "Редактирование студентов", Description = "Изменение данных студентов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("20000004-0000-0000-0000-000000000004"), Name = "Students.Delete", DisplayName = "Удаление студентов", Description = "Удаление студентов из системы", CreatedAt = baseDate, LastModifiedAt = baseDate },

            // Права на преподавателей
            new Permission { Uid = Guid.Parse("30000001-0000-0000-0000-000000000001"), Name = "Teachers.View", DisplayName = "Просмотр преподавателей", Description = "Просмотр списка преподавателей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("30000002-0000-0000-0000-000000000002"), Name = "Teachers.Create", DisplayName = "Создание преподавателей", Description = "Регистрация новых преподавателей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("30000003-0000-0000-0000-000000000003"), Name = "Teachers.Edit", DisplayName = "Редактирование преподавателей", Description = "Изменение данных преподавателей", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("30000004-0000-0000-0000-000000000004"), Name = "Teachers.Delete", DisplayName = "Удаление преподавателей", Description = "Удаление преподавателей из системы", CreatedAt = baseDate, LastModifiedAt = baseDate },

            // Права на курсы и предметы
            new Permission { Uid = Guid.Parse("40000001-0000-0000-0000-000000000001"), Name = "Courses.View", DisplayName = "Просмотр курсов", Description = "Просмотр списка курсов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("40000002-0000-0000-0000-000000000002"), Name = "Courses.Create", DisplayName = "Создание курсов", Description = "Создание новых курсов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("40000003-0000-0000-0000-000000000003"), Name = "Courses.Edit", DisplayName = "Редактирование курсов", Description = "Изменение курсов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("40000004-0000-0000-0000-000000000004"), Name = "Courses.Delete", DisplayName = "Удаление курсов", Description = "Удаление курсов", CreatedAt = baseDate, LastModifiedAt = baseDate },

            // Права на оценки
            new Permission { Uid = Guid.Parse("50000001-0000-0000-0000-000000000001"), Name = "Grades.View", DisplayName = "Просмотр оценок", Description = "Просмотр оценок студентов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("50000002-0000-0000-0000-000000000002"), Name = "Grades.Create", DisplayName = "Выставление оценок", Description = "Создание новых оценок", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("50000003-0000-0000-0000-000000000003"), Name = "Grades.Edit", DisplayName = "Редактирование оценок", Description = "Изменение оценок", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("50000004-0000-0000-0000-000000000004"), Name = "Grades.Delete", DisplayName = "Удаление оценок", Description = "Удаление оценок", CreatedAt = baseDate, LastModifiedAt = baseDate },

            // Права на библиотеку
            new Permission { Uid = Guid.Parse("60000001-0000-0000-0000-000000000001"), Name = "Library.View", DisplayName = "Просмотр библиотеки", Description = "Просмотр библиотечных ресурсов", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Permission { Uid = Guid.Parse("60000002-0000-0000-0000-000000000002"), Name = "Library.Manage", DisplayName = "Управление библиотекой", Description = "Управление библиотечными ресурсами", CreatedAt = baseDate, LastModifiedAt = baseDate }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);
    }

    /// <summary>
    /// Связывание ролей с правами доступа
    /// </summary>
    private static void SeedRolePermissions(ModelBuilder modelBuilder)
    {
        var rolePermissions = new List<RolePermission>();

        // SystemAdmin - все права
        var systemAdminRoleUid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var allPermissions = new[]
        {
            Guid.Parse("10000001-0000-0000-0000-000000000001"), // SystemAdmin.FullAccess
            Guid.Parse("10000002-0000-0000-0000-000000000002"), // Users.View
            Guid.Parse("10000003-0000-0000-0000-000000000003"), // Users.Create
            Guid.Parse("10000004-0000-0000-0000-000000000004"), // Users.Edit
            Guid.Parse("10000005-0000-0000-0000-000000000005"), // Users.Delete
            Guid.Parse("20000001-0000-0000-0000-000000000001"), // Students.View
            Guid.Parse("20000002-0000-0000-0000-000000000002"), // Students.Create
            Guid.Parse("20000003-0000-0000-0000-000000000003"), // Students.Edit
            Guid.Parse("20000004-0000-0000-0000-000000000004"), // Students.Delete
            Guid.Parse("30000001-0000-0000-0000-000000000001"), // Teachers.View
            Guid.Parse("30000002-0000-0000-0000-000000000002"), // Teachers.Create
            Guid.Parse("30000003-0000-0000-0000-000000000003"), // Teachers.Edit
            Guid.Parse("30000004-0000-0000-0000-000000000004"), // Teachers.Delete
            Guid.Parse("40000001-0000-0000-0000-000000000001"), // Courses.View
            Guid.Parse("40000002-0000-0000-0000-000000000002"), // Courses.Create
            Guid.Parse("40000003-0000-0000-0000-000000000003"), // Courses.Edit
            Guid.Parse("40000004-0000-0000-0000-000000000004"), // Courses.Delete
            Guid.Parse("50000001-0000-0000-0000-000000000001"), // Grades.View
            Guid.Parse("50000002-0000-0000-0000-000000000002"), // Grades.Create
            Guid.Parse("50000003-0000-0000-0000-000000000003"), // Grades.Edit
            Guid.Parse("50000004-0000-0000-0000-000000000004"), // Grades.Delete
            Guid.Parse("60000001-0000-0000-0000-000000000001"), // Library.View
            Guid.Parse("60000002-0000-0000-0000-000000000002")  // Library.Manage
        };

        foreach (var permissionUid in allPermissions)
        {
            rolePermissions.Add(new RolePermission { RoleUid = systemAdminRoleUid, PermissionUid = permissionUid });
        }

        // Teacher - права преподавателя
        var teacherRoleUid = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var teacherPermissions = new[]
        {
            Guid.Parse("20000001-0000-0000-0000-000000000001"), // Students.View
            Guid.Parse("40000001-0000-0000-0000-000000000001"), // Courses.View
            Guid.Parse("50000001-0000-0000-0000-000000000001"), // Grades.View
            Guid.Parse("50000002-0000-0000-0000-000000000002"), // Grades.Create
            Guid.Parse("50000003-0000-0000-0000-000000000003"), // Grades.Edit
            Guid.Parse("50000004-0000-0000-0000-000000000004"), // Grades.Delete
            Guid.Parse("60000001-0000-0000-0000-000000000001")  // Library.View
        };

        foreach (var permissionUid in teacherPermissions)
        {
            rolePermissions.Add(new RolePermission { RoleUid = teacherRoleUid, PermissionUid = permissionUid });
        }

        // Student - права студента
        var studentRoleUid = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var studentPermissions = new[]
        {
            Guid.Parse("40000001-0000-0000-0000-000000000001"), // Courses.View
            Guid.Parse("50000001-0000-0000-0000-000000000001"), // Grades.View
            Guid.Parse("60000001-0000-0000-0000-000000000001")  // Library.View
        };

        foreach (var permissionUid in studentPermissions)
        {
            rolePermissions.Add(new RolePermission { RoleUid = studentRoleUid, PermissionUid = permissionUid });
        }

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
    }

    /// <summary>
    /// Создание начальных департаментов
    /// </summary>
    private static void SeedDepartments(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var departments = new[]
        {
            new Department { Uid = Guid.Parse("d1111111-1111-1111-1111-111111111111"), Code = "IT", Name = "Информационные технологии", Description = "Кафедра информационных технологий и программирования", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Department { Uid = Guid.Parse("d2222222-2222-2222-2222-222222222222"), Code = "MATH", Name = "Математика", Description = "Кафедра математики и статистики", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Department { Uid = Guid.Parse("d3333333-3333-3333-3333-333333333333"), Code = "PHYS", Name = "Физика", Description = "Кафедра физики и естественных наук", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Department { Uid = Guid.Parse("d4444444-4444-4444-4444-444444444444"), Code = "LANG", Name = "Иностранные языки", Description = "Кафедра иностранных языков", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new Department { Uid = Guid.Parse("d5555555-5555-5555-5555-555555555555"), Code = "ECON", Name = "Экономика", Description = "Кафедра экономики и менеджмента", CreatedAt = baseDate, LastModifiedAt = baseDate }
        };

        modelBuilder.Entity<Department>().HasData(departments);
    }

    /// <summary>
    /// Создание системных настроек
    /// </summary>
    private static void SeedSystemSettings(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var settings = new[]
        {
            new SystemSetting { Uid = Guid.Parse("a1111111-1111-1111-1111-111111111111"), Key = "System.Name", Value = "Viridisca LMS", Description = "Название системы", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a2222222-2222-2222-2222-222222222222"), Key = "System.Version", Value = "1.0.0", Description = "Версия системы", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a3333333-3333-3333-3333-333333333333"), Key = "Academic.DefaultCredits", Value = "3", Description = "Количество кредитов по умолчанию для предмета", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a4444444-4444-4444-4444-444444444444"), Key = "Academic.MaxGrade", Value = "5.0", Description = "Максимальная оценка в системе", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a5555555-5555-5555-5555-555555555555"), Key = "Academic.MinPassingGrade", Value = "2.5", Description = "Минимальная проходная оценка", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a6666666-6666-6666-6666-666666666666"), Key = "Library.MaxLoanDays", Value = "30", Description = "Максимальный срок займа библиотечного ресурса (дни)", CreatedAt = baseDate, LastModifiedAt = baseDate },
            new SystemSetting { Uid = Guid.Parse("a7777777-7777-7777-7777-777777777777"), Key = "Notification.EmailEnabled", Value = "true", Description = "Включены ли email уведомления", CreatedAt = baseDate, LastModifiedAt = baseDate }
        };

        modelBuilder.Entity<SystemSetting>().HasData(settings);
    }

    /// <summary>
    /// Создание шаблонов уведомлений
    /// </summary>
    private static void SeedNotificationTemplates(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var templates = new[]
        {
            new NotificationTemplate 
            { 
                Uid = Guid.Parse("b1111111-1111-1111-1111-111111111111"), 
                Name = "WelcomeStudent", 
                TitleTemplate = "Добро пожаловать в Viridisca LMS!", 
                MessageTemplate = "Здравствуйте, {StudentName}! Добро пожаловать в систему управления обучением Viridisca LMS. Ваш логин: {Username}", 
                Category = "Welcome",
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            },
            new NotificationTemplate 
            { 
                Uid = Guid.Parse("b2222222-2222-2222-2222-222222222222"), 
                Name = "GradePublished", 
                TitleTemplate = "Новая оценка", 
                MessageTemplate = "Здравствуйте, {StudentName}! По предмету '{CourseName}' выставлена новая оценка: {Grade}", 
                Category = "Academic",
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            },
            new NotificationTemplate 
            { 
                Uid = Guid.Parse("b3333333-3333-3333-3333-333333333333"), 
                Name = "AssignmentDue", 
                TitleTemplate = "Напоминание о задании", 
                MessageTemplate = "Здравствуйте, {StudentName}! Напоминаем, что задание '{AssignmentName}' должно быть сдано до {DueDate}", 
                Category = "Reminder",
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            }
        };

        modelBuilder.Entity<NotificationTemplate>().HasData(templates);
    }

    /// <summary>
    /// Создание системного администратора
    /// </summary>
    private static void SeedSystemAdministrator(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Создаем Person для системного администратора
        var adminPersonUid = Guid.Parse("aaaabbbb-0000-0000-0000-000000000001");
        var adminPerson = new Person
        {
            Uid = adminPersonUid,
            FirstName = "Системный",
            LastName = "Администратор",
            Email = "admin@viridisca.edu",
            Phone = "+7 (999) 123-45-67",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        // Создаем Account для системного администратора
        var adminAccount = new Account
        {
            Uid = Guid.Parse("aaaabbbb-1111-1111-1111-111111111111"),
            PersonUid = adminPersonUid,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Временный пароль
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        // Назначаем роль SystemAdmin
        var adminPersonRole = new PersonRole
        {
            Uid = Guid.Parse("aaaabbbb-2222-2222-2222-222222222222"),
            PersonUid = adminPersonUid,
            RoleUid = Guid.Parse("11111111-1111-1111-1111-111111111111"), // SystemAdmin role
            AssignedAt = baseDate,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        // Создаем демонстрационного преподавателя
        var teacherPersonUid = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var teacherPerson = new Person
        {
            Uid = teacherPersonUid,
            FirstName = "Анна",
            LastName = "Петрова",
            MiddleName = "Сергеевна",
            Email = "a.petrova@viridisca.edu",
            Phone = "+7 (999) 234-56-78",
            DateOfBirth = new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var teacherAccount = new Account
        {
            Uid = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"),
            PersonUid = teacherPersonUid,
            Username = "a.petrova",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var teacherPersonRole = new PersonRole
        {
            Uid = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
            PersonUid = teacherPersonUid,
            RoleUid = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Teacher role
            AssignedAt = baseDate,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var teacher = new Teacher
        {
            Uid = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333"),
            PersonUid = teacherPersonUid,
            EmployeeCode = "T001",
            Salary = 75000,
            Qualification = "Кандидат технических наук",
            DepartmentUid = Guid.Parse("d1111111-1111-1111-1111-111111111111"), // IT Department
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        // Создаем демонстрационного студента
        var studentPersonUid = Guid.Parse("22222222-0000-0000-0000-000000000001");
        var studentPerson = new Person
        {
            Uid = studentPersonUid,
            FirstName = "Иван",
            LastName = "Иванов",
            MiddleName = "Петрович",
            Email = "i.ivanov@student.viridisca.edu",
            Phone = "+7 (999) 345-67-89",
            DateOfBirth = new DateTime(2003, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var studentAccount = new Account
        {
            Uid = Guid.Parse("cccccccc-1111-1111-1111-111111111111"),
            PersonUid = studentPersonUid,
            Username = "i.ivanov",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var studentPersonRole = new PersonRole
        {
            Uid = Guid.Parse("cccccccc-2222-2222-2222-222222222222"),
            PersonUid = studentPersonUid,
            RoleUid = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Student role
            AssignedAt = baseDate,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var student2PersonUid = Guid.Parse("33333333-0000-0000-0000-000000000002");
        var student2Person = new Person
        {
            Uid = student2PersonUid,
            FirstName = "Мария",
            LastName = "Сидорова",
            MiddleName = "Александровна",
            Email = "m.sidorova@student.viridisca.edu",
            Phone = "+7 (999) 456-78-90",
            DateOfBirth = new DateTime(2003, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var student2Account = new Account
        {
            Uid = Guid.Parse("dddddddd-1111-1111-1111-111111111111"),
            PersonUid = student2PersonUid,
            Username = "m.sidorova",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        var student2PersonRole = new PersonRole
        {
            Uid = Guid.Parse("dddddddd-2222-2222-2222-222222222222"),
            PersonUid = student2PersonUid,
            RoleUid = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Student role
            AssignedAt = baseDate,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        // Добавляем все данные в модель
        modelBuilder.Entity<Person>().HasData(adminPerson, teacherPerson, studentPerson, student2Person);
        modelBuilder.Entity<Account>().HasData(adminAccount, teacherAccount, studentAccount, student2Account);
        modelBuilder.Entity<PersonRole>().HasData(adminPersonRole, teacherPersonRole, studentPersonRole, student2PersonRole);
        modelBuilder.Entity<Teacher>().HasData(teacher);
    }

    /// <summary>
    /// Создание академических периодов
    /// </summary>
    private static void SeedAcademicPeriods(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var periods = new[]
        {
            new AcademicPeriod 
            { 
                Uid = Guid.Parse("aa111111-1111-1111-1111-111111111111"), 
                Code = "FALL2024", 
                Name = "Осенний семестр 2024", 
                Type = AcademicPeriodType.Semester,
                AcademicYear = 2024,
                StartDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            },
            new AcademicPeriod 
            { 
                Uid = Guid.Parse("aa222222-2222-2222-2222-222222222222"), 
                Code = "SPRING2025", 
                Name = "Весенний семестр 2025", 
                Type = AcademicPeriodType.Semester,
                AcademicYear = 2024,
                StartDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = false,
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            }
        };

        modelBuilder.Entity<AcademicPeriod>().HasData(periods);
    }

    /// <summary>
    /// Создание примерных данных для демонстрации
    /// </summary>
    private static void SeedSampleData(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Создаем примерные предметы
        var subjects = new[]
        {
            new Subject 
            { 
                Uid = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), 
                Code = "CS101", 
                Name = "Основы программирования", 
                Description = "Введение в программирование на C#",
                Credits = 4,
                Type = SubjectType.Required,
                DepartmentUid = Guid.Parse("d1111111-1111-1111-1111-111111111111"), // IT Department
                IsActive = true,
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            },
            new Subject 
            { 
                Uid = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), 
                Code = "MATH201", 
                Name = "Высшая математика", 
                Description = "Математический анализ и линейная алгебра",
                Credits = 5,
                Type = SubjectType.Required,
                DepartmentUid = Guid.Parse("d2222222-2222-2222-2222-222222222222"), // Math Department
                IsActive = true,
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            },
            new Subject 
            { 
                Uid = Guid.Parse("cccccccc-0000-0000-0000-000000000003"), 
                Code = "ENG101", 
                Name = "Английский язык", 
                Description = "Базовый курс английского языка",
                Credits = 3,
                Type = SubjectType.Elective,
                DepartmentUid = Guid.Parse("d4444444-4444-4444-4444-444444444444"), // Language Department
                IsActive = true,
                CreatedAt = baseDate, 
                LastModifiedAt = baseDate 
            }
        };

        modelBuilder.Entity<Subject>().HasData(subjects);

        // Создаем примерную группу с фиксированным Uid
        var groupUid = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
        var group = new Group
        {
            Uid = groupUid,
            Code = "IT-21-1",
            Name = "Информационные технологии 2021, группа 1",
            DepartmentUid = Guid.Parse("d1111111-1111-1111-1111-111111111111"), // IT Department
            IsActive = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        modelBuilder.Entity<Group>().HasData(group);

        // Создаем учебный план
        var curriculumUid = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");
        var curriculum = new Curriculum
        {
            Uid = curriculumUid,
            Code = "IT-2021",
            Name = "Информационные технологии 2021",
            Description = "Учебный план по направлению Информационные технологии",
            TotalCredits = 240,
            DurationMonths = 48,
            DepartmentUid = Guid.Parse("d1111111-1111-1111-1111-111111111111"), // IT Department
            IsActive = true,
            CreatedAt = baseDate,
            LastModifiedAt = baseDate
        };

        modelBuilder.Entity<Curriculum>().HasData(curriculum);

        // Связываем предметы с учебным планом
        var curriculumSubjects = new[]
        {
            new CurriculumSubject
            {
                CurriculumUid = curriculumUid,
                SubjectUid = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), // CS101
                Semester = 1,
                Credits = 4,
                IsRequired = true
            },
            new CurriculumSubject
            {
                CurriculumUid = curriculumUid,
                SubjectUid = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), // MATH201
                Semester = 1,
                Credits = 5,
                IsRequired = true
            },
            new CurriculumSubject
            {
                CurriculumUid = curriculumUid,
                SubjectUid = Guid.Parse("cccccccc-0000-0000-0000-000000000003"), // ENG101
                Semester = 2,
                Credits = 3,
                IsRequired = false
            }
        };

        modelBuilder.Entity<CurriculumSubject>().HasData(curriculumSubjects);

        // Обновляем студентов, назначая им группу и учебный план
        var updatedStudents = new[]
        {
            new Student
            {
                Uid = Guid.Parse("ffffffff-0000-0000-0000-000000000001"),
                PersonUid = Guid.Parse("22222222-0000-0000-0000-000000000001"),
                StudentCode = "S2021001",
                GPA = 4.2,
                Status = StudentStatus.Active,
                GroupUid = groupUid,
                CurriculumUid = curriculumUid,
                EnrollmentDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = baseDate,
                LastModifiedAt = baseDate
            },
            new Student
            {
                Uid = Guid.Parse("aaaabbbb-0000-0000-0000-000000000002"),
                PersonUid = Guid.Parse("33333333-0000-0000-0000-000000000002"),
                StudentCode = "S2021002",
                GPA = 3.8,
                Status = StudentStatus.Active,
                GroupUid = groupUid,
                CurriculumUid = curriculumUid,
                EnrollmentDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = baseDate,
                LastModifiedAt = baseDate
            }
        };

        // Заменяем данные студентов
        modelBuilder.Entity<Student>().HasData(updatedStudents);
    }

    #endregion
}
