using FoodDesk.Application.Interfaces.Services;
using MediatR;

namespace FoodDesk.Application.Features.Auth.Queries.Login;

public class LoginQueryHandler(IAuthService _authService) : IRequestHandler<LoginQuery, bool>
{
    public async Task<bool> Handle(LoginQuery query, CancellationToken cancellationToken)
        => await _authService.LoginAsync(query.email, query.password);
}
