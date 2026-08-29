using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAcademicPeriod;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetCurrentAcademicPeriod;

internal sealed class GetCurrentAcademicPeriodQueryHandler : IRequestHandler<GetCurrentAcademicPeriodQuery, AcademicPeriodDto?>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;

    public GetCurrentAcademicPeriodQueryHandler(IAcademicPeriodRepository academicPeriodRepository)
    {
        _academicPeriodRepository = academicPeriodRepository;
    }

    public async Task<AcademicPeriodDto?> Handle(GetCurrentAcademicPeriodQuery request, CancellationToken cancellationToken)
    {
        var period = await _academicPeriodRepository.GetCurrentAsync(cancellationToken);
        return period is null ? null : AcademicPeriodMapper.Map(period);
    }
}
