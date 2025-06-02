namespace FoodDesk.Application.Interfaces.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string email, string password, string confirmpassword, bool iscourier);
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
}