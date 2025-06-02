using FoodDesk.Application.Interfaces.Services;
using FoodDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FoodDesk.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IEmailSender _emailSender;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)//, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, string confirmpassword, bool iscourier)
        {
            var user = new ApplicationUser { Email = email, UserName = email };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return false;

            await _signInManager.SignInAsync(user, isPersistent: false);

            // Отправка письма
            //await _emailSender.SendAsync(email, "Регистрация", $"Ваш логин: {email}\nВаш пароль: {password}");

            return true;  
        }
         
        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            return result.Succeeded;
        }
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
