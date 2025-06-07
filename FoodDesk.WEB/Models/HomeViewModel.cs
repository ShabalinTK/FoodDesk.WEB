using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Models;

public class HomeViewModel
{
    public List<Category> Categories { get; set; }
    public List<Product> Products { get; set; }
    public List<Order> Orders { get; set; }
}