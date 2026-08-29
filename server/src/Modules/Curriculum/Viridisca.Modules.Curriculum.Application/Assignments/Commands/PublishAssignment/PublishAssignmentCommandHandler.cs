using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.PublishAssignment;

internal sealed class PublishAssignmentCommandHandler : IRequestHandler<PublishAssignmentCommand, bool>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishAssignmentCommandHandler(IAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PublishAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByUidAsync(request.AssignmentUid, cancellationToken)
            ?? throw new InvalidOperationException($"Задание с ID {request.AssignmentUid} не найдено");

        assignment.Publish();
        _assignmentRepository.Update(assignment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
