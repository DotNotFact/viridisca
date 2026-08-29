using System;
using MediatR;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Subjects.Commands.CreateSubject;

public sealed record CreateSubjectCommand(
    string Name,
    string Code,
    string Description,
    int Credits,
    SubjectType Type,
    SubjectDifficulty Difficulty,
    Guid? DepartmentUid = null,
    int? MinimumRequiredGrade = null) : IRequest<Guid>;
