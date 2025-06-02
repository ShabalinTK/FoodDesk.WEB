using FoodDesk.Application.Interfaces.Services;
using MediatR;

namespace FoodDesk.Application.Features.Auth.Command.Register
{
    public class RegisterCommandHandler(IAuthService _authService) : IRequestHandler<RegisterCommand, bool>
    {
        public async Task<bool> Handle(RegisterCommand command, CancellationToken cancellationToken) =>
            await _authService.RegisterAsync(command.UserName, command.Email, command.Password, command.ConfirmPassword, command.IsCourier);
    }
}
