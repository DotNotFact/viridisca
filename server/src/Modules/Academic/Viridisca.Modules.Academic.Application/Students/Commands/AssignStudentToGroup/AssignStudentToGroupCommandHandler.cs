using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Students.Commands.AssignStudentToGroup
{
    internal sealed class AssignStudentToGroupCommandHandler : IRequestHandler<AssignStudentToGroupCommand, bool>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignStudentToGroupCommandHandler(
            IStudentRepository studentRepository,
            IGroupRepository groupRepository,
            IUnitOfWork unitOfWork)
        {
            _studentRepository = studentRepository;
            _groupRepository = groupRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AssignStudentToGroupCommand request, CancellationToken cancellationToken)
        {
            // Получаем студента
            var student = await _studentRepository.GetByUidAsync(request.StudentUid, cancellationToken);
            if (student == null)
            {
                throw new Exception($"Студент с ID {request.StudentUid} не найден");
            }

            // Получаем группу
            var group = await _groupRepository.GetByUidAsync(request.GroupUid, cancellationToken);
            if (group == null)
            {
                throw new Exception($"Группа с ID {request.GroupUid} не найдена");
            }

            // Проверяем, есть ли место в группе (реальный подсчёт по БД, а не in-memory счётчик)
            var currentStudentsCount = (await _studentRepository.GetByGroupUidAsync(request.GroupUid, cancellationToken)).Count();
            if (currentStudentsCount >= group.MaxStudents)
            {
                throw new Exception($"В группе {group.Name} нет свободных мест");
            }

            // Назначаем студента в группу
            student.AssignToGroup(request.GroupUid);
            _studentRepository.Update(student);

            // Сохраняем изменения
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
} 