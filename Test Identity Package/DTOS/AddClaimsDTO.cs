using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Test_Identity_Package.DTOS
{
    public class AddClaimsDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public List<string> Claims { get; set; }
        [DefaultValue(false)]
        public bool RemmberMe { get; set; }

    }
}
