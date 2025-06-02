namespace FoodDesk.WEB.Models.Auth
{
    public class RegisterViewModel
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public bool IsCourier { get; set; }
    }
}
