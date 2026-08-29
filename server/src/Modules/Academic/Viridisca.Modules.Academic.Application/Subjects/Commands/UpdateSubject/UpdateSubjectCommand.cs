using System;
using MediatR;

namespace Viridisca.Modules.Academic.Application.Subjects.Commands.UpdateSubject;

public sealed record UpdateSubjectCommand(Guid SubjectUid, string Name, string Description, int Credits) : IRequest<bool>;
