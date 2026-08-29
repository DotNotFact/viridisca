using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.SetCurrentAcademicPeriod;

public sealed record SetCurrentAcademicPeriodCommand(Guid AcademicPeriodUid) : IRequest<bool>;
