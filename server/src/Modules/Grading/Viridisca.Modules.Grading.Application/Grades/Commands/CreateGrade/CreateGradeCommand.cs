using System;
using MediatR;
using Viridisca.Modules.Grading.Domain.Models;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.CreateGrade;

public sealed record CreateGradeCommand(
    Guid StudentUid,
    Guid SubjectUid,
    Guid TeacherUid,
    decimal Value,
    GradeType Type,
    string Description = "",
    Guid? LessonUid = null) : IRequest<Guid>;
