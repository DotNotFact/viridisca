using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.CreateAcademicPeriod;

internal sealed class CreateAcademicPeriodCommandHandler : IRequestHandler<CreateAcademicPeriodCommand, Guid>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAcademicPeriodCommandHandler(IAcademicPeriodRepository academicPeriodRepository, IUnitOfWork unitOfWork)
    {
        _academicPeriodRepository = academicPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAcademicPeriodCommand request, CancellationToken cancellationToken)
    {
        var exists = await _academicPeriodRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Учебный период с кодом {request.Code} уже существует");
        }

        var periodResult = AcademicPeriod.Create(
            request.Name,
            request.Code,
            request.StartDate,
            request.EndDate,
            request.AcademicYear,
            request.Type,
            request.Description);

        if (periodResult.IsFailure)
        {
            throw new InvalidOperationException(periodResult.Error.Message);
        }

        var period = periodResult.Value;

        _academicPeriodRepository.Insert(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return period.Uid;
    }
}
