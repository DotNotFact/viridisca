using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Academic.Domain.Models;
using Viridisca.Modules.Academic.Infrastructure.Database;

namespace Viridisca.Modules.Academic.Infrastructure.Repositories;

public class TeacherRepository(AcademicDbContext dbContext) : ITeacherRepository
{
    private readonly AcademicDbContext _dbContext = dbContext;

    public async Task<Teacher> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Groups)
            .FirstOrDefaultAsync(t => t.Uid == uid, cancellationToken);

    public async Task<Teacher> GetByUserUidAsync(Guid userUid, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Groups)
            .FirstOrDefaultAsync(t => t.UserUid == userUid, cancellationToken);

    public async Task<Teacher> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Groups)
            .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode, cancellationToken);

    public async Task<IEnumerable<Teacher>> GetByDepartmentUidAsync(Guid departmentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers
            .Where(t => t.DepartmentUid == departmentUid)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Teacher>> GetBySubjectUidAsync(Guid subjectUid, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers
            .Include(t => t.Subjects)
            .Where(t => t.Subjects.Any(ts => ts.SubjectUid == subjectUid && ts.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
        => await _dbContext.Teachers.AnyAsync(t => t.EmployeeCode == employeeCode, cancellationToken);

    public void Insert(Teacher teacher) => _dbContext.Teachers.Add(teacher);

    public void Update(Teacher teacher) => _dbContext.Teachers.Update(teacher);

    public void Delete(Teacher teacher) => _dbContext.Teachers.Remove(teacher);

    public void AddTeacherSubject(TeacherSubject teacherSubject) => _dbContext.TeacherSubjects.Add(teacherSubject);
}
