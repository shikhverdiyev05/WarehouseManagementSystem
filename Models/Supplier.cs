using System.ComponentModel.DataAnnotations;

namespace WarehouseMS.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirkət adı mütləq daxil edilməlidir!")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty; // string.Empty xəbərdarlığı silir

        [Required(ErrorMessage = "Əlaqədar şəxs mütləq qeyd olunmalıdır!")]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt ünvanı boş qala bilməz!")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon nömrəsi mütləqdir!")]
        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin!")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ünvan boş qala bilməz!")]
        public string Address { get; set; } = string.Empty;
    }
}