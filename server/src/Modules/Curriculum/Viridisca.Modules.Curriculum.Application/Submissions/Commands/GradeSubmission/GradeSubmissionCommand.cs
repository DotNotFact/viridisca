using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Commands.GradeSubmission;

public sealed record GradeSubmissionCommand(Guid SubmissionUid, decimal Score, string Feedback, Guid GradedByUid) : IRequest<bool>;
