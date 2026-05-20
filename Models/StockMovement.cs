using System;

namespace WarehouseMS.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }
        public string Type { get; set; } = "In"; // In və ya Out
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }
}