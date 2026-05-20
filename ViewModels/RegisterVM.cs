using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseMS.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Ad Soyad vacibdir")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email vacibdir"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə vacibdir"), MinLength(3)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Şifrələr eyni deyil")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}