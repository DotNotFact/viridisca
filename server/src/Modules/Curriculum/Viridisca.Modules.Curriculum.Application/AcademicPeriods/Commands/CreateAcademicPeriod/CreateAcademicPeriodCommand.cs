using System;
using MediatR;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.CreateAcademicPeriod;

public sealed record CreateAcademicPeriodCommand(
    string Name,
    string Code,
    DateTime StartDate,
    DateTime EndDate,
    int AcademicYear,
    AcademicPeriodType Type,
    string Description = "") : IRequest<Guid>;
