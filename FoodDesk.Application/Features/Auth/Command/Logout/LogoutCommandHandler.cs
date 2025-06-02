using FoodDesk.Application.Interfaces.Services;
using MediatR;

namespace FoodDesk.Application.Features.Auth.Command.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync();
        return true;
    }
}
