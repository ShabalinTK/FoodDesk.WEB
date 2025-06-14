using System;

namespace FoodDesk.WEB.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }
    }
} 