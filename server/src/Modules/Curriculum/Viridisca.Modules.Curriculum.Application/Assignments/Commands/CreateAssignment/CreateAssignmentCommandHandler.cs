using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.CreateAssignment;

internal sealed class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseInstanceRepository _courseInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssignmentCommandHandler(
        IAssignmentRepository assignmentRepository,
        ICourseInstanceRepository courseInstanceRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _courseInstanceRepository = courseInstanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var courseInstance = await _courseInstanceRepository.GetByUidAsync(request.CourseInstanceUid, cancellationToken)
            ?? throw new InvalidOperationException($"Экземпляр курса с ID {request.CourseInstanceUid} не найден");

        var assignmentResult = Assignment.Create(
            courseInstance.Uid,
            request.Title,
            request.Description,
            request.Type,
            request.MaxScore,
            request.DueDate,
            request.Instructions,
            request.Difficulty);

        if (assignmentResult.IsFailure)
        {
            throw new InvalidOperationException(assignmentResult.Error.Message);
        }

        var assignment = assignmentResult.Value;

        _assignmentRepository.Insert(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return assignment.Uid;
    }
}
