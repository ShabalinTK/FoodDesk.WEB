using MediatR;

namespace FoodDesk.Application.Features.Auth.Command.Register;

public class RegisterCommand : IRequest<bool>
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public bool IsCourier { get; set; }
}
