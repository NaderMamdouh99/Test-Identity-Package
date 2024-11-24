using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Test_Identity_Package.DTOS
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } 
        [DataType(DataType.Password)]
        public string Password { get; set; } 
        [DefaultValue(false)]
        public bool RemmberMe { get; set; }
    }
}
