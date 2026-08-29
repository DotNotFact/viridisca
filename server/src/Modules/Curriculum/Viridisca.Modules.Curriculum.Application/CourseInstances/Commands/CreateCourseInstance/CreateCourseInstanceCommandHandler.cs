using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.CreateCourseInstance;

internal sealed class CreateCourseInstanceCommandHandler : IRequestHandler<CreateCourseInstanceCommand, Guid>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;
    private readonly IAcademicPeriodRepository _academicPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseInstanceCommandHandler(
        ICourseInstanceRepository courseInstanceRepository,
        IAcademicPeriodRepository academicPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _courseInstanceRepository = courseInstanceRepository;
        _academicPeriodRepository = academicPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCourseInstanceCommand request, CancellationToken cancellationToken)
    {
        var period = await _academicPeriodRepository.GetByUidAsync(request.AcademicPeriodUid, cancellationToken)
            ?? throw new InvalidOperationException($"Учебный период с ID {request.AcademicPeriodUid} не найден");

        var courseInstanceResult = CourseInstance.Create(
            request.SubjectUid,
            request.GroupUid,
            period.Uid,
            request.Name,
            request.Code,
            request.StartDate,
            request.TeacherUid,
            request.Description,
            request.MaxEnrollments);

        if (courseInstanceResult.IsFailure)
        {
            throw new InvalidOperationException(courseInstanceResult.Error.Message);
        }

        var courseInstance = courseInstanceResult.Value;

        _courseInstanceRepository.Insert(courseInstance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return courseInstance.Uid;
    }
}
