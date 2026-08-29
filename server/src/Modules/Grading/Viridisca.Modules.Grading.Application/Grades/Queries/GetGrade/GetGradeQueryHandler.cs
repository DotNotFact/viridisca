using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;
using Viridisca.Modules.Grading.Domain.Models;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGrade;

internal sealed class GetGradeQueryHandler : IRequestHandler<GetGradeQuery, GradeDto>
{
    private readonly IGradeRepository _gradeRepository;

    public GetGradeQueryHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<GradeDto> Handle(GetGradeQuery request, CancellationToken cancellationToken)
    {
        var grade = await _gradeRepository.GetByUidAsync(request.GradeUid, cancellationToken)
            ?? throw new InvalidOperationException($"Grade with UID {request.GradeUid} not found");

        return GradeMapper.Map(grade);
    }
}

internal static class GradeMapper
{
    public static GradeDto Map(Grade grade) => new()
    {
        Uid = grade.Uid,
        StudentUid = grade.StudentUid,
        SubjectUid = grade.SubjectUid,
        TeacherUid = grade.TeacherUid,
        LessonUid = grade.LessonUid,
        Value = grade.Value,
        Description = grade.Description,
        Type = grade.Type.ToString(),
        IssuedAtUtc = grade.IssuedAtUtc,
        IsPublished = grade.IsPublished,
        PublishedAtUtc = grade.PublishedAtUtc,
        CreatedAtUtc = grade.CreatedAtUtc,
        LastModifiedAtUtc = grade.LastModifiedAtUtc,
    };
}
