using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByAssignment;

public sealed record GetSubmissionsByAssignmentQuery(Guid AssignmentUid) : IRequest<List<SubmissionDto>>;
