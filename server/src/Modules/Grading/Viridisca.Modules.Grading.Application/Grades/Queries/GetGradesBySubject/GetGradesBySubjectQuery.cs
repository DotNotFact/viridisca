using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesBySubject;

public sealed record GetGradesBySubjectQuery(Guid SubjectUid) : IRequest<List<GradeDto>>;
