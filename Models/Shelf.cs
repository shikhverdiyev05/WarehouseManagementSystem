using WarehouseMS.Models;

public class Shelf
{
    public int Id { get; set; }
    public string ShelfCode { get; set; }
    public int WarehouseId { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
    public virtual ICollection<Stock>? Stocks { get; set; }
}