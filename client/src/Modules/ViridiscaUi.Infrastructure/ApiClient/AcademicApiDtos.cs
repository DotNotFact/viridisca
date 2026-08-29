namespace ViridiscaUi.Infrastructure.ApiClient;

// ---- Students ----

public sealed record CreateStudentRequestDto(
    Guid UserUid, string FirstName, string LastName, string Email, DateTime BirthDate,
    string StudentCode, DateTime EnrollmentDate, string? MiddleName = null,
    string? PhoneNumber = null, Guid? GroupUid = null);

public sealed record UpdateStudentRequestDto(string? EmergencyContactName, string? EmergencyContactPhone, string? MedicalInformation);

public sealed record AssignToGroupRequestDto(Guid GroupUid);

public sealed record AddParentRequestDto(Guid ParentUserUid, string Relation, bool IsPrimaryContact = false, bool HasLegalGuardianship = false);

public sealed record StudentParentResponseDto(
    Guid Uid, Guid ParentUserUid, string? ParentFullName, string Relation,
    bool IsPrimaryContact, bool HasLegalGuardianship, string? Email, string? PhoneNumber);

public sealed record StudentResponseDto(
    Guid Uid, Guid UserUid, string StudentCode, DateTime EnrollmentDate, string Status,
    Guid? GroupUid, string? GroupName, string? EmergencyContactName, string? EmergencyContactPhone,
    string? MedicalInformation, DateTime? GraduationDate, DateTime CreatedAtUtc,
    IReadOnlyCollection<StudentParentResponseDto> Parents,
    string FirstName, string LastName, string? MiddleName, string Email,
    string? PhoneNumber, string? ProfileImageUrl, DateTime DateOfBirth);

// ---- Teachers ----

public sealed record CreateTeacherRequestDto(
    Guid UserUid, string EmployeeCode, DateTime HireDate, string? Specialization,
    string? Qualifications, int YearsOfExperience, Guid? DepartmentUid = null);

public sealed record AssignSubjectRequestDto(Guid SubjectUid, bool IsMainTeacher = false);

public sealed record TeacherSubjectResponseDto(
    Guid Uid, Guid SubjectUid, string? SubjectName, string? SubjectCode,
    bool IsMainTeacher, DateTime AssignedAtUtc, DateTime? EndedAtUtc, bool IsActive);

public sealed record TeacherGroupResponseDto(
    Guid Uid, Guid GroupUid, string? GroupName, Guid SubjectUid, string? SubjectName,
    bool IsCurator, DateTime AssignedAtUtc, DateTime? EndedAtUtc, bool IsActive);

public sealed record TeacherResponseDto(
    Guid Uid, Guid UserUid, string EmployeeCode, DateTime HireDate, string Status,
    string? Specialization, string? Qualifications, int YearsOfExperience, string? Biography,
    Guid? DepartmentUid, string? DepartmentName, DateTime CreatedAtUtc,
    IReadOnlyCollection<TeacherSubjectResponseDto> Subjects,
    IReadOnlyCollection<TeacherGroupResponseDto> Groups,
    string FirstName, string LastName, string? MiddleName, string FullName,
    string Email, string? PhoneNumber, string? ProfileImageUrl);

// ---- Groups ----

public sealed record CreateGroupRequestDto(
    string Code, string Name, string? Description, int Year, DateTime StartDate,
    int MaxStudents, Guid DepartmentUid, Guid? CuratorUid = null);

public sealed record UpdateGroupRequestDto(string Name, string? Description, int MaxStudents);

public sealed record SetCuratorRequestDto(Guid? CuratorUid);

public sealed record GroupResponseDto(
    Guid Uid, string Code, string Name, string? Description, int Year, DateTime StartDate,
    DateTime? EndDate, int MaxStudents, int CurrentStudentsCount, string Status,
    Guid? CuratorUid, Guid DepartmentUid, DateTime CreatedAtUtc);

// ---- Subjects ----

public sealed record CreateSubjectRequestDto(
    string Name, string Code, string? Description, int Credits, string Type, string Difficulty,
    Guid? DepartmentUid = null, int? MinimumRequiredGrade = null);

public sealed record UpdateSubjectRequestDto(string Name, string? Description, int Credits);

public sealed record SubjectResponseDto(
    Guid Uid, string Name, string Code, string? Description, int Credits, string Type,
    string Difficulty, Guid? DepartmentUid, int? MinimumRequiredGrade, bool IsActive, DateTime CreatedAtUtc);
