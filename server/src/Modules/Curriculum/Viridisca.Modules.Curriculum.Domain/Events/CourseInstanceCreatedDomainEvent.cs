using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

public sealed class CourseInstanceCreatedDomainEvent : DomainEvent
{
    public Guid CourseInstanceUid { get; }

    public CourseInstanceCreatedDomainEvent(Guid courseInstanceUid)
    {
        CourseInstanceUid = courseInstanceUid;
    }
}
