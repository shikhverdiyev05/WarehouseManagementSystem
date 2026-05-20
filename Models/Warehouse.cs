using System;

namespace WarehouseMS.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Shelf> Shelves { get; set; }
    }
}
