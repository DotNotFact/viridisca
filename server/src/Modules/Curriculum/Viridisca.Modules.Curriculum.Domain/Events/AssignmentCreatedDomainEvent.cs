using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

public sealed class AssignmentCreatedDomainEvent : DomainEvent
{
    public Guid AssignmentUid { get; }

    public AssignmentCreatedDomainEvent(Guid assignmentUid)
    {
        AssignmentUid = assignmentUid;
    }
}
