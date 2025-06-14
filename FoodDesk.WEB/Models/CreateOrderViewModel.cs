using System.ComponentModel.DataAnnotations;

namespace FoodDesk.WEB.Models;

public class CreateOrderViewModel
{
    [Required]
    public string RestaurantName { get; set; } = "";
    
    [Required]
    public int DeliveryTime { get; set; }
    
    [Required]
    public decimal Distance { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }
} 