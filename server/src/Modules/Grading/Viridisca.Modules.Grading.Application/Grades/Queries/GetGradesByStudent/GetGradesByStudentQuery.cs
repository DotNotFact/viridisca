using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesByStudent;

public sealed record GetGradesByStudentQuery(Guid StudentUid) : IRequest<List<GradeDto>>;
