using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.Dto;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignment;

internal sealed class GetAssignmentQueryHandler : IRequestHandler<GetAssignmentQuery, AssignmentDto>
{
    private readonly IAssignmentRepository _assignmentRepository;

    public GetAssignmentQueryHandler(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<AssignmentDto> Handle(GetAssignmentQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByUidAsync(request.AssignmentUid, cancellationToken)
            ?? throw new InvalidOperationException($"Задание с ID {request.AssignmentUid} не найдено");

        return AssignmentMapper.Map(assignment);
    }
}

internal static class AssignmentMapper
{
    public static AssignmentDto Map(Assignment assignment) => new()
    {
        Uid = assignment.Uid,
        CourseInstanceUid = assignment.CourseInstanceUid,
        Title = assignment.Title,
        Description = assignment.Description,
        Instructions = assignment.Instructions,
        DueDate = assignment.DueDate,
        MaxScore = assignment.MaxScore,
        Type = assignment.Type.ToString(),
        Difficulty = assignment.Difficulty.ToString(),
        Status = assignment.Status.ToString(),
        IsPublished = assignment.IsPublished,
        CreatedAtUtc = assignment.CreatedAtUtc,
    };
}
