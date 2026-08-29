using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByStudent;

public sealed record GetSubmissionsByStudentQuery(Guid StudentUid) : IRequest<List<SubmissionDto>>;
