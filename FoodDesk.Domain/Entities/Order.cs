namespace FoodDesk.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public string RestaurantName { get; set; }
    public int DeliveryTime { get; set; }
    public decimal Distance { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> OrderItems { get; set; }
}
