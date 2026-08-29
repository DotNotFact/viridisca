using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Services
{
    /// <summary>
    /// Реализация сервиса для работы с выполненными заданиями
    /// </summary>
    public class SubmissionService : ISubmissionService
    {
        private readonly ApplicationDbContext _context;

        public SubmissionService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Submission?> GetSubmissionAsync(Guid uid)
        {
            return await _context.Submissions
                .FirstOrDefaultAsync(s => s.Uid == uid);
        }

        public async Task<IEnumerable<Submission>> GetAllSubmissionsAsync()
        {
            return await _context.Submissions.ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid)
        {
            return await _context.Submissions
                .Where(s => s.StudentUid == studentUid)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
        {
            return await _context.Submissions
                .Where(s => s.AssignmentUid == assignmentUid)
                .ToListAsync();
        }

        public async Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid)
        {
            return await _context.Submissions
                .FirstOrDefaultAsync(s => s.StudentUid == studentUid && s.AssignmentUid == assignmentUid);
        }

        public async Task AddSubmissionAsync(Submission submission)
        {
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            _context.Submissions.Update(submission);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSubmissionAsync(Guid uid)
        {
            var submission = await _context.Submissions.FindAsync(uid);
            if (submission == null) return false;
            
            _context.Submissions.Remove(submission);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> GradeSubmissionAsync(Guid uid, int grade, string feedback)
        {
            var submission = await _context.Submissions.FindAsync(uid);
            if (submission == null) return false;

            submission.Score = grade;
            submission.Feedback = feedback;
            submission.GradedDate = DateTime.UtcNow;
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Submission>> GetPendingSubmissionsAsync(Guid? teacherUid = null)
        {
            var query = _context.Submissions
                .Where(s => !s.Score.HasValue);

            if (teacherUid.HasValue)
            {
                query = query.Where(s => s.Assignment != null && 
                    s.Assignment.CourseInstance != null && 
                    s.Assignment.CourseInstance.TeacherUid == teacherUid.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetOverdueSubmissionsAsync()
        {
            return await _context.Submissions
                .Where(s => s.Assignment != null && 
                    s.Assignment.DueDate < DateTime.Now && 
                    !s.Score.HasValue)
                .ToListAsync();
        }
    }
} 