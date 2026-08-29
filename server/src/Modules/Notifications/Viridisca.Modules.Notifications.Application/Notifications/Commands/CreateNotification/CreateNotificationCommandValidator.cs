using FluentValidation;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.CreateNotification;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.RecipientUid).NotEmpty().WithMessage("ID получателя обязателен");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Заголовок уведомления обязателен");
    }
}
