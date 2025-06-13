namespace FoodDesk.WEB.Areas.AdminPanel.Models;

public class OrdersViewModel
{
    public List<OrderViewModel> PendingOrders { get; set; } = new List<OrderViewModel>();
    public List<OrderViewModel> ConfirmedOrders { get; set; } = new List<OrderViewModel>();
    public List<OrderViewModel> DeliveredOrders { get; set; } = new List<OrderViewModel>();
}

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