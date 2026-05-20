using System.ComponentModel.DataAnnotations;

namespace WarehouseMS.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? SKU { get; set; } // Nullable edildi ki, xəta verməsin
        public string? Barcode { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int MinStockLevel { get; set; }
        public string? Unit { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Stoklarla əlaqə
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}