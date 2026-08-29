using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject.Dto;

namespace Viridisca.Modules.Academic.Application.Subjects.Queries.GetAllSubjects;

public sealed record GetAllSubjectsQuery : IRequest<List<SubjectDto>>;
