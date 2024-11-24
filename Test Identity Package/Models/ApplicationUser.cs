using Microsoft.AspNetCore.Identity;

namespace Test_Identity_Package.Models
{
    public class ApplicationUser:IdentityUser<string>
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }
}
