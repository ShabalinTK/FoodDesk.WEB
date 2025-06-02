using MediatR;

namespace FoodDesk.Application.Features.Auth.Queries.Login;

public record LoginQuery(string email, string password) : IRequest<bool>;
