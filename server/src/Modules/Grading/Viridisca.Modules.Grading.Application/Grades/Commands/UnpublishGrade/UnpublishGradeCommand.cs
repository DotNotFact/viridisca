using System;
using MediatR;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.UnpublishGrade;

public sealed record UnpublishGradeCommand(Guid GradeUid) : IRequest<bool>;
