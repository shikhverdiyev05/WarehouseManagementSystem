using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseMS.Models
{
    public class Stock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")] // Bu mütləq olmalıdır!
        public virtual Product? Product { get; set; }
        public int WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        public string Type { get; set; } = "In"; // In və ya Out
        public int? ShelfId { get; set; }
        public virtual Shelf? Shelf { get; set; }

        public int Quantity { get; set; }
    }
}