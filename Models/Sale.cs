using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseMS.Models
{
    public class Sale
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [Required]
        public int ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miqdar ən azı 1 olmalıdır!")]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public int? CustomerId { get; internal set; }
    }
}