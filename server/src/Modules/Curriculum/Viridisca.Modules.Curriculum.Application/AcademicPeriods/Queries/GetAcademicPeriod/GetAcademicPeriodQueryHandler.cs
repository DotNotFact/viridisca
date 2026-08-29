using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAcademicPeriod;

internal sealed class GetAcademicPeriodQueryHandler : IRequestHandler<GetAcademicPeriodQuery, AcademicPeriodDto>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;

    public GetAcademicPeriodQueryHandler(IAcademicPeriodRepository academicPeriodRepository)
    {
        _academicPeriodRepository = academicPeriodRepository;
    }

    public async Task<AcademicPeriodDto> Handle(GetAcademicPeriodQuery request, CancellationToken cancellationToken)
    {
        var period = await _academicPeriodRepository.GetByUidAsync(request.AcademicPeriodUid, cancellationToken)
            ?? throw new InvalidOperationException($"Учебный период с ID {request.AcademicPeriodUid} не найден");

        return AcademicPeriodMapper.Map(period);
    }
}

internal static class AcademicPeriodMapper
{
    public static AcademicPeriodDto Map(AcademicPeriod period) => new()
    {
        Uid = period.Uid,
        Name = period.Name,
        Code = period.Code,
        Description = period.Description,
        Type = period.Type.ToString(),
        Status = period.Status.ToString(),
        StartDate = period.StartDate,
        EndDate = period.EndDate,
        AcademicYear = period.AcademicYear,
        IsCurrent = period.IsCurrent,
        CreatedAtUtc = period.CreatedAtUtc,
    };
}
