using System.Collections.Generic;

namespace FoodDesk.WEB.Areas.AdminPanel.Models
{
    public class MenuViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<ProductDto> Top5PopularDishes { get; set; } = new();
        public ProductDto? PromoProduct { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int TotalSold { get; set; } // для топа
    }
} 