namespace FoodDesk.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public double Rating { get; set; }
    public string ImageUrl { get; set; }
    public bool IsPopular { get; set; }
    public int? CategoryId { get; set; }
    public Category Category { get; set; }
}
