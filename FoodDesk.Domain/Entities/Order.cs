namespace FoodDesk.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; } // Связь с пользователем
    public string RestaurantName { get; set; }
    public int DeliveryTime { get; set; } // В минутах
    public decimal Distance { get; set; } // В км
    public string Status { get; set; } // Completed, Delivering, Preparing
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> OrderItems { get; set; }
}
