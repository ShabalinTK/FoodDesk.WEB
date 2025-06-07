using System.ComponentModel.DataAnnotations;

namespace FoodDesk.WEB.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Введите имя")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Введите фамилию")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите телефон")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Введите адрес")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Введите город")]
    public string City { get; set; } = "";

    [Required(ErrorMessage = "Введите страну")]
    public string Country { get; set; } = "";

    [Required(ErrorMessage = "Введите номер карты")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Номер карты должен содержать 16 цифр")]
    public string CardNumber { get; set; } = "";

    [Required(ErrorMessage = "Выберите месяц")]
    public string ExpiryMonth { get; set; } = "";

    [Required(ErrorMessage = "Введите год")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Год должен содержать 4 цифры")]
    public string ExpiryYear { get; set; } = "";

    [Required(ErrorMessage = "Введите CVV")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV должен содержать 3 цифры")]
    public string CVV { get; set; } = "";

    public decimal CartTotal { get; set; }
    public decimal TipAmount { get; set; }
    public decimal TotalAmount => CartTotal + TipAmount;

    public List<CartItem> CartItems { get; set; } = new();
}
