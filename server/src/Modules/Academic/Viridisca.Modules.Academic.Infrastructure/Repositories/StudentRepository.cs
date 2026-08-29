using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Academic.Domain.Models;
using Viridisca.Modules.Academic.Infrastructure.Database;

namespace Viridisca.Modules.Academic.Infrastructure.Repositories;

public class StudentRepository(AcademicDbContext dbContext) : IStudentRepository
{
    private readonly AcademicDbContext _dbContext = dbContext;

    public async Task<Student> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .Include(s => s.Parents)
            .FirstOrDefaultAsync(s => s.Uid == uid, cancellationToken);

    public async Task<Student> GetByUserUidAsync(Guid userUid, CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .Include(s => s.Parents)
            .FirstOrDefaultAsync(s => s.UserUid == userUid, cancellationToken);

    public async Task<Student> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .Include(s => s.Parents)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode, cancellationToken);

    public async Task<IEnumerable<Student>> GetByGroupUidAsync(Guid groupUid, CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .Where(s => s.GroupUid == groupUid)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        => await _dbContext.Students.AnyAsync(s => s.StudentCode == studentCode, cancellationToken);

    public void Insert(Student student) => _dbContext.Students.Add(student);

    public void Update(Student student) => _dbContext.Students.Update(student);

    public void Delete(Student student) => _dbContext.Students.Remove(student);

    public void AddStudentParent(StudentParent studentParent) => _dbContext.StudentParents.Add(studentParent);
}
