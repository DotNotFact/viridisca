using System;
using MediatR;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.PublishGrade;

public sealed record PublishGradeCommand(Guid GradeUid) : IRequest<bool>;
