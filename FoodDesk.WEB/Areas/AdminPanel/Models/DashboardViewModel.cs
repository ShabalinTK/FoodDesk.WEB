using System;

namespace FoodDesk.WEB.Areas.AdminPanel.Models
{
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public int TotalOrderConfirmed { get; set; }
        public int TotalOrderDelivered { get; set; }
        public int TotalOrderCanceled { get; set; }
        public int TotalOrderPending { get; set; }
        public int OrderTotal { get; set; }
        public int OrderTarget { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }
    }
} 