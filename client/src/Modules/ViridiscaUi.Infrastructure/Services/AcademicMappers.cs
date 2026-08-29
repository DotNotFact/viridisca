using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Maps backend Academic API DTOs to the client's own (much richer) domain entities.
/// Fields backend doesn't track yet (GPA, curricula, analytics, navigation collections)
/// are left at their default — these entities aren't EF-tracked, they're POCOs built
/// purely from the API response, same approach as HttpAuthService's Person/Account mapping.
/// </summary>
internal static class AcademicMappers
{
    public static Student ToStudent(StudentResponseDto dto) => new()
    {
        Uid = dto.Uid,
        PersonUid = dto.UserUid,
        StudentCode = dto.StudentCode,
        EnrollmentDate = dto.EnrollmentDate,
        GraduationDate = dto.GraduationDate,
        Status = ParseEnum(dto.Status, StudentStatus.Active),
        GroupUid = dto.GroupUid,
        IsActive = dto.Status == nameof(StudentStatus.Active),
        Person = new Domain.Entities.Auth.Person
        {
            Uid = dto.UserUid,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            ProfileImageUrl = dto.ProfileImageUrl,
            DateOfBirth = dto.DateOfBirth == default ? null : dto.DateOfBirth,
        },
    };

    public static Teacher ToTeacher(TeacherResponseDto dto) => new()
    {
        Uid = dto.Uid,
        PersonUid = dto.UserUid,
        EmployeeCode = dto.EmployeeCode,
        HireDate = dto.HireDate,
        Specialization = dto.Specialization,
        Qualification = dto.Qualifications ?? string.Empty,
        IsActive = dto.Status == "Active",
        DepartmentUid = dto.DepartmentUid,
        Person = new Domain.Entities.Auth.Person
        {
            Uid = dto.UserUid,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            ProfileImageUrl = dto.ProfileImageUrl,
        },
    };

    public static Group ToGroup(GroupResponseDto dto) => new()
    {
        Uid = dto.Uid,
        Code = dto.Code,
        Name = dto.Name,
        Description = dto.Description,
        Year = dto.Year,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        MaxStudents = dto.MaxStudents,
        DepartmentUid = dto.DepartmentUid,
        CuratorUid = dto.CuratorUid,
        Status = ParseEnum(dto.Status, GroupStatus.Active),
        IsActive = dto.Status == nameof(GroupStatus.Active),
    };

    public static Subject ToSubject(SubjectResponseDto dto) => new()
    {
        Uid = dto.Uid,
        Name = dto.Name,
        Code = dto.Code,
        Description = dto.Description,
        Credits = dto.Credits,
        Type = ParseSubjectType(dto.Type),
        DepartmentUid = dto.DepartmentUid,
        IsActive = dto.IsActive,
    };

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct, Enum
        => Enum.TryParse<TEnum>(value, out var parsed) ? parsed : fallback;

    // Backend's SubjectType (Core/Elective/Laboratory/Practical/Seminar/Research/Workshop) and
    // the client's own (Required/Elective/Specialized/Practicum/Seminar/Laboratory/Lecture)
    // only partially overlap by name — this predates the API integration and isn't something
    // to silently reconcile by guessing; unmapped values fall back to Required.
    private static SubjectType ParseSubjectType(string backendType) => backendType switch
    {
        "Elective" => SubjectType.Elective,
        "Seminar" => SubjectType.Seminar,
        "Laboratory" => SubjectType.Laboratory,
        "Practical" => SubjectType.Practicum,
        _ => SubjectType.Required,
    };
}
