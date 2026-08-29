using System;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGrade;

public sealed record GetGradeQuery(Guid GradeUid) : IRequest<GradeDto>;
