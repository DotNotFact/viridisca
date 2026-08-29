using System;
using MediatR;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.UpdateGrade;

public sealed record UpdateGradeCommand(Guid GradeUid, decimal Value, string? Description = null, string? Reason = null) : IRequest<bool>;
