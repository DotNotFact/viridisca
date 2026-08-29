using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAllAcademicPeriods;

public sealed record GetAllAcademicPeriodsQuery : IRequest<List<AcademicPeriodDto>>;
