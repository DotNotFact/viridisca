using System;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmission;

public sealed record GetSubmissionQuery(Guid SubmissionUid) : IRequest<SubmissionDto>;
