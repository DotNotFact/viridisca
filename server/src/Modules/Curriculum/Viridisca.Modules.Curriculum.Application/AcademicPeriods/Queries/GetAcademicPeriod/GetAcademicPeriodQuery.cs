using System;
using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAcademicPeriod;

public sealed record GetAcademicPeriodQuery(Guid AcademicPeriodUid) : IRequest<AcademicPeriodDto>;
