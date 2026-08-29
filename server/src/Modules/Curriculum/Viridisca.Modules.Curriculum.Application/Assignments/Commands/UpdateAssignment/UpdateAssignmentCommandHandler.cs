using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.UpdateAssignment;

internal sealed class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, bool>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssignmentCommandHandler(IAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByUidAsync(request.AssignmentUid, cancellationToken)
            ?? throw new InvalidOperationException($"Задание с ID {request.AssignmentUid} не найдено");

        var detailsResult = assignment.UpdateDetails(request.Title, request.Description, request.Instructions);
        if (detailsResult.IsFailure)
        {
            throw new InvalidOperationException(detailsResult.Error.Message);
        }

        assignment.UpdateDueDate(request.DueDate);

        var scoreResult = assignment.UpdateMaxScore(request.MaxScore);
        if (scoreResult.IsFailure)
        {
            throw new InvalidOperationException(scoreResult.Error.Message);
        }

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
