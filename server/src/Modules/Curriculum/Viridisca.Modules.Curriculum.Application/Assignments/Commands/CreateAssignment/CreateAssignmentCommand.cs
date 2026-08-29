using System;
using MediatR;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.CreateAssignment;

public sealed record CreateAssignmentCommand(
    Guid CourseInstanceUid,
    string Title,
    string Description,
    AssignmentType Type,
    decimal MaxScore = 100,
    DateTime? DueDate = null,
    string Instructions = "",
    AssignmentDifficulty Difficulty = AssignmentDifficulty.Medium) : IRequest<Guid>;
