using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Models;

public class FavoriteMenuViewModel
{
    public List<Product> Products { get; set; } = new List<Product>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
} 