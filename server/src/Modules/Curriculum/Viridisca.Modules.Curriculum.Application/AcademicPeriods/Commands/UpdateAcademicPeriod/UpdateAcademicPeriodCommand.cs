using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.UpdateAcademicPeriod;

public sealed record UpdateAcademicPeriodCommand(Guid AcademicPeriodUid, string Name, string Description, DateTime StartDate, DateTime EndDate) : IRequest<bool>;
