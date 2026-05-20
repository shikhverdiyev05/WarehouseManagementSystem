namespace WarehouseMS.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; } // Create, Update, Delete
        public string TableName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string IpAddress { get; set; }
    }
}