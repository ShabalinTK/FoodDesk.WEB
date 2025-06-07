using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Models;

public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RestaurantName { get; set; } = "";
    public int DeliveryTime { get; set; }
    public decimal Distance { get; set; }
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
} 