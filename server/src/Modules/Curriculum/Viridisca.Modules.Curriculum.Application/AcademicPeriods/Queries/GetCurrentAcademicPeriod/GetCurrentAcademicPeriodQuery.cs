using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetCurrentAcademicPeriod;

public sealed record GetCurrentAcademicPeriodQuery : IRequest<AcademicPeriodDto?>;
