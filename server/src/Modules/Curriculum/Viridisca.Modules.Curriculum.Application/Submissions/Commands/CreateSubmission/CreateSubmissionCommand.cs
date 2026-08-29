using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Commands.CreateSubmission;

public sealed record CreateSubmissionCommand(Guid AssignmentUid, Guid StudentUid, string? Content = null, string? FilePath = null) : IRequest<Guid>;
